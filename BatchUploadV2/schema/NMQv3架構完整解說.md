# BatchUpload NMQv3 架構完整解說

> 本文件基於 `Nine1.Commerce.Nmqv3.Console.NMQv3Worker` 實際程式碼整理，為**新版分散式架構**，與舊版 SCM NMQ 單一主流程不同。

## 目錄

- [架構概覽](#架構概覽)
- [Job 清單](#job-清單)
- [SCM API 端點說明](#scm-api-端點說明)
- [DynamoDB 的角色](#dynamodb-的角色)
- [完整流程（一般批次）](#完整流程一般批次)
- [完整流程（預約批次）](#完整流程預約批次)
- [狀態流轉圖](#狀態流轉圖)
- [時序圖](#時序圖)
- [各 Job 職責說明](#各-job-職責說明)
- [常見卡住原因對照](#常見卡住原因對照)

<br>

---

## 架構概覽

新版 NMQv3 將原本「單一主 NMQ 同步執行」拆成**多個獨立 Job**，每個 Job 只負責單一職責，透過 SCM API 協調進度與觸發下一步。

```
SMS / OSM
  └─ 建立 BatchUpload 記錄
  └─ 建立 BatchUploadLoadDataJob

NMQv3 Worker（各 Job 獨立執行）
  ├─ BatchUploadLoadDataJob    讀 Excel，建 BatchUploadData
  ├─ BatchUploadValidateJob    驗證資料（多輪平行）
  ├─ BatchUploadProcessJob     執行業務邏輯（多輪平行）
  ├─ BatchUploadFinishJob      階段橋接 + 最終收尾
  └─ BatchUploadAlertJob       逾時警告（定時觸發）

SCM API（orchestrator）
  ├─ POST /v2/BatchUpload/Create    啟動新執行階段
  ├─ POST /v2/BatchUpload/Start     取得執行許可（防重複）
  └─ POST /v2/BatchUpload/Finish    回報進度，完成時觸發 FinishJob

DynamoDB（進度追蹤器）
  ├─ BatchUploadCounter    TotalCount / CompletedCount / TaskLimit / Dispatched
  └─ BatchUploadTaskData   執行中 Task 記錄（防重複執行）
```

<br>

---

## Job 清單

| Job 名稱 | 觸發者 | 職責 |
|---|---|---|
| `BatchUploadLoadDataJob` | SMS / OSM | 讀 Excel → 建 BatchUploadData → 觸發 Validate 或 Process 階段 |
| `BatchUploadValidateJob` | SCM `/Create(Validate)` | 逐批驗證資料，多輪 re-queue，每輪回報進度 |
| `BatchUploadProcessJob` | SCM `/Create(Process)` | 逐批執行業務邏輯，多輪 re-queue，每輪回報進度 |
| `BatchUploadFinishJob` | SCM `/Finish`（階段完成時） | ① Validate 完成後橋接至 Process 階段<br>② Process 完成後收尾，壓最終狀態 |
| `BatchUploadAlertJob` | 排程定時觸發 | 查詢逾時批次，發送 Slack 警告 |

<br>

---

## SCM API 端點說明

### `POST /v2/BatchUpload/Create`

**啟動一個新的執行階段（Validate 或 Process）**

```
① GetDataCount()          查 WebStoreDB，計算 ReadyToValidate / ReadyToProcess 筆數
② 筆數 = 0               → 直接 CreateTaskAsync → 發動 BatchUploadFinishJob（跳過執行）
③ 筆數 > 0               → CreateItemAsync → DynamoDB 建 Counter
                           → CreateTaskAsync → 建立 TaskLimit 個平行 NMQ Job
```

| ProcessType | 建立的 NMQ Job |
|---|---|
| `Validate` | `BatchUploadValidateJob` × TaskLimit |
| `Process` | `BatchUploadProcessJob` × TaskLimit |

---

### `POST /v2/BatchUpload/Start`

**ValidateJob / ProcessJob 每輪開始前呼叫，取得執行許可**

| 回應碼 | 意義 | Job 行為 |
|---|---|---|
| `200 OK` | 正常取得許可，撈 ReadyTo* 資料 | 繼續執行 |
| `202 Accepted` | ProcessId 前次未完成 | 撈 InProcess/InValidation 資料重跑 |
| `204 No Content` | 全部已完成 | 直接 return |

---

### `POST /v2/BatchUpload/Finish`

**ValidateJob / ProcessJob 每輪結束後呼叫，更新進度**

```
UpdateItemAsync → DynamoDB CompletedCount += ProcessCount
若 CompletedCount == TotalCount 且 Dispatched == false
  → CreateTaskAsync(BatchUploadFinishJob)   觸發 FinishJob
  → DispatchItemAsync                       標記 Dispatched = true（防重複觸發）
DeleteItemAsync → 刪除 TaskData 執行記錄
```

<br>

---

## DynamoDB 的角色

NMQv3 Worker 本身**無法**直接建立 DynamoDB Counter 或建立 NMQ Task，這些都委託 SCM API 處理。

### BatchUploadCounter 表

| 欄位 | 說明 |
|---|---|
| `PK` | `{BatchUploadId}#{ProcessType}`，例如 `313934#Process` |
| `TotalCount` | 此階段待執行總筆數 |
| `CompletedCount` | 已完成筆數（每次 /Finish 累加） |
| `TaskLimit` | 最多幾個平行 Job（預設 3） |
| `Dispatched` | 是否已觸發下一步（防重複觸發 FinishJob） |

### BatchUploadTaskData 表

| 欄位 | 說明 |
|---|---|
| `PK` | `{BatchUploadId}#{ProcessType}` |
| `SK` | `ProcessId`（0, 1, 2...） |

> `/Start` 時寫入，`/Finish` 時刪除，用來防止同一 ProcessId 被執行兩次（202 Accepted 路徑）

<br>

---

## 完整流程（一般批次）

```
SMS / OSM
  └─ 建立 BatchUpload（WaitingToLoadData）
  └─ 建立 BatchUploadLoadDataJob

BatchUploadLoadDataJob
  ├─ 狀態: WaitingToLoadData → InLoadData
  ├─ 讀取 Excel → 建立 BatchUploadData（每列一筆）
  ├─ 狀態: InLoadData → InValidation
  └─ 打 /Create(Validate) → SCM 建 Counter + 建 ValidateJob ×N

BatchUploadValidateJob（多輪，每輪處理一批）
  ├─ /Start → 取得執行許可
  ├─ DoValidate(N筆) → 更新 BatchUploadData 狀態
  ├─ /Finish → SCM 累加 CompletedCount
  ├─ re-queue 自己 → 繼續下一批
  └─ 當 CompletedCount == TotalCount → SCM 觸發 FinishJob(Validate)

BatchUploadFinishJob（ProcessType=Validate，一般批次）
  └─ 打 /Create(Process) → SCM 建 Counter + 建 ProcessJob ×N
  └─ 狀態: InValidation → WaitingToProcess
  └─ return

BatchUploadProcessJob（多輪，每輪處理一批）
  ├─ 第一輪: 狀態 WaitingToProcess → InProcess
  ├─ /Start → 取得執行許可
  ├─ DoProcess(N筆) → 實際執行業務邏輯
  ├─ /Finish → SCM 累加 CompletedCount
  ├─ re-queue 自己 → 繼續下一批
  └─ 當 CompletedCount == TotalCount → SCM 觸發 FinishJob(Process)

BatchUploadFinishJob（ProcessType=Process）
  ├─ 計算成功筆數（Finish 狀態的 BatchUploadData）
  ├─ 計算失敗筆數（ValidateFailed + ProcessFailed）
  ├─ 有失敗 → 狀態: FinishWithError
  └─ 無失敗 → 狀態: Finish
```

<br>

---

## 完整流程（預約批次）

預約批次多了「等待排程時間」的橋接，`BatchUploadFinishJob` 會執行**兩次**。

```
SMS / OSM
  └─ 建立 BatchUpload（WaitingToLoadData，ScheduledDateTime 有值）
  └─ 建立 BatchUploadLoadDataJob

BatchUploadLoadDataJob
  └─ 流程同一般批次，最後打 /Create(Validate)

BatchUploadValidateJob（多輪）
  └─ 流程同一般批次，完成後 SCM 觸發 FinishJob(Validate)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
BatchUploadFinishJob（第一次，ProcessType=Validate，狀態=InValidation）
  ├─ [有驗證失敗] DoFinish() → 狀態: ScheduleAbortedWithErrors → 終態
  ├─ [無失敗，排程時間未到]
  │    ├─ 建立定時 FinishJob（設定執行時間 = ScheduledDateTime）
  │    └─ 狀態: InValidation → WaitingToProcess → return
  └─ [無失敗，排程時間已到]（直接往下走）
       └─ 打 /Create(Process) → 流程同一般批次
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

（等待排程時間到達）

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
BatchUploadFinishJob（第二次，ProcessType=Validate，狀態=WaitingToProcess）
  └─ 打 /Create(Process) → SCM 建 Counter + 建 ProcessJob ×N
  └─ return（本次不壓任何狀態）
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

BatchUploadProcessJob（多輪）
  └─ 第一輪: 狀態 WaitingToProcess → InProcess
  └─ 流程同一般批次

BatchUploadFinishJob（ProcessType=Process）
  └─ 收尾，狀態: Finish / FinishWithError
```

<br>

---

## 狀態流轉圖

```
SMS 建立
  │
  ▼
WaitingToLoadData ──── [NMQ Worker 未啟動] ──→ 卡住
  │
  │ BatchUploadLoadDataJob 開始
  ▼
InLoadData ──── [讀檔失敗/超時] ──→ FinishWithUnExpectedError（終態）
  │
  │ 讀完 Excel
  ▼
InValidation ──── [驗證中] ──→ (持續此狀態直到 ValidateJob 全部完成)
  │
  ├──→ [預約批次，驗證有失敗] ──→ ScheduleAbortedWithErrors（終態）
  │
  ├──→ [預約批次，時間未到] ──→ WaitingToProcess（等排程）
  │                                  │
  │                                  │ [排程時間到，FinishJob 第二次]
  │                                  │
  │         ┌─────────────────────────┘
  │         │
  ├─────────┘
  │ [一般批次 FinishJob] 或 [預約批次 FinishJob 第二次]
  ▼
WaitingToProcess ──── [/Create(Process) 失敗] ──→ 卡住（本次案例）
  │
  │ BatchUploadProcessJob 第一輪
  ▼
InProcess ──── [正常執行中，可能持續很長時間]
  │
  ├──→ [全部成功] ──→ Finish（終態）
  │
  └──→ [部分失敗] ──→ FinishWithError（終態）

[使用者手動取消，限 WaitingToProcess 狀態]
  └──→ ScheduleCancelled（終態）
```

<br>

---

## 時序圖

### 一般批次

```
SMS/OSM          LoadDataJob       ValidateJob×N      SCM API          ProcessJob×N      FinishJob
  │                  │                  │                │                  │                │
  │ 建立BatchUpload  │                  │                │                  │                │
  │─────────────────>│                  │                │                  │                │
  │                  │ 讀Excel           │                │                  │                │
  │                  │─────────────────>│(建BatchUploadData)                │                │
  │                  │                  │                │                  │                │
  │                  │ /Create(Validate)│                │                  │                │
  │                  │────────────────────────────────>  │                  │                │
  │                  │                  │ 建Counter+建Job │                  │                │
  │                  │                  │<───────────────│                  │                │
  │                  │                  │                │                  │                │
  │                  │              [多輪執行]            │                  │                │
  │                  │                  │ /Start         │                  │                │
  │                  │                  │───────────────>│                  │                │
  │                  │                  │ DoValidate     │                  │                │
  │                  │                  │─────────────>  │                  │                │
  │                  │                  │ /Finish        │                  │                │
  │                  │                  │───────────────>│                  │                │
  │                  │                  │ re-queue自己   │                  │                │
  │                  │                  │<───────────────│                  │                │
  │                  │              [最後一輪，CompletedCount==TotalCount]  │                │
  │                  │                  │ /Finish        │                  │                │
  │                  │                  │───────────────>│ 觸發FinishJob    │                │
  │                  │                  │                │────────────────────────────────>  │
  │                  │                  │                │                  │                │
  │                  │                  │                │ /Create(Process) │                │
  │                  │                  │                │<───────────────────────────────── │
  │                  │                  │                │ 建Counter+建Job  │                │
  │                  │                  │                │─────────────────>│                │
  │                  │                  │                │                  │                │
  │                  │                  │            [多輪執行]             │                │
  │                  │                  │                │  /Start          │                │
  │                  │                  │                │<─────────────────│                │
  │                  │                  │                │  DoProcess       │                │
  │                  │                  │                │  /Finish         │                │
  │                  │                  │                │<─────────────────│                │
  │                  │                  │                │  re-queue自己    │                │
  │                  │                  │                │─────────────────>│                │
  │                  │                  │            [最後一輪，CompletedCount==TotalCount]  │
  │                  │                  │                │  /Finish         │                │
  │                  │                  │                │<─────────────────│ 觸發FinishJob  │
  │                  │                  │                │────────────────────────────────>  │
  │                  │                  │                │                  │  DoFinish()    │
  │                  │                  │                │                  │  Finish /      │
  │                  │                  │                │                  │  FinishWithError
```

---

### 預約批次（額外的等待橋接）

```
ValidateJob×N        SCM API        FinishJob(第一次)   定時排程        FinishJob(第二次)   ProcessJob×N
     │                  │                 │                 │                  │                │
     │  [完成所有Validate]│                 │                 │                  │                │
     │  /Finish          │                 │                 │                  │                │
     │─────────────────> │ 觸發FinishJob   │                 │                  │                │
     │                   │──────────────> │                 │                  │                │
     │                   │                │ bookingTime未到  │                  │                │
     │                   │                │ 建定時FinishJob  │                  │                │
     │                   │                │────────────────> │                  │                │
     │                   │                │ 狀態→WaitingToProcess               │                │
     │                   │                │ return           │                  │                │
     │                   │                │                  │                  │                │
     │                   │            (排程時間到達)          │                  │                │
     │                   │                │                  │ 觸發FinishJob    │                │
     │                   │                │                  │─────────────────>│                │
     │                   │  /Create(Process)                 │                  │                │
     │                   │<─────────────────────────────────────────────────── │                │
     │                   │ 建Counter+建Job│                  │                  │                │
     │                   │─────────────────────────────────────────────────────────────────────>│
     │                   │                │                  │                  │ return         │
     │                   │                │                  │              [多輪執行繼續...]    │
```

<br>

---

## 各 Job 職責說明

### BatchUploadLoadDataJob

- **觸發條件**：BatchUpload 狀態 = `WaitingToLoadData`
- **主要流程**：
  1. 狀態 → `InLoadData`
  2. 讀取 Excel，產生 BatchUploadData 清單
  3. 狀態 → `InValidation`
  4. 呼叫 `/Create(Validate)` 或 `/Create(Process)`（若 `EnableValidation = false`）
- **失敗處理**：catch 未預期例外 → 狀態 → `FinishWithUnExpectedError`

---

### BatchUploadValidateJob

- **觸發條件**：SCM `/Create(Validate)` 建立，ProcessId = 0, 1, 2...
- **主要流程**：
  1. `/Start`（200=正常取許可，202=重跑前次未完成的資料）
  2. `DoValidate(N筆)`
  3. 更新 BatchUploadData 狀態
  4. 寫入錯誤訊息至 BatchUploadMessage
  5. `/Finish` 回報進度
  6. re-queue 自己（繼續下一批）
- **結束條件**：`dataCount == 0`（沒有 ReadyToValidate 資料）

---

### BatchUploadFinishJob

> 這是整個架構最關鍵的橋接 Job，有三種不同執行路徑

| 進入條件 | 執行動作 |
|---|---|
| `ProcessType=Validate`，`status=InValidation`，非預約批次 | 打 `/Create(Process)`，狀態 → `WaitingToProcess` |
| `ProcessType=Validate`，`status=InValidation`，預約批次，**時間未到** | 建定時 FinishJob，狀態 → `WaitingToProcess` |
| `ProcessType=Validate`，`status=WaitingToProcess`，預約批次，**時間已到** | 打 `/Create(Process)`，不改狀態 |
| `ProcessType=Process` | `DoFinish()`，狀態 → `Finish` 或 `FinishWithError` |
| 狀態已是終態（Finish/FinishWithError/...） | 直接 return，不做任何事 |

---

### BatchUploadProcessJob

- **觸發條件**：SCM `/Create(Process)` 建立
- **主要流程**：
  1. 第一輪：狀態 `WaitingToProcess` → `InProcess`
  2. `/Start`（取執行許可）
  3. `DoProcess(N筆)` → 實際業務邏輯
  4. 寫入錯誤訊息
  5. `/Finish` 回報進度
  6. re-queue 自己

---

### BatchUploadAlertJob

- **觸發條件**：定時排程
- **職責**：
  1. 查 DynamoDB 逾時清單（超過 10 分鐘未執行下一步）
  2. 查 WebStoreDB 逾時清單（建立超過 30 分鐘仍未完成）
  3. 從 S3 讀取 `AlertGroup.csv`（TypeDef → GroupName 對應表）
  4. 查 Redis 確認是否靜音
  5. 依群組發送 Slack 通知（`@sg-ecom`、`@sg-omo`、`@oversea_brd` 等）

<br>

---

## 常見卡住原因對照

| 狀態 | 卡住原因 | 排查方向 |
|---|---|---|
| `WaitingToLoadData` | NMQ Worker 未啟動 / 佇列塞車 | 確認 NMQ Worker 服務狀態 |
| `InLoadData` | Excel 太大 / S3 連線異常 | 確認 Filer / S3 服務 |
| `InValidation` | ValidateJob 打 API/DB 過慢 | 查 ValidateJob Log，確認下游 API 回應時間 |
| `WaitingToProcess`（一般批次） | `/Create(Process)` 打 SCM API 失敗 | 查 SCM API Log，用 ErrorId 追查 |
| `WaitingToProcess`（預約批次） | 等排程時間（正常）或 `/Create(Process)` 失敗 | 確認 `ScheduledDateTime` 是否已過期 |
| `InProcess` | ProcessJob 業務邏輯耗時過長 | 正常現象，看資料量評估 |

### 本次案例（BatchUploadId: 313934）

```
類型: BatchModifyPromotionSalePage
狀態: WaitingToProcess（卡住）

根因: BatchUploadFinishJob（第二次，預約時間到）
      打 /scm/v2/BatchUpload/Create(Process)
      → SCM API 500 InternalServerError
      → ErrorId: 4fdee8c2-4745-47ef-bbd7-5181f1cc3ed1
      → 耗時 51.8 秒才回傳錯誤

排查: 用 ErrorId 在 SCM API Log 追查 BatchUpload/Create 500 的根因
```

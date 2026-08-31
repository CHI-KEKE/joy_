# BatchUploadData 表說明

## 一句話定義

**BatchUploadData 是把 Excel 每一列拆成一筆 DB 記錄，作為批次執行的工作佇列（Work Queue）。**

---

## 主表 vs 資料表的分工

| 表 | 代表什麼 | 粒度 |
|----|---------|------|
| `BatchUpload` | 這次批次任務本身（一個檔案） | 1 個檔案 = 1 筆 |
| `BatchUploadData` | 每一列資料的執行狀態 | 1 列 = 1 筆 |

---

## 欄位說明

| 欄位 | 型別 | 用途 |
|------|------|------|
| `BatchUploadData_Id` | long | PK，每筆資料列的唯一識別 |
| `BatchUploadData_BatchUploadId` | long | FK，對應哪個主批次（BatchUpload_Id） |
| `BatchUploadData_SupplierId` | int | 所屬供應商 |
| `BatchUploadData_TypeDef` | string | 批次類型（同主表 TypeDef） |
| `BatchUploadData_StatusDef` | string | **這列目前在哪個執行階段**（見下方） |
| `BatchUploadData_Title` | string | 這列的代表名稱（如商品名稱），方便人工查詢識別 |
| `BatchUploadData_Data` | string (varchar) | **這列的完整資料，JSON 序列化後存入** |

---

## BatchUploadData_StatusDef 狀態流

```
ReadyToValidate   → 主 NMQ DoValidate 驗證中
      ↓ 驗證通過          ↓ 驗證失敗
ReadyToProcess    ValidateFailed（不再進入子 Task）
      ↓ 子 Task 執行
   ↙        ↘
Finish    ProcessFailed
```

| 狀態 | 說明 |
|------|------|
| `ReadyToValidate` | 主 NMQ LoadData 寫入後的初始狀態 |
| `ValidateFailed` | DoValidate 驗證失敗，終止於此 |
| `ReadyToProcess` | 驗證通過，等待子 Task 執行 |
| `ProcessFailed` | 子 Task DoProcess 執行失敗 |
| `Finish` | 子 Task DoProcess 執行成功 |

---

## BatchUploadData_Data 欄位的運作

### 寫入（主 NMQ，LoadData 階段）

Excel 每列被解析成對應的 Entity 物件，**序列化為 JSON 字串**存入 `Data` 欄位：

```csharp
BatchUploadData_Data = JsonConvert.SerializeObject(rowEntity);
// 例如：{"ProductId":1001,"BlackListType":"Member","MemberId":"A123456"}
```

### 讀取（子 Task，DoProcess 階段）

子 Task 撈出 `ReadyToProcess` 的記錄後，**反序列化**還原成 Entity 物件再執行業務邏輯：

```csharp
var data = JsonConvert.DeserializeObject<List<TEntity>>(batchUploadData.BatchUploadData_Data);
```

---

## BatchUploadData 在整體流程中的角色

```
使用者上傳 Excel
      ↓
主 NMQ LoadData
  → 每列 INSERT BatchUploadData
    （Status = ReadyToValidate, Data = JSON）
      ↓
主 NMQ DoValidate
  → 逐筆驗證
  → Status 改為 ReadyToProcess 或 ValidateFailed
      ↓
子 Task GetReadyToProcessDataList
  → SELECT WHERE StatusDef = 'ReadyToProcess' LIMIT N（ProcessCount）
      ↓
子 Task DoBatchProcess
  → 逐筆反序列化 Data → 執行業務邏輯
  → Status 改為 Finish 或 ProcessFailed
      ↓
子 Task 再次查詢，仍有 ReadyToProcess → re-queue 自己
無 ReadyToProcess → 主 BatchUpload 狀態更新為 Finish
```

---

## 一列 Excel ≠ 一筆 BatchUploadData

**BatchUploadData 的筆數由各 Service 的分組邏輯決定，不是 1:1 對應 Excel 列數。**

### 三種模式

#### 模式一：多列合成一筆（最常見）
以業務主體為 Key，同 Key 的多列 Excel 合併成一筆 BatchUploadData：

| Service | GroupBy Key | 意義 |
|---------|------------|------|
| `BatchCreateBlackListService` | `MemberCode` | 同一會員的所有黑名單項目 |
| `BatchAllocateCodeService` | `TradesOrderCode` | 同一訂單的所有配碼 |
| `BatchHomeDeliveryService` | `TradesOrderSlaveCode` | 同一子訂單的宅配資料 |
| `BatchDigitalDeliveryService` | `TradesOrderSlaveCode` | 同一子訂單的數位交付 |
| `BatchCrmOrderImportService` | 複合 key | 同一 CRM 訂單 |

```csharp
// 例：以 MemberCode 分組，同一會員多列合成一筆
var dataGroup = uploadList.GroupBy(i => i.MemberCode);
// BatchUploadData_Data = 該 MemberCode 所有列的 JSON 陣列
```

#### 模式二：每列各自獨立（1 Excel 列 = 1 BatchUploadData）
以每列本身的唯一識別 GroupBy，等同不合併：
```csharp
uploadList.GroupBy(x => x.SalePageId)  // 每個 SalePageId 唯一 → 1列 = 1筆
```

#### 模式三：匯出型批次（0 列 Excel = 1 筆 BatchUploadData）
匯出批次不讀 Excel，整個批次只建立 1 筆 BatchUploadData，`Data` 欄位存的是**查詢條件**而非 Excel 資料：
```json
{ "DateFrom": "2024-01-01", "DateTo": "2024-01-31", "ShopId": 123 }
```

**BatchUploadData 筆數 = Excel 資料依業務 Key GroupBy 後的分組數**

---

## 為什麼需要這個表？

**BatchUploadData 是「分批斷點續做」的核心機制。**

沒有這個表，子 Task 就不知道：
- 哪些列已經執行完（Finish）
- 哪些列還沒執行（ReadyToProcess）
- 哪些列驗證就失敗了（ValidateFailed）
- 哪些列執行過程出錯（ProcessFailed）

有了這個表，子 Task 只需要每次撈 N 筆 `ReadyToProcess` 的記錄執行，**不需要知道 Excel 的任何內容**，實現了主 NMQ（解析）與子 Task（執行）的完全解耦。

---

## 相關 DB 查詢

```sql
-- 查某次批次所有列的狀態分布
SELECT BatchUploadData_StatusDef, COUNT(*) AS cnt
FROM BatchUploadData
WHERE BatchUploadData_BatchUploadId = {BatchUpload_Id}
GROUP BY BatchUploadData_StatusDef;

-- 查某次批次失敗的列與原因
SELECT BatchUploadData_Id, BatchUploadData_Title, BatchUploadData_StatusDef, BatchUploadData_Data
FROM BatchUploadData
WHERE BatchUploadData_BatchUploadId = {BatchUpload_Id}
  AND BatchUploadData_StatusDef IN ('ValidateFailed', 'ProcessFailed');

-- 查目前還有多少列待執行
SELECT COUNT(*) AS pendingCount
FROM BatchUploadData
WHERE BatchUploadData_BatchUploadId = {BatchUpload_Id}
  AND BatchUploadData_StatusDef = 'ReadyToProcess';

-- 查某次批次的所有錯誤明細
SELECT BatchUploadMessage_Note, BatchUploadMessage_StatusDef, BatchUploadMessage_BatchUploadDataId
FROM BatchUploadMessage
WHERE BatchUploadMessage_BatchUploadId = {BatchUpload_Id}
ORDER BY BatchUploadMessage_Id;
```

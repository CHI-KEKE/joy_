# ExceededTimesLimiter 限流開關 — 四把 Config Key 完整流程說明

> 對象 Key（皆屬「三階段開關」分類）：
>
> | Repo | Config Key |
> |---|---|
> | NineYi.Scm.Api | `Prod.ExceededTimesLimiter.SalePageLite.GetMain.SupplierId` |
> | NineYi.Scm.Api | `Prod.ExceededTimesLimiter.SalePageLite.GetStock.SupplierId` |
> | NineYi.Scm.ApiV2 | `Prod.ExceededTimesLimiter.SalesOrderGetLite.Get.SupplierId` |
> | NineYi.Scm.ApiV2 | `Prod.ExceededTimesLimiter.ShippingOrder.Get.SupplierId` |

---

## 1. 兩個 Repo、對應四支 API

| API | 所屬 Repo | Controller | Action | Route |
|---|---|---|---|---|
| 商品頁內容查詢 | `NineYi.Scm.Api`（`C:\91APP\Api\SCM`） | `SalePageLiteController` | `GetMain` | `SalePage/GetMain` |
| 商品即時庫存查詢 | `NineYi.Scm.Api`（`C:\91APP\Api\SCM`） | `SalePageLiteController` | `GetStock` | `SalePage/GetStock` |
| 訂單查詢 | `NineYi.Scm.ApiV2`（`C:\91APP\SCMAPIV2`） | `SalesOrderGetLiteController` | `Get` | `SalesOrder/Get` |
| 貨運單查詢 | `NineYi.Scm.ApiV2`（`C:\91APP\SCMAPIV2`） | `ShippingOrderController` | `Get` | `ShippingOrder/Get` |

四支 API 的 Action 上皆掛有 `[ExceededTimesLimiter]` Attribute，是限流機制的**唯一進入點**。

---

## 2. Config Key 命名規則 = 執行環境前綴 + 固定組字規則

程式內部組字（`CircuitBreakerManager.cs` / `CircuitBreakerService.cs`）：

```
ExceededTimesLimiter.{ControllerName}.{ActionName}.{RecognizeType}
```

`ControllerName`、`ActionName` 是 Attribute 執行當下從 MVC/WebApi 的 `ActionDescriptor` **動態取得**（不是寫死字串），`RecognizeType` 則在 Attribute 內**寫死為 `"SupplierId"`**。

而 `GetAppSetting()`（`NineYi.Utility.AppSetting` / `IConfigService`）在讀取設定時，會**自動加上目前執行環境前綴**：

```
{Execmode}.{Key}
例如：Prod.ExceededTimesLimiter.SalePageLite.GetMain.SupplierId
```

`Execmode` 依部署環境不同為 `Dev` / `QA` / `PP` / `Prod` / `Release` 等。你提供的四把 Key 皆為 `Prod.` 前綴 → **代表這是正式環境（Production）要生效的限流設定**。

> ⚠️ 「三階段開關」是你們 Config 治理表單上對此 Key 的**分類標籤**（用來標示上版/審核流程，例如 Off → 灰度 → 全開三個佈署階段的管控方式），
> 但**程式碼本身的判斷邏輯只有單純的布林 `Enabled`**（見下方第 5 節），並沒有「三段式」百分比灰度的程式邏輯。也就是說：
> - Config 治理層面：這把 Key 走「三階段開關」的審核/上版流程管控。
> - 程式執行層面：實際只認 `Enabled=true/false`，值一旦設定為 `true`，就是**全量生效**（依 SupplierId 個別計次），沒有中間態的百分比開關。

---

## 3. Config Value 格式

```
Enabled|MaxTimes|CacheSeconds|LockedSeconds
```

| 欄位 | 說明 |
|---|---|
| Enabled | 是否啟用限流（`true`/`false`） |
| MaxTimes | 在 `CacheSeconds` 週期內，允許的最大呼叫次數 |
| CacheSeconds | 計次的時間窗口（秒），也用來做「整秒對齊」（見第 6 節） |
| LockedSeconds | 超過次數後，鎖定該 SupplierId 的秒數（此期間內直接擋掉，不論次數） |

若完全沒有設定該 Key，`GetAppSetting` 會使用程式碼內建**預設值 `"false|3|1|30"`**（即：關閉、3次/1秒、鎖30秒）。

範例（若要開啟正式環境限流）：
```
Prod.ExceededTimesLimiter.SalePageLite.GetMain.SupplierId = true|200|60|30
```
代表：對每個供應商（SupplierId），60 秒內最多可呼叫 `SalePage/GetMain` 200 次，超過則鎖定該供應商 30 秒。

---

## 4. 完整流程圖（Mermaid）

```mermaid
flowchart TD
    A[Client 呼叫 API<br/>SalePageLite.GetMain / GetStock<br/>SalesOrderGetLite.Get / ShippingOrder.Get] --> B{Request Header<br/>是否有 N1-TRACE-ID？}
    B -- 否（非經 API Gateway） --> Z1[略過檢查<br/>直接放行執行 Action]
    B -- 是 --> C[ExceededTimesLimiterAttribute<br/>OnActionExecuting]

    C --> D[取得 ControllerName / ActionName<br/>取得目前 SupplierId]
    D --> E[組成 ExceededTimesLimiterRequestEntity<br/>RecognizeType = SupplierId<br/>RecognizeKey = 目前 SupplierId]
    E --> F[呼叫 Manager/Service<br/>ExamineTryOverTimesLockApi]

    F --> G[CircuitBreakerManager/Service<br/>GetExceededTimesLimiterSettings]

    G --> H{supplierId 是否有值？}
    H -- 是 --> I[查詢 SupplierStaticSetting DB<br/>GroupName=ExceededTimesLimiter<br/>Key = Controller.Action]
    I --> J{該供應商<br/>有客製設定？}
    J -- 有 --> K[使用供應商層級設定<br/>SettingType = Supplier]
    J -- 沒有 --> L
    H -- 否 --> L[Fallback: 讀取 AppSetting<br/>Key: {Execmode}.ExceededTimesLimiter.<br/>Controller.Action.SupplierId<br/>例：Prod.ExceededTimesLimiter.SalePageLite.GetMain.SupplierId]
    L --> M[Parse 字串<br/>Enabled|MaxTimes|CacheSeconds|LockedSeconds]
    M --> N[SettingType = All<br/>找不到設定則用預設值 false|3|1|30]

    K --> O{Enabled == true？}
    N --> O

    O -- false --> P[直接 return<br/>不限流，正常執行 Action]
    O -- true --> Q[組 CacheKey：<br/>Controller:Action-版本 + SupplierId<br/>+ 依 CacheSeconds 做整秒對齊]

    Q --> R{是否已被鎖定<br/>Locked Cache 存在？}
    R -- 是 --> S[丟出 ExceededTimesLimiterException<br/>HTTP 429<br/>回應 error 訊息]
    R -- 否 --> T[Redis/Memory 計次 +1<br/>IncrBy TryTimes]

    T --> U{TryTimes > MaxTimes？}
    U -- 否 --> P
    U -- 是 --> V[設定 Locked Cache<br/>存活 LockedSeconds 秒]
    V --> S

    S --> W[Controller Action **不會執行**<br/>直接回傳 429 + JSON error]
    P --> X[正常執行 Controller Action<br/>回傳原本業務結果]
```

---

## 5. 兩個 Repo「ExceededTimesLimiter 限流機制」本身的邏輯/機制比較

這裡聚焦在**限流這套機制自己的演算法與行為**（不是兩邊技術框架的一般性差異），逐檔逐行比對後的結論如下。

### 5.1 結論先講：核心演算法「完全一致」

`NineYi.Scm.Api`（`ExceededTimesLimiterManager.cs`）與 `NineYi.Scm.ApiV2`（`ExceededTimesLimiterService.cs`）這兩支「限流判斷主邏輯」程式碼，逐行比對後除了類別/介面命名（`Manager` ↔ `Service`）與 namespace 不同以外，**演算法、參數、常數、判斷順序全部一模一樣**，屬於「同一套邏輯被複製貼上到兩個 Repo 各自維護」，不是各自獨立設計的兩套機制：

| 判斷步驟 | Api（Manager） | ApiV2（Service） | 是否一致 |
|---|---|---|---|
| 是否為 API Gateway 流量（`N1-TRACE-ID`） | 有檢查，沒有就直接放行 | 有檢查，沒有就直接放行 | ✅ 一致 |
| Enabled=false 時的行為 | 直接 `return`，不限流 | 直接 `return`，不限流 | ✅ 一致 |
| CacheKey 組成規則 | `{Controller}:{Action}-2024102801:SupplierId:{值}` | `{Controller}:{Action}-2024102801:SupplierId:{值}` | ✅ 一致（連版本戳記數字都相同） |
| 計次時間窗口對齊演算法 | `GetAdjustDateTimeByIntervalSeconds`（依 CacheSeconds 做整秒無條件捨去對齊） | 同名同邏輯函式，程式碼逐行相同 | ✅ 一致 |
| 計次方式 | Redis `IncrBy` + `Cache.DefaultExpire`（預設10800秒）過期時間 | 同左 | ✅ 一致 |
| 鎖定判斷/設定鎖定 | `IsLocked` / `SetLocked`，鎖定用 MemoryCache(1秒) + Redis(LockedSeconds) 雙層 | 同左，程式碼逐行相同 | ✅ 一致 |
| 例外拋出 | 超過次數 or 已鎖定 → `ExceededTimesLimiterException(..., NoMoreElmah)` | 同左 | ✅ 一致 |
| Redis Timeout 處理 | catch `RedisTimeoutException` → 降級放行 | 同左 | ✅ 一致 |

### 5.2 真正有差異的地方：設定來源的「取得管道」不同

限流的「行為判斷」一樣，但**取得 `Enabled/MaxTimes/CacheSeconds/LockedSeconds` 設定值的底層管道**在兩邊有實質差異：

| 項目 | NineYi.Scm.Api（`CircuitBreakerManager.cs`） | NineYi.Scm.ApiV2（`CircuitBreakerService.cs`） |
|---|---|---|
| 查詢 DB 用的連線 | **讀寫主連線**（`ERP`，未指定 ReadOnly） | **明確指定 `[WithKey("ReadOnly")]`，強制走唯讀複本**（`ERP.ReadOnly`） |
| DB Repository 對 ValidFlag 的篩選寫法 | LINQ where 手動寫 `ValidFlag == true` | 使用共用擴充方法 `.Valids()` |
| 設定快取（`ExceededTimesLimiterSettings`）的 CacheKey 版本戳記 | **無日期版號**，`TypeName = "ExceededTimesLimiterSettings"` 直接使用 | **有日期版號**，`TypeName = "ExceededTimesLimiterSettings-20200826"` |
| GetCacheKey 例外處理細緻度 | 找不到對應 type 時直接 `throw ArgumentException` | 多包一層 try/catch，額外攔截 `IndexOutOfRangeException` 並補充錯誤訊息 |
| Config Key 讀取呼叫 | `IConfigService.GetAppSetting("ExceededTimesLimiter.{Controller}.{Action}.SupplierId", "false|3|1|30")` | 呼叫方式、Key 命名規則、預設值**完全相同** |

**影響評估：**
- **DB 連線差異（主連線 vs 唯讀複本）是唯一具備實質運維意義的差異**：`NineYi.Scm.ApiV2` 查供應商客製設定時固定走唯讀複本，若 DB 有主從複寫延遲（replication lag），剛異動完的供應商設定在 ApiV2 這邊可能會有短暫的「還沒同步到唯讀庫」的延遲可見性；`NineYi.Scm.Api` 則因為走主連線，異動後立即可見。不過因為外層還有 5~10 秒的 MemoryCache/RedisCache，實務上這個差異的影響幅度遠小於快取本身造成的延遲。
- 設定快取的版本戳記（有無日期後綴）純粹是**各自程式碼上版時用來手動 cache-busting 的工程慣例**，不影響限流判斷結果。
- 兩邊的 **四把 Config Key 皆各自獨立維護**在各自服務的 `AppSettings*.config` 內（`NineYi.Scm.Api` 一份、`NineYi.Scm.ApiV2` 一份），**互不共用、也不會互相同步**——這代表你要開啟 `SalePageLite.*` 兩把 Key 要改 `NineYi.Scm.Api` 的 config，開啟 `SalesOrderGetLite.Get` / `ShippingOrder.Get` 兩把 Key 則要改 `NineYi.Scm.ApiV2` 的 config，兩邊互不影響也不會自動同步。

> 💡 **總結一句話**：兩個 Repo 的限流「機制」是同一套演算法的平行複製（行為 100% 一致），差異只在「設定來源查詢時走的 DB 連線（主 vs 唯讀）」與「快取版本戳記的工程慣例」，並不影響限流本身的邏輯正確性。

---

## 6. 資料庫查詢明細（DB / Table / Column）

### 6.1 資料庫與資料表

| 項目 | 內容 |
|---|---|
| 資料庫（邏輯名稱） | **ERPDB**（Connection String Key：`ERP` / `ERP.ReadOnly`，實際連線字串在各環境的 `Web.config` / `AppSettings.config` 設定） |
| Schema 定義來源 | `NineYi.Scm.Api`：`SCM\DA\ERPDB\Tables\ERPDBModel.edmx`（EF6 Model-First）／`NineYi.Scm.ApiV2`：`DataAccess\ErpDB\Tables\ErpDbContext.cs`（Code-based） |
| 資料表 | **`SupplierStaticSetting`**（供應商靜態設定表） |
| 用途 | 儲存以「供應商（SupplierId）」為單位的各種功能開關/客製設定，`ExceededTimesLimiter` 只是眾多 `GroupName` 分類中的其中一種用途 |

### 6.2 `SupplierStaticSetting` 資料表欄位

| Column | 型別 | 說明 |
|---|---|---|
| `SupplierStaticSetting_Id` | bigint（PK, Identity） | 主鍵 |
| `SupplierStaticSetting_SupplierId` | int | 供應商序號（對應限流機制的 SupplierId） |
| `SupplierStaticSetting_GroupName` | varchar(30) | 設定群組名稱 → 限流機制固定查 **`"ExceededTimesLimiter"`** |
| `SupplierStaticSetting_Key` | varchar(50) | 設定鍵名 → 限流機制查 **`{ControllerName}.{ActionName}`**（超過50字元會被截斷取前50碼） |
| `SupplierStaticSetting_Value` | nvarchar(3000) | 設定值 → 格式為 `RecognizeType=Enabled\|MaxTimes\|CacheSeconds\|LockedSeconds`，多筆設定用 `;` 分隔（例如同時有 SupplierId、ShopId 等不同 RecognizeType） |
| `SupplierStaticSetting_Description` | nvarchar(300) | 設定說明（人工填寫用途） |
| `SupplierStaticSetting_CreatedDateTime` / `CreatedUser` | datetime / varchar(50) | 建立時間/建立人 |
| `SupplierStaticSetting_UpdatedTimes` | tinyint | 更新次數 |
| `SupplierStaticSetting_UpdatedDateTime` / `UpdatedUser` | datetime / varchar(50) | 最後更新時間/更新人 |
| `SupplierStaticSetting_ValidFlag` | bit | 是否有效（限流查詢只撈 `ValidFlag = true` 的資料，等同軟刪除機制） |
| `SupplierStaticSetting_Rowversion` | rowversion(timestamp) | 併發控制用版本戳記 |

### 6.3 實際查詢條件（SQL 語意等價）

```sql
SELECT TOP 1 SupplierStaticSetting_Value
FROM SupplierStaticSetting
WHERE SupplierStaticSetting_SupplierId = @supplierId
  AND SupplierStaticSetting_GroupName  = 'ExceededTimesLimiter'
  AND SupplierStaticSetting_Key        = @ControllerName + '.' + @ActionName  -- 例：'SalePageLite.GetMain'
  AND SupplierStaticSetting_ValidFlag  = 1;
```

取得 `Value` 後，程式會用 `;` 切開多筆設定，再找出以 `SupplierId=` 開頭的那一段（因為同一個 Key 底下可能同時存放多種 `RecognizeType` 的設定），Parse 成 `Enabled|MaxTimes|CacheSeconds|LockedSeconds`。

### 6.4 與四把 Config Key 的關係

- 這張 DB 表是**供應商層級的個別覆蓋設定**，優先序高於你問的四把全域 `Prod.ExceededTimesLimiter.*.SupplierId` Config Key。
- 若某供應商想要跟全域設定不同的限流門檻（例如特定大供應商需要更高的 MaxTimes），會在這張表新增一筆 `GroupName='ExceededTimesLimiter'`、`Key='SalePageLite.GetMain'` 的資料，而不需要更動全域 Config。
- 若查無資料（`FirstOrDefault()` 回傳 `null`），才會 fallback 回本文四把 Config Key 走全域規則。

### 6.5 實際資料範例解析

你提供的實際資料列（`SupplierStaticSetting` 表）：

| Column | 值 |
|---|---|
| `SupplierStaticSetting_Id` | 1201 |
| `SupplierStaticSetting_SupplierId` | **1614** |
| `SupplierStaticSetting_GroupName` | `ExceededTimesLimiter` |
| `SupplierStaticSetting_Key` | `SalesOrderGetLite.Get` |
| `SupplierStaticSetting_Value` | `SupplierId=true\|200\|1\|0` |
| `SupplierStaticSetting_Description` | OSM API /V2/SalesOrder/Get 限速器 |
| `SupplierStaticSetting_CreatedDateTime` / `CreatedUser` | 2025-09-18 14:57:02.867 / VSTS528216 |
| `SupplierStaticSetting_UpdatedTimes` | 1 |
| `SupplierStaticSetting_UpdatedDateTime` / `UpdatedUser` | 2025-10-22 15:44:35.090 / VSTS528216 |
| `SupplierStaticSetting_ValidFlag` | 1（有效） |
| `SupplierStaticSetting_Rowversion` | 0x00000010A9106771 |

**逐步拆解程式怎麼解讀這筆資料：**

1. 這是針對**供應商 SupplierId=1614** 的**個別覆蓋設定**（`GroupName='ExceededTimesLimiter'` + `Key='SalesOrderGetLite.Get'`），對應到 `SalesOrderGetLiteController.Get` 這支 API。
2. `Value` 欄位內容 `SupplierId=true|200|1|0`：
   - 因為程式會用 `;` 切開多筆設定，再找出以 `{RecognizeType}=` 開頭（此處 `RecognizeType` 固定為 `"SupplierId"`）的那段，這裡只有一段，前綴剛好是 `SupplierId=`，取出後半段 `true|200|1|0` 交給 `ExceededTimesLimiterSettingsEntity.Parse()`。
   - Parse 後對應：`Enabled=true`、`MaxTimes=200`、`CacheSeconds=1`、`LockedSeconds=0`。
3. **實際行為意義**：
   - `Enabled=true` → 此供應商的這支 API **有開啟限流**。
   - `MaxTimes=200`、`CacheSeconds=1` → 在**每 1 秒**的時間窗口內，最多允許呼叫 **200 次**。
   - `LockedSeconds=0` → 回頭看程式邏輯 `if (settings.LockedSeconds > 0 && this.IsLocked(...))` 與 `if (settings.LockedSeconds > 0) { SetLocked(...) }`：**當 `LockedSeconds` 為 0（或負數）時，鎖定機制完全不會被觸發**。也就是說：
     - 超過 200 次/秒時，該次請求**仍會被擋下並回傳 429**（`tryTimes > MaxTimes` 這個判斷不受 LockedSeconds 影響）。
     - 但**不會額外設定「鎖定」**，鎖定 cache 不會被寫入 → 下一個 1 秒時間窗口一到，計數器歸零重新累計，**不會有額外的鎖定延長期**。
   - 換句話說，這是一個**「純粹瞬間流量閥門」**的設定：僅擋掉單一秒內超過 200 次的部分請求，不會像 `LockedSeconds > 0` 的設定那樣把供應商「多鎖一段時間」。
4. **與全域 Config Key 的關係**：因為 `SupplierId=1614` 在 DB 已有此筆有效資料（`ValidFlag=1`），`CircuitBreakerService.GetExceededTimesLimiterSettings` 會**優先採用這筆 DB 設定**，**完全不會**去讀取全域 `Prod.ExceededTimesLimiter.SalesOrderGetLite.Get.SupplierId` 這把 Config Key（除非未來這筆 DB 資料被下架/`ValidFlag` 改為 0，才會 fallback 回全域 Key）。
5. `UpdatedTimes=1`、`UpdatedDateTime` 晚於 `CreatedDateTime` → 顯示這筆設定**建立後曾被異動過一次**（例如原本門檻不同，後來調整為目前的 `200|1|0`）。

---

## 7. 關鍵程式節點對照表

| 步驟 | NineYi.Scm.Api（SalePageLite） | NineYi.Scm.ApiV2（SalesOrderGetLite / ShippingOrder） |
|---|---|---|
| Attribute 進入點 | `SCM\Api\WebApi\Extentions\ActionFilters\ExceededTimesLimiterAttribute.cs` | `WebSite\WebSite\Extensions\ActionFilters\ExceededTimesLimiterAttribute.cs` |
| 業務邏輯層 | `IExceededTimesLimiterManager` / `ExceededTimesLimiterManager.cs` | `IExceededTimesLimiterService` / `ExceededTimesLimiterService.cs` |
| 設定來源判斷 | `ICircuitBreakerManager` / `CircuitBreakerManager.cs` | `ICircuitBreakerService` / `CircuitBreakerService.cs` |
| 設定 Entity/Parse | `SCM\Api\BE\ExceededTimesLimiter\ExceededTimesLimiterSettingsEntity.cs` | `BusinessLogic\BE\ExceededTimesLimiter\ExceededTimesLimiterSettingsEntity.cs` |
| Config 讀取 | `IConfigService.GetAppSetting("ExceededTimesLimiter.{Controller}.{Action}.SupplierId", "false|3|1|30")` | 同左 |
| 例外/回應 | 429 + `{ error: message }`（MVC `JsonResult`） | 429（WebApi `CreateErrorResponse`） |
| 供應商客製設定 | `SupplierStaticSettingManager`（DB: `SupplierStaticSetting` 表，GroupName=`ExceededTimesLimiter`） | 同左（ApiV2 版本） |

---

## 8. 判斷細節補充

1. **只認 API Gateway 流量**：Header 沒有 `N1-TRACE-ID` 直接略過整個限流檢查（表示內部直呼、非透過 Gateway 進來的流量不受限）。
2. **設定優先序**：供應商個別設定（DB）> Config Key（AppSetting，即你這四把 Key）> 程式碼內建預設值 `false|3|1|30`。
3. **CacheKey 組成**：`ExceededTimesLimiterManager(或Service):{Controller}:{Action}-{版本號}:SupplierId:{實際SupplierId值}:{整秒對齊時間}`，確保不同供應商、不同時間窗口互不干擾。
4. **鎖定機制**：一旦某供應商在時間窗口內超過 `MaxTimes`，會額外設一個 `Locked` cache，鎖定 `LockedSeconds` 秒——這段期間**不論次數多寡都直接擋**，不會重新計次。
5. **快取層**：使用 Redis（`ICacheProvider`）搭配 In-Process `MemoryCache` 雙層快取，減少 Redis 壓力。
6. **例外處理**：若 Redis Timeout（`RedisTimeoutException`），視為降級放行，**不會**因為快取服務異常而誤擋合法請求。

---

## 9. 目前狀態結論（依你提供的四把 Key 開關資訊）

若這四把 `Prod.ExceededTimesLimiter.*.SupplierId` 已在正式環境設定為 `true|MaxTimes|CacheSeconds|LockedSeconds`，則：

- ✅ 對應的四支 API（`SalePageLite.GetMain`、`SalePageLite.GetStock`、`SalesOrderGetLite.Get`、`ShippingOrder.Get`）在正式環境（Prod）會依 SupplierId 個別限流。
- ⚠️ 若供應商在 `SupplierStaticSetting` 資料表另有客製設定，會**優先蓋過**這把全域 Config（依第 6 節優先序）。
- ⚠️ 若這把 Key 尚未設定 or 設為 `false|...`，則該 API 目前**不會限流**，Action 永遠正常執行。

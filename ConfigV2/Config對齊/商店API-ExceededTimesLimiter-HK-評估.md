# ExceededTimesLimiter 限流開關 — 精簡版說明

> 聚焦於「Key 對照」「命名規則」「Value 格式」「HK Prod 套用評估」四個既有主題，並新增「實際案例：不同情境下怎麼回應呼叫端」章節。

---

## 1. 四把 Key 對照四支 API

| Repo / Controller.Action | Config Key |
|---|---|
| `NineYi.Scm.Api` · `SalePageLiteController.GetMain` | `Prod.ExceededTimesLimiter.SalePageLite.GetMain.SupplierId` |
| `NineYi.Scm.Api` · `SalePageLiteController.GetStock` | `Prod.ExceededTimesLimiter.SalePageLite.GetStock.SupplierId` |
| `NineYi.Scm.ApiV2` · `SalesOrderGetLiteController.Get` | `Prod.ExceededTimesLimiter.SalesOrderGetLite.Get.SupplierId` |
| `NineYi.Scm.ApiV2` · `ShippingOrderController.Get` | `Prod.ExceededTimesLimiter.ShippingOrder.Get.SupplierId` |

| API | 所屬 Repo | 對外 Route |
|---|---|---|
| 商品頁內容查詢 | `C:\91APP\Api\SCM` | `SalePage/GetMain` |
| 商品即時庫存查詢 | `C:\91APP\Api\SCM` | `SalePage/GetStock` |
| 訂單查詢 | `C:\91APP\SCMAPIV2` | `SalesOrder/Get` |
| 貨運單查詢 | `C:\91APP\SCMAPIV2` | `ShippingOrder/Get` |

> **備註：** Route（對外網址）與 Config Key 用的 `ControllerName` 是兩件事——前者由 `[RoutePrefix]/[Route]` 決定，後者由「類別名稱去掉 Controller 後綴」決定，兩者互相獨立，因此 `SalesOrder/Get` 對應到的 Key 是 `SalesOrderGetLite.Get`（實際執行的類別是 `SalesOrderGetLiteController`）。

---

## 2. Config Key 命名規則

程式內部固定組字規則：

```
ExceededTimesLimiter.{ControllerName}.{ActionName}.{RecognizeType}
```

`ControllerName`／`ActionName` 由 Attribute 執行當下動態取得；`RecognizeType` 目前寫死為 `"SupplierId"`。讀取設定時會自動補上環境前綴：

```
{Execmode}.{Key}
例：Prod.ExceededTimesLimiter.SalePageLite.GetMain.SupplierId
```

> **「三階段開關」只是 Config 治理標籤：** 管控上版審核流程（Off→灰度→全開），但程式碼本身只認單純布林 `Enabled`，一旦設為 `true` 即為**全量生效**，沒有內建百分比灰度邏輯。

---

## 3. Config Value 格式

```
Enabled|MaxTimes|CacheSeconds|LockedSeconds
```

| 欄位 | 說明 |
|---|---|
| Enabled | 是否啟用限流（true / false） |
| MaxTimes | CacheSeconds 週期內允許的最大呼叫次數 |
| CacheSeconds | 計次時間窗口（秒），同時用於整秒對齊分桶（固定窗口計數器，非滑動窗口） |
| LockedSeconds | 超過次數後鎖定該 SupplierId 的秒數（0＝不啟用鎖定層） |

找不到設定時，程式使用內建預設值：`"false|3|1|30"`（關閉、3次/1秒、鎖30秒）。

目前 TW／OR Prod 實際套用值：`true|200|1|0` — 每個供應商每 1 秒最多 200 次，不啟用鎖定層。

---

## 4. HK Prod Config 套用評估（實際 IIS Log 數據驗證）

方法：以 Athena SQL 查詢 `hk_prod_osm.osm_api_iislog`，針對 `SalePage/GetMain`，依「日期＋秒級時間（`time` 欄位本身精確到秒）＋供應商 Token」分組計數，取近 3.5 個月（2026/05/01~2026/08/14）最高的 10 筆，藉此還原「某供應商在某一整秒內實際打了幾次」，可直接對照 `MaxTimes=200／CacheSeconds=1` 門檻，不需估算換算。

```sql
SELECT
    date,
    time,
    regexp_extract(cs_uri_query, '(^|&)t=([^&]+)', 2) AS token,
    COUNT(*) AS request_count
FROM "hk_prod_osm"."osm_api_iislog"
WHERE cs_uri_stem = '/scm/V1/SalePage/GetMain'
  AND date > '2026/05/01'
  AND date < '2026/08/14'
GROUP BY regexp_extract(cs_uri_query, '(^|&)t=([^&]+)', 2), date, time
ORDER BY request_count DESC
LIMIT 10;
```

**查詢邏輯重點：** 用 `regexp_extract` 從 `cs_uri_query` 這個 Log 欄位裡把驗證用的 `t`（Token，識別供應商）參數抓出來，依「日期＋秒級時間＋Token」分組計數，還原「某供應商在那一整秒內實際打了幾次」——與限流器 `CacheSeconds=1` 窗口的統計粒度完全對齊（1:1 對應，非估算）。

**查詢結果：**

| 日期 | 時間（秒級） | Token | 當秒請求數 |
|---|---|---|---|
| 2026/06/24 | 19:59:36 | 30671037 | 277 |
| 2026/06/24 | 19:58:36 | 30671037 | 265 |
| 2026/06/24 | 19:56:36 | 30671037 | 254 |
| 2026/06/24 | 19:59:37 | 30671037 | 241 |
| 2026/06/24 | 20:00:37 | 30671037 | 236 |
| 2026/06/24 | 19:57:37 | 30671037 | 232 |
| 2026/06/24 | 19:57:36 | 30671037 | 212 |
| 2026/06/24 | 19:55:36 | 30671037 | 199 |
| 2026/06/24 | 19:54:37 | 30671037 | 176 |
| 2026/07/13 | 06:32:48 | 30671037 | 173 |

Token `30671037` ＝「香港莎莎網店」，是唯一在近 3.5 個月內出現過高頻呼叫的供應商。

> **結論：** 若 HK Prod 套用與 TW 相同參數（`true|200|1|0`），近 3.5 個月內約有 **8 個秒級樣本** 會實際超過或逼近 200 門檻（集中在 2026/06/24 晚間 19:54~20:00 這 6 分鐘），僅「香港莎莎網店」這一個供應商會被限流收到 429，其餘供應商不受影響 — 屬低頻率、可控的偶發風險。建議：上線前確認該供應商呼叫端有 retry 機制、或改用 DB `SupplierStaticSetting` 個別設定較寬鬆的 `MaxTimes`，並於上線後觀察監控紀錄。

---

## 5. 實際案例：不同情境下，限流器怎麼拿設定、回給呼叫端什麼

以 `SalePageLite.GetMain`（Token=30671037，SupplierId 對應之供應商）為例，說明 `ExceededTimesLimiterAttribute.OnActionExecuting` 攔截點在三種情境下的實際行為與回應內容。

**前置條件：只攔截透過 API Gateway 的請求**

Attribute 一開始會檢查 Request Header 是否帶有 `N1-TRACE-ID`；**沒帶這個 Header 的請求（例如內部直連）完全不受限流器管控**，直接放行進入 Controller 正常執行。以下三種情境皆假設請求有帶此 Header。

### 情境 A：正常呼叫（未超過門檻）

**設定：** `Prod.ExceededTimesLimiter.SalePageLite.GetMain.SupplierId = true|200|1|0`

1. 取得設定：DB `SupplierStaticSetting` 查無此 SupplierId 的個別設定 → fallback 到 Config，取得 `Enabled=true, MaxTimes=200, CacheSeconds=1, LockedSeconds=0`。
2. `Enabled=true`，繼續往下執行。組出當前 1 秒窗口的 Cache Key（如 `SalePageLite:GetMain-2024102801:SupplierId:30671037:20260624195936`），對 Redis `IncrBy(1)`，例如回傳 `tryTimes=120`。
3. `LockedSeconds=0` → 略過鎖定檢查；`tryTimes(120) <= MaxTimes(200)` → 不拋例外。
4. **回應呼叫端：** Attribute 直接 `return`，請求正常放行進入 Controller Action，依原本業務邏輯回傳 **HTTP 200** + 正常商品資料 JSON。呼叫端完全無感。

### 情境 B：Config 設為 Enabled=false（限流關閉）

**設定：** `Prod.ExceededTimesLimiter.SalePageLite.GetMain.SupplierId = false|200|1|0`（或該 Key 根本不存在，走內建預設值 `false|3|1|30`）

1. 取得設定：`Enabled=false`。
2. `ExamineTryOverTimesLockApi` 內部第一行判斷 `if (settings.Enabled == false) { return; }` — **連 Redis `IncrBy` 計次都不會執行**，等於完全不計數、不檢查。
3. **回應呼叫端：** 無論實際呼叫頻率多高（即使 1 秒內打了 1000 次），都不會被攔截，全部正常進入 Controller，回傳 **HTTP 200**。這也是目前 HK／MY／PX Prod 的實際現況（4 把 Key 皆未設定，等同此情境）。

### 情境 C：Config 已開啟且實際超過 MaxTimes 上限

**設定：** `Prod.ExceededTimesLimiter.SalePageLite.GetMain.SupplierId = true|200|1|0`；情境對應 2026/06/24 19:59:36 這一秒內，香港莎莎網店（Token 30671037）實際發出第 201 次以後的請求（該秒總計 277 次）。

1. 取得設定同情境 A：`Enabled=true, MaxTimes=200, CacheSeconds=1, LockedSeconds=0`。
2. 同一秒窗口 Cache Key 已被前面 200 次請求陸續 `IncrBy` 累加，第 201 次呼叫時 `IncrBy(1)` 回傳 `tryTimes=201`。
3. `LockedSeconds=0` → 略過鎖定檢查（不會額外鎖定，下一秒視窗重置後即可恢復正常）；`tryTimes(201) > MaxTimes(200)` → 拋出 `ExceededTimesLimiterException`。
4. Attribute 的 `catch (ExceededTimesLimiterException ex)` 攔截到後，直接組出：
   ```csharp
   actionContext.Response = actionContext.Request.CreateErrorResponse(
       (HttpStatusCode)429, ex.Message);
   ```
5. **回應呼叫端：**
   - HTTP 狀態碼：**429 Too Many Requests**
   - Body：`ExceptionSource:SalePageLite.GetMain (SupplierId:30671037) TryTimes:201`

   此請求**完全不會進入 Controller Action**（不查 DB、不跑業務邏輯），在 ActionFilter 階段就被短路攔截並直接回應。該秒內第 202~277 次的請求也會依同樣邏輯逐一收到 429，直到下一秒新窗口 Key 產生、計數歸零為止。

> **額外容錯：** 若限流器內部呼叫 Redis 逾時（`RedisTimeoutException`），Attribute 會直接 `return` 放行，不擋請求 — 避免限流元件本身故障時反而造成正常流量中斷（Fail-Open 設計）。

---

*精簡版摘要 — 完整版請參閱 `ExceededTimesLimiter-流程說明.html`*

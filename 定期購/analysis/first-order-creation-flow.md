# 首單建立機制總覽

> 分析日期：2026-05-21  
> 範圍：mweb 結帳 → order_create_queue → regular_order 建立完成

---

## 一、全貌鏈路圖

```
用戶在 mweb 前台結帳（含定期購商品）
        ↓
[mweb] 付款成功
        ↓
[mweb] CreateTradesOrderEventProcessor
  → PublishEventService.FireOrderCreatedEvent(tgCode)
  → 建立 NMQ Task: "CreateEvent"（EventName: "OrderCreated"）
        ↓
[nineyi.scm.nmqv2] RegularOrderCreatedProcess
  → 查 ShopDefault: RegularOrderProvider
  → GameBall → 呼叫 PHP POST /api/NewOrder
  → Nine1  → 跳過（未來 .NET Core 服務負責）
        ↓
[PHP 定期購服務] /api/NewOrder
  → 防重複驗證（X-API-Key + TGCode 去重）
  → 寫入 order_create_queue（ocq_status = 0）
        ↓
[Kubernetes CronJob] create_order_housekeeper（每 1 分鐘）
  → 用 UUID token 鎖定最多 100 筆待處理 queue（先進先出）
  → 對每筆 queue：
      1. 呼叫 91APP V2/SalesOrder/Get 取得訂單完整資料
      2. 呼叫 91APP V2/RegularOrder/Get 取得定期購設定
      3. 建立 regular_order（主計畫）
      4. 建立 regular_order_detail（第 1 期明細）
  → 更新 ocq_status = 1（成功）/ 8（失敗）
```

---

## 二、觸發條件

| 層次 | 觸發方式 |
|------|---------|
| mweb 結帳 | 用戶完成付款，`CreateTradesOrderEventProcessor` 發出 `OrderCreated` 事件 |
| NMQ Job | 監聽 `OrderCreated` 事件，自動觸發 `RegularOrderCreatedProcess` |
| PHP API | NMQ Job HTTP 呼叫 `/api/NewOrder` |
| CronJob | Kubernetes 每分鐘定時觸發，主動消費 queue |

---

## 三、各層職責分工

| 元件 | 職責 | 技術 |
|------|------|------|
| mweb `CreateTradesOrderEventProcessor` | 付款後發出 `OrderCreated` 事件 | .NET (mweb) |
| scmnmq `RegularOrderCreatedProcess` | 訂閱事件 → 判斷 Provider → 呼叫 PHP API | .NET NMQ Worker |
| PHP `/api/NewOrder` | 接收通知 → 防重複 → 寫 queue | PHP |
| `create_order_housekeeper` | 消費 queue → 建計畫 | PHP CronJob（每 1 分鐘）|

---

## 四、防重複機制（三道關卡）

| 層次 | 防重複方式 |
|------|-----------|
| NMQ 事件 | `IdempotencyKey = "TG-{TGCode}"`，NMQ 框架層去重 |
| PHP API `/api/NewOrder` | ① 查 `order_create_queue` 是否已有相同 `ocq_tgid`（TGCode）② 查 `regular_order_detail` 是否已有相同 TG+TS 組合 |
| CronJob 消費 | `ocq_occupy_id` 機制：先用 UPDATE 原子鎖定一批，避免多個 instance 重複取同一筆 |

---

## 五、重要邊界條件

- **定期購識別**：`SalesOrderSlave_ShippingTypeDef == 7` 才是定期購子單，其他一律跳過
- **Provider 判斷**：`ShopDefault.RegularOrderProvider == "GameBall"` 才呼叫 PHP；未來切換 `"Nine1"` 後走新 .NET Core 路徑
- **`InternalSchedule` 防循環**：後續每期自動建的訂單帶 `TrackSourceTypeDef = "InternalSchedule"`，RegularOrderCreatedProcess 遇到此類直接跳過，不再觸發 PHP

---

## 六、關鍵檔案索引

| 角色 | 路徑 |
|------|------|
| NMQ Job | `C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\RegularOrders\RegularOrderCreatedProcess.cs` |
| PHP API 入口 | `C:\91APP\regular\n1-regularpurchase-nkp\src\...\api\webapi.php`（`new_order()` function）|
| PHP Queue Model | `C:\91APP\regular\n1-regularpurchase-nkp\src\...\models\order_create_queue.php` |
| PHP CronJob | `C:\91APP\regular\n1-regularpurchase-nkp\src\...\admin\daemon\create_order_housekeeper.php` |

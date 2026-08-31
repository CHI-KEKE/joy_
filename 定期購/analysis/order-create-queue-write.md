# order_create_queue — 寫入內容說明

> 分析日期：2026-05-21  
> 資料表：`order_create_queue`  
> 寫入來源：PHP `/api/NewOrder`（`webapi.php` → `new_order()` function）

---

## 一、資料表結構

| 欄位 | 型別 | 說明 |
|------|------|------|
| `ocq_pk` | int PK | 自動遞增主鍵 |
| `ocq_tgid` | varchar | TG 主單編號（如 `TG202501010001`）|
| `ocq_txid` | varchar | 本次請求的 TxId（UUID，全小寫，由 API 層產生）|
| `ocq_data` | text | 整個 Request Body 的 JSON 字串 |
| `ocq_occupy_id` | varchar | 消費者鎖定 token（空字串 = 未被取走）|
| `ocq_status` | tinyint | 狀態（0=待處理 / 1=成功 / 8=失敗）|
| `ocq_create_time` | datetime | 寫入時間（台灣時間）|

---

## 二、寫入時機與觸發來源

```
NMQ Job RegularOrderCreatedProcess
  → HTTP POST /api/NewOrder
       ↓
PHP new_order()
  → 防重複檢查通過後
  → INSERT INTO order_create_queue
```

---

## 三、防重複機制（寫入前的兩道檢查）

```php
// 檢查 1：同一 TGCode 是否已在 queue 中
if ($regular_queue->load_by_tg_code($tg_code)) {
    throw new Exception("Duplicated order.", 10002);
}

// 檢查 2：同一 TG+TS 組合是否已建過 regular_order_detail
foreach ($ts_list as $ts_code) {
    if ($regular_detail->load_by_tg_and_ts($tg_code, $ts_code)) {
        throw new Exception("Duplicated order.", 10003);
    }
}
```

| 檢查項目 | 防止的情況 |
|---------|-----------|
| `ocq_tgid` 已存在 | NMQ retry 重複呼叫 `/api/NewOrder` |
| TG+TS 已建立 `regular_order_detail` | Queue 已被消費過但再次呼叫 |

---

## 四、`ocq_data` 的內容（完整 Request Body JSON）

```json
{
  "ShopId": 1204,
  "TGCode": "TG202501010001",
  "TSCodeList": [
    "TS202501010001-1"
  ],
  "MemberMobile": "0912345678"
}
```

| 欄位 | 說明 |
|------|------|
| `ShopId` | 商店序號 |
| `TGCode` | 主單編號（OrderCreated 事件帶來）|
| `TSCodeList` | 定期購子單編號清單（`ShippingTypeDef == 7` 的 TS）|
| `MemberMobile` | 會員手機號碼（從 VipMemberService 取得）|

> **重要：** `ocq_data` 只是入場券。實際的訂單詳細資料（收件人、配送方式、商品設定）在消費時再向 91APP API 即時查詢取得。

---

## 五、狀態流轉

```
INSERT 時：ocq_status = 0, ocq_occupy_id = ''
        ↓
取走時：ocq_occupy_id = '{UUID token}'（仍 status = 0）
        ↓
處理成功：ocq_status = 1
處理失敗：ocq_status = 8
```

| `ocq_status` | 意義 |
|-------------|------|
| `0` | 待處理（含已被鎖定但尚未完成的）|
| `1` | 已成功處理 |
| `8` | 處理失敗（可觀察但不自動重試）|

---

## 六、關鍵檔案

| 角色 | 路徑 |
|------|------|
| 寫入邏輯 | `C:\91APP\regular\n1-regularpurchase-nkp\src\...\api\webapi.php`（`new_order()` L94-136）|
| Model | `C:\91APP\regular\n1-regularpurchase-nkp\src\...\models\order_create_queue.php` |
| Base Model（SQL）| `C:\91APP\regular\n1-regularpurchase-nkp\src\...\models\base\order_create_queue_base.php` |

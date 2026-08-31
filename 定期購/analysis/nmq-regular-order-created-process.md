# RegularOrderCreatedProcess — NMQ Job 詳細說明

> 分析日期：2026-05-21  
> 專案：`nineyi.scm.nmqv2`  
> 檔案：`SCM\Frontend\NMQV2\RegularOrders\RegularOrderCreatedProcess.cs`

---

## 一、定位

這支 NMQ Job 是 **GameBall 定期購首單通知的橋接器**：

- **觸發來源**：`OrderCreated` 事件（mweb 結帳完成後發出）
- **核心任務**：判斷該訂單是否為定期購、判斷 Provider，決定是否通知 PHP 服務

> 注意：這支 Job **不建立任何訂單**，只做判斷與通知。

---

## 二、全貌流程

```
NMQ Task: "CreateEvent"（EventName: "OrderCreated"）
        ↓
DoJob(taskInfo)
        ↓
Step 1: 解析 NMQ Task Data
  taskInfo.Data → FireEventExtRequest
  fireEventRequest.Source → OrderCreatedSourceEntity
  取得：ShopId、OrderCode（TGCode）
        ↓
Step 2: 查詢 WebStoreDB 訂單資料
  salesOrderService.GetGroup(TGCode)           → SalesOrderGroup
  salesOrderService.GetMasterList(groupId)     → SalesOrder 清單
  salesOrderService.GetSalesOrderSlaveList()   → SalesOrderSlave 清單
  .Where(ShippingTypeDef == 7)                 → 篩選定期購子單（TSCodeList）
        ↓
Step 3: 判斷是否需要通知（分支決策 ↓）
        ↓
Step 4: （若需通知）呼叫 PHP /api/NewOrder
  取得 RegularOrder.Url + RegularOrder.Api.Key
  取得 MemberMobile（vipMemberService.GetVipMemberInfoItem）
  POST {url}/api/NewOrder
    Header: x-api-key: {apiKey}
    Body: { ShopId, TGCode, TSCodeList, MemberMobile }
  檢查 response.StatusCode == 200
```

---

## 三、分支決策表

| 條件 | Log 訊息 | 行為 |
|------|----------|------|
| 沒有 `ShippingTypeDef == 7` 的 TS | 「無定期購商品，不須通知」 | 直接結束 |
| 有定期購 TS，`RegularOrderProvider == "Nine1"` | 「舊版定期購商品，不須通知」 | 跳過（未來 .NET Core 自己處理）|
| 有定期購 TS，`TrackSourceTypeDef == "InternalSchedule"` | 「系統排程建單版定期購商品，不須通知」 | 跳過（防止循環觸發）|
| 有定期購 TS，以上條件都不符合 | — | **呼叫 `/api/NewOrder`** |

---

## 四、呼叫 PHP 的 Request 格式

```http
POST {RegularOrder.Url}/api/NewOrder
Content-Type: application/json
x-api-key: {RegularOrder.Api.Key}
```

```json
{
  "ShopId": 1204,
  "TGCode": "TG202501010001",
  "TSCodeList": ["TS202501010001-1"],
  "MemberMobile": "0912345678"
}
```

| 欄位 | 來源 |
|------|------|
| `ShopId` | `OrderCreatedSourceEntity.ShopId` |
| `TGCode` | `OrderCreatedSourceEntity.OrderCode` |
| `TSCodeList` | WebStoreDB 查詢後篩選 `ShippingTypeDef == 7` 的 TS 編號清單 |
| `MemberMobile` | `VipMemberService.GetVipMemberInfoItem(shopId, memberId)` |

---

## 五、例外處理

| 例外類型 | 發生時機 | 行為 |
|----------|----------|------|
| `ApplicationException`（解析失敗）| taskInfo.Data 無法轉型 | throw → NMQ retry |
| `ApplicationException`（Source 解析失敗）| fireEventRequest.Source 格式錯誤 | throw → NMQ retry |
| `ApplicationException`（通知失敗）| response.Status != "Success" | throw → NMQ retry |
| `ApplicationException`（HTTP 非 200）| response.StatusCode != 200 | throw → NMQ retry |
| 其他 `Exception` | 未預期例外 | Logger.Error → rethrow |

`OnError()` 方法目前為空，所有錯誤均靠 NMQ 框架 retry。

---

## 六、翻寫 Nine1 版本的影響

當新版 .NET Core 定期購服務上線後：

1. 商店的 ShopDefault 由 `"GameBall"` 切換為 `"Nine1"`
2. 此 NMQ Job 檢查到 `Provider == "Nine1"` → **直接跳過**
3. 新版 .NET Core 服務需要自己訂閱 `OrderCreated` 事件並處理首單邏輯

---

## 七、關鍵檔案

| 角色 | 路徑 |
|------|------|
| NMQ Job 主程式 | `C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\RegularOrders\RegularOrderCreatedProcess.cs` |
| DI 設定 | `C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\RegularOrders\RegularOrderCreatedProcessSetting.cs` |
| Request Entity | `C:\91APP\NMQ\nineyi.scm.nmqv2\ERP\Backend\BE\RegularOrder\RegularOrderCreatedEntity.cs` |
| OrderCreated Source | `C:\91APP\NMQ\nineyi.scm.nmqv2\WebStore\Backend\BE\Events\OrderCreatedSourceEntity.cs` |

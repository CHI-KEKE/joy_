```json
{
  "ExecuteTime": "2025-12-27T06:05:29.7032835+08:00"
}
```

```json
{"Data":"{\"ExecuteTime\":\"2025-08-14T02:20:41.7973958+00:00\",\"IsCrmOthersOrder\":true,\"ShopId\":2}"}
{"Data":"{\"ExecuteTime\":\"2025-03-18T11:32\",\"ShopId\":2,\"IsCrmOthersOrder\":true}"}
{"Data":"{\"ExecuteTime\":\"2025-03-18T11:32\",\"ShopId\":2,\"IsCrmOthersOrder\":true}"}
{"ExecuteTime":"2025-04-11T00:01:15.6443120+08:00"}
{"Data":"{\"ExecuteTime\":\"2025-04-11T00:01:15.6443120+08:00\"}"}
```

- 會同時處理給點給券
- IsCrmOthersOrder 判定
- groupedByShopId, 以該訂單最大最小區間撈活動, 這其間有點數活動就建點數活動稽核, 券一樣

## 線上訂單

**Cronjob 定時觸發** 每小時 01 分執行

| 參數 | 說明 | 資料來源 |
|------|------|----------|
| **時間範圍** | 執行時間前 2 小時 ~ 後一小時的訂單 | `salesOrderGroup` |
| **有效性檢查** | 檢查 `SalesOrderGroup.IsValid` | 資料庫驗證 |
| **ShopDefault 檢查** | GroupType : LoyaltyPoint / Key : IsCustomRuleEnable| 資料庫驗證 |

**資料對應關係**
```
ShopId => salesOrderGroup.ShopId
MemberId => salesOrderGroup.MemberId  
OrderCode => salesOrderGroup.TradesOrderGroupCode
OrderDateTime => salesOrderGroup.OrderDateTime
```

<br>
<br>

## 線下訂單

**由 `PromotionRewardBatchDispatcherV2Job` 觸發** PromotionRewardBatchDispatcherV2Job 線下訂單派發線下訂單回收點道具稽核當下 + 10.5hr

**資料對應關係**

| 目標欄位 | 資料來源 | 說明 |
|----------|----------|------|
| `ShopId` | `CrmSalesOrderShopId` | 商店 ID |
| `CrmMemberId` | `CrmSalesOrderCrmMemberId` | CRM 會員 ID |
| `CrmSalesOrderId` | `CrmSalesOrderId` | CRM 訂單 ID |
| `TradesOrderFinishDatetime` | `CrmSalesOrderTradesOrderFinishDateTime` | 交易完成時間 |


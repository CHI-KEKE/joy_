

https://91app.slack.com/archives/C08BE4B4KQW/p1753756490298899

## 問題分析

- PromotionRewardLoyaltyPointsDispatcherV2Job 沒有被觸發
- OrderCreated Event 沒看到對應的TG
- OrderFailed 有紀錄
- OrderRecheck 後訂單狀態為 Timeout 觸發了 OrderCancelled

## Athena

```SQL
SELECT * FROM "hk_prod_nmqv3"."archive_event"
WHERE event_name = 'OrderCreated'
AND date = '2025/06/15'
AND event LIKE '%TG250616A00008%'
LIMIT 10;
```

-  SalesOrderGroup
-  invalid => OrderFailed

<br>

### 結論

因為是訂單狀態的問題，Recycle 時應該確認訂單狀態根本不是 valid
不應該應去做回收，DDB 給券應該要檢查訂單狀態做噴錯
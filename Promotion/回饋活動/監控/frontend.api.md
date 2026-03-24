
## Apis


#### Cart Calculate

-  `/api/cart-calculate`
- `/api/carts/create`
- `2 Calculate Data`

<br>
<br>

####  Basket Calculate

- `/api/basket-calculate`
- `2 Basket Data`
- `CalculateByProcessGroup`
- `RuleList Count: 1`

<br>
<br>

## Log


#### S3 資料處理流程 

```
S3 取得商品與商品料號的標籤對應關係, 開始
S3 檔案 SaleProducts 節點為空
比對活動圈選料號結果為空
S3 取得門市資訊, 開始
S3 取得門市資訊, 結束
```

<br>

## PromotionRuleIds
```
RuleId: 5728, Type: RewardReachPriceWithRatePoint, Priority: 90000 -> 90000
活動序號 "" GetPromotionListAsync() 回傳結果為空
"PromotionRuleIds":[6886]
"CartExtendInfoItemGroup":82774
```

<br>
<br>

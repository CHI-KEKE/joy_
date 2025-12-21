## 🔑 主要欄位

| 欄位 | 說明 |
|------|------|
| **PromotionRuleId** | 單一活動規則ID |
| **PromotionRuleIds** | 多個活動規則ID清單 |
| **PromotionRules** | 點數活動規則（需要以訂單當下版本為主，從外部傳入） |
| **CalculateDateTime** | 計算時間（點數活動需要以訂單當下時間為準） |
| **PromotionSourceType** | 促銷來源類型：Promotion / Coupon / LoyaltyPoint / ExtraPurchase / FeePromotion |

<br>

## 👤 使用者資訊 (User)

| 欄位 | 說明 |
|------|------|
| **Id** | 會員ID (MemberId) |
| **Tags** | 會員標籤（首購、會員等級） |
| **OuterId** | 會員外部編號 (VipMember上的) |
| **Channel** | 通路 |
| **CurrencyDecimalDigits** | 幣別小數位數 |

<br>

## 🛒 商品清單 (SalepageSkuList)

```json
{
  "SalepageId": 0,
  "SkuId": 0,
  "Price": 8,
  "SuggestPrice": 0,
  "Qty": 1,
  "Flags": [],
  "OuterId": "",
  "Tags": null,
  "OptionalTypeDef": "",
  "OptionalTypeId": 0,
  "CartExtendInfoItemGroup": 828042,
  "CartExtendInfoItemType": "TradesOrderSlaveId",
  "PointsPayPair": null,
  "CartExtendInfos": [],
  "CartId": 0
}
```
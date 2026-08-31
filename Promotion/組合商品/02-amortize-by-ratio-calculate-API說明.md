# amortize-by-ratio-calculate API 說明

## API 位置

```
POST https://promotion-api-frontend-internal.hk.91app.io/api/amortize-by-ratio-calculate
```

專案：`nine1.promotion.web.api.frontend`  
Controller：`CalculateController.cs`  
Service：`PriceCalculateService.cs` → `AmortizeByRatioCalculate()`

---

## 用途

> **把整包 bundle 的所有折扣，按比例分配給每個子商品，讓每筆訂單明細都有正確的折扣金額。**

成單後，每個 `TradesOrderSlave`（子訂單）都需要有自己的 `TotalDiscount`，  
這支 API 就是負責在結帳前幫每個子商品算出該分到多少折扣。

---

## 兩件事情同時做

### 1. 計算 Bundle 差價（additionalDiscount）

```csharp
// PriceCalculateService.cs - SetSalepageBundleAdditionalDiscount()
var majorTotalPrice = majorItem.Price * majorItem.Qty;
var subItemTotalPrice = subItemList.Sum(x => x.Price * x.Qty);
var additionalDiscount = subItemTotalPrice - majorTotalPrice;
```

用來表達「子商品原本多少錢，現在 bundle 只要付多少，差了多少」。

### 2. 攤提其他折扣到子商品（按金額比例）

若有優惠碼、Coupon、點數折扣等作用在整個 bundle 上，  
這支 API 會依照各子商品的金額比例來分配折扣。

```
例：整包折扣 50 元，子商品共 2 件
子商品 A 小計 245 / 總計 735 ≈ 33% → 分到 16.5 元
子商品 B 小計 490 / 總計 735 ≈ 67% → 分到 33.5 元
合計 = 50 元 ✅
```

---

## Request 結構

```json
{
  "CurrencyDecimalDigits": 2,
  "SalepageSkuList": [
    {
      "SalepageId": 593720,
      "SkuId": 4104441,
      "Price": 160.00,
      "Qty": 2,
      "Flags": ["SalepageBundleMajor"],
      "CartExtendInfoItemGroup": 1778092643694,
      "CartExtendInfoItemType": "Major",
      "CartId": 29666166
    },
    {
      "SalepageId": 589830,
      "SkuId": 4090706,
      "Price": 79.00,
      "Qty": 4,
      "Flags": ["SalepageBundleSub"],
      "CartExtendInfoItemGroup": 1778092643694,
      "CartExtendInfoItemType": "Sub",
      "CartId": 29666167
    }
  ],
  "AmortizeDiscountItems": [
    {
      "CartId": 29666166,
      "TotalDiscount": 0,
      "PromotionDiscount": 0,
      "DiscountDetailList": []
    }
  ]
}
```

- `SalepageSkuList`：主商品與子商品列表
- `AmortizeDiscountItems`：需要攤提的折扣項目（只需填主商品的）

---

## Response 結構

```json
{
  "code": "Success",
  "data": {
    "salepageSkuList": [
      {
        "skuId": 4104441,
        "cartId": 29666166,
        "additionalDiscount": 0,
        "totalDiscount": 0,
        "totalPrice": 320.00,
        "totalPayment": 320.00,
        "flags": ["SalepageBundleMajor"]
      },
      {
        "skuId": 4090706,
        "cartId": 29666167,
        "additionalDiscount": -4,
        "totalDiscount": -4,
        "totalPrice": 316.00,
        "totalPayment": 320.00,
        "flags": ["SalepageBundleSub"]
      }
    ]
  }
}
```

- 主商品：`additionalDiscount` 固定為 0（line 490）
- 子商品：`additionalDiscount` = 攤提計算結果（可能為正值折扣或負值加費）
- `totalPayment`：子商品最終付款 = `totalPrice - totalDiscount`

---

## 計算流程（PriceCalculateService.cs）

```
foreach 主商品:
  1. SetSalepageBundleAdditionalDiscount()  → 計算 bundle 差價
  2. AmortizeDiscountAmountByRatio()        → 按比例攤提折扣
  3. GetSalepageSkuProcessEntity(主商品)    → additionalDiscount = 0
  4. foreach 子商品:
       GetSalepageSkuProcessEntity(子商品) → additionalDiscount = Sum(Additional 折扣)
```

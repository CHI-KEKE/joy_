## 生日壽星貼標

### DDB Table

<br>

**Table Name:** HK_QA_OSM_MemberCollectionMapping
**Key:** System_BirthdayMonth_5
**說明:** 該 shop 的 12 個 Birthday CollectionIds
**組成:** Name / Month / CollectionId

<br>

### 程式碼路徑

<br>

```
C:\91APP\Promotion\frontEnd\nine1.promotion.web.api.frontend\src\BusinessLogic\Nine1.Promotion.BL.Services\Rules\Repositories\PromotionRuleRepository.cs
```

<br>

### GetRuleInfoListAsync

<br>

以活動是否有 "rule.IsBirthdayMonthEnabled" 來確認是否為當月壽星

<br>

拉訂單對應的時間 promotionCollectionId + birthdayCollectionId + memberId 送去打 memberCollection，有 match 的會貼標在 context.Calculate.User.Tags
，壽星要置換成純文字 CurrentBirthdayMonth

<br>

### 時間判斷

<br>

- **cart-calculate:** context.Cart.Now.Month
- **basket-calculate:** basketCalculateRequest.CalculateDateTime.Value

<br>

### MatchedUserScopes 範例

<br>

```json
"MatchedUserScopes": [
  {
    "UserScopeType": "NineYi.Msa.Promotion.Engine.AllUserScope"
  },
  {
    "UserScopeType": "NineYi.Msa.Tagging.TagUserScope",
    "Tag": "CurrentBirthdayMonth"
  }
]
```

<br>

### 引擎判斷邏輯

<br>

引擎會看 IsBirthdayMonthEnabled + CurrentBirthdayMonth有貼才中

<br>



## 壽星當月活動 UserTag 未包含 CurrentBirthdayMonth


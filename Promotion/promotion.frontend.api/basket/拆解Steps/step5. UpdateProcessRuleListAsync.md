
## UpdateProcessRuleListAsync

**context.ProcessRuleList 塞進 context.ProcessRuleList.RuleList**
```csharp
//// 以 ProcessRule 取得 Rule
foreach (var processRule in context.ProcessRuleList)
{
    processRule.RuleList = ruleList.Where(i => i.TypeDef == processRule.Name).ToList();
}
```

**2️⃣ 處理會員集合 (MemberCollection)**
   - 當月壽星解析Rule，取得當下時間並取得對應 `birthdayCollectionId`
   - 加入 MemberCollection
   - 打 match，若有中就改成 `CurrentBirthdayMonth` 貼在 UserTag

**3️⃣ 設定規則清單**
   - 拿剛剛撈的 Ids 設定 `_rulesList`
   - 格式：`RuleEntity(i.PromotionEngineId, this.SourceType, i.TypeDef, i.Rule, i.PayProfileTypeDef)`

📌 **條件**: 商品與 promotion 有交集到才 `UpdateProcessRule` => 設定 `context.ProcessRuleList.RuleList`

<br>
<br>

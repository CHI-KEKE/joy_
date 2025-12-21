
**🔄 LoadRules 流程**
```csharp
var ruleList = context.ProcessRuleList.SelectMany(i => i.RuleList).ToList();
_promotionEngine.LoadRules(RuleLoader.AssemblyFullName, ruleList.Select(i => i.Rule).ToList());
```

**📊 引擎設定**
- 在引擎設定 `Rules[promotionRuleBase.Id] = promotionRuleBase`
- 型別: `public IDictionary<long, PromotionRuleBase> Rules { get; private set; } = new Dictionary<long, PromotionRuleBase>();`

**⚡ 更新規則 Priority**
- `context.ProcessRuleList.Single(i => i.Name == type).Priority => _promotionEngine.Rules.Values`


## Group Priority & PromotionType Priority

#### GetProcessGroupList

拉出 group list 會有 priority，最後面是 bottom group

<br>

####　CalculateByProcessGroupAsync

會 order by priority 依序計算

<br>

####　CreateGroupContext

每個 group 會 GetProcessRuleList，把每個 group 的活動類型都拉出來，且活動類型也都有 priority，到引擎處理步驟，這邊會先設定進去 `groupContext.ProcessRuleList`

<br>

#### LoadRules

這邊會依據活動類型設定的 priority 設定 `rule.Priority`，最後促購引擎依據 priority 排序 如果排序相同就依據 Id , for loop 依序計算
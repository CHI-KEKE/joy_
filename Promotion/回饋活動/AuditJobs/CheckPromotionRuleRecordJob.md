```json
{"Data": "{\"StartDatetime\":\"2025-03-07T02:00\",\"EndDatetime\":\"2025-03-20T02:45\"}"}
```

## 輸入參數

| 參數名稱 | 說明 | 格式範例 |
|---------|------|----------|
| `StartDatetime` | 活動時間區間起始 | `2025-03-07T02:00` |
| `EndDatetime` | 活動時間區間結束 | `2025-03-20T02:45` |


| 資料表 | 檢查範圍 | 說明 |
|--------|----------|------|
| **PromotionEngine** | 指定時間區間內尚未結束的活動 | 活動主表資料 |
| **PromotionRuleRecord** | 每檔活動最新一筆規則記錄 | 活動規則設定 |


**🗃️ RuleRecord 存在性** 檢查活動是否有對應的規則記錄

**🔑 S3 Key 完整性** 驗證最新 RuleRecord 是否包含 S3 Key

**☁️ S3 資料可用性** 確認 S3 檔案實際存在且可存取

**🏷️ ProductScope 標籤解析** 解析 ProductScope 並驗證 OuterIdTag 能正確取得 promotionTagId

**🔗 資料一致性比對** 比對 S3 的 `S3ProductSkuOuterIds` 與 `PromotionTagSlave_TargetTypeCode`

當檢核發現問題活動時：

- **⚠️ 風險評估**: 新增/編輯活動時必定會上傳 S3 並產生對應 Record
- **🗑️ 資料清理**: 若無有效 Record 則視為髒資料，應移除該活動
- **🛡️ 風險防護**: 防止購物車無法進入等系統異常
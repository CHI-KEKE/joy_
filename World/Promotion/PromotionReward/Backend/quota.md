

## 📋 目錄
1. [節點](#節點)
2. [PromotionEngineSetting](#promotionenginesetting)
3. [驗證器 (PromotionBaseEntity)](#驗證器-promotionbaseentity)
4. [PCPS](#pcps)
5. [S3 RuleRecord](#s3-rulerecord)

---

## 節點

**PromotionBaseEntity**

- Quota
  - 0 : 不限次數
- IsQuotaEnabled

<br>

## PromotionEngineSetting

- Table：PromotionEngineSetting
- Column：PromotionEngineSetting_CustomField2

無上限的資料 : Quota:0
有上限的資料 : Quota:1


<br>

## 驗證器 (PromotionBaseEntity)

<br>

- 上限Ｎ次（預設此選項），若選此項，則Ｎ為必填寫
- 此Ｎ欄位最多輸入 1-30，若輸入非半形數字，需要出現錯誤訊息：請輸入半形數字
- 無上限
- 新舊相容 (舊的活動沒有佔額概念，編輯時要確保給預設值，但預設值是 N 次，要定義清楚多少)


## PCPS

- Since 開始佔額時間 = 活動開始時間 (未來訂單無法佔額)
- Until 結束時間無限延長 (線下訂單匯入過去訂單仍然可以佔額 佔額可以佔用過去活動)
- 活動開始前還可以異動佔額

PromoCodePoolService.CreateAndEnablePromotionEngineQuotaGroup
PromoCodePoolService.UpdatePromotionEngineQuotaGroup
PromoCodePoolService.GetPromotionEngineQuotaGroupData

```json
{
  "ModeType": "Functional",
  "DispatchType": "Random",
  "Pattern": "GUID6",
  "Since": "2025-11-05T00:00:00",
  "Until": "9999-12-31T23:59:59.9999999",
  "LimitRuleSettings": [
    {
      "ActionType": "Redeem",
      "LimitTarget": "2", // 2 表示會員
      "LimitRules": [
        {
          "Quota": "entity.Quota"
        }
      ]
    }
  ],
  "CanRedeemDirectly": true
}
```


## S3 RuleRecord

相當於 PCPS 的 groupId

DB : PromotionEngine_GroupCode

promotionRewardPointRulePackageEntity
S3 : GroupCode

## 📋 目錄
1. [APIs](#apis)
2. [UpdatePromotionRequestEntity](#updatepromotionrequestentity)

---

## APIs

`/api/promotion-rules/create`
`/api/promotion-rules/get`
`api/promotion-rules/update`



## UpdatePromotionRequestEntity

**繼承來源**：PromotionBaseEntity → 因此自動包含所有活動的共通欄位屬性

<br>

🔑 **主要結構**

<br>

**1️⃣ 共通欄位 (PromotionBaseEntity)**

<br>

| 欄位名稱 | 說明 |
|----------|------|
| ShopId | 商店序號，代表活動所屬店家 |
| Id | 活動唯一序號，更新時必填 |
| Name | 活動名稱 |
| Description | 活動描述 |
| TypeDef | 活動折扣類型，對應促銷邏輯 |
| StartDateTime / EndDateTime | 活動期間 |
| IsRegular | 是否為常態活動 |
| HasPicture | 活動是否包含圖片 |
| Terms / AgreeContent | 活動條款與同意內容 |

<br>

**2️⃣ 🧩 目標對象設定**

<br>

| 欄位名稱 | 說明 |
|----------|------|
| TargetMemberType / TargetMemberTypeDef | 目標會員類型（如所有會員、VIP） |
| CrmShopMemberCardIds | 指定會員卡等級 ID 清單 |
| PromotionTargetRegionType | 配送區域範圍（全區/分區） |
| IncludeTargetRegionIdList | 指定配送區域清單 |
| TargetLocationType | 門市範圍類型（全店/部分門市） |
| IncludeTargetLocationIdList | 指定門市 ID |
| TargetPlatformTypeList | 適用通路類型（官網/APP 等） |

<br>

**3️⃣ 🏷️ 指定 / 排除商品範圍**

<br>

| 欄位名稱 | 說明 |
|----------|------|
| TargetType / TargetTypeDef | 活動適用範圍（如全店、指定商品） |
| IncludeTargetIdList | 指定的商品或分類 ID 清單 |
| IncludeProductSkuOuterIdList | 指定商品 SKU（外部編號） |
| ExcludeTargetIdList | 排除的商品 ID 清單 |
| ExcludeProductSkuOuterIdList | 排除的 SKU 清單 |

<br>

**4️⃣ 🛍️ 促銷規則**

<br>

| 欄位名稱 | 說明 |
|----------|------|
| DiscountRuleList | 折抵規則（滿額折抵） |
| GiftRuleList | 贈品規則（滿額贈品） |
| AddOnsRuleList | 加價購商品設定 |
| IsCyclable | 是否循環執行 |
| Accumulated | 是否累贈（累積多次） |
| IsMultiLevel | 是否多階層設定 |
| CanUseECoupon | 是否可與折價券併用 |
| CalculateTypeDef | 與其他活動計算邏輯 |

<br>

**5️⃣ 🎁 給點 / 給券**

<br>

| 欄位名稱 | 說明 |
|----------|------|
| RewardPointConditions | 舊版給點條件（已標記 Obsolete） |
| RewardConditions | 新版給點條件 |
| RewardPointRuleList | 給點規則清單（滿額給點細節） |
| RewardCouponRuleList | 給券規則清單 |
| PeriodType | 點數效期設定 |
| PromotionRewardPointCalculateTypeDef | 點數計算方式（百分比、固定值等） |

<br>

**6️⃣ 📦 其他設定**

<br>

| 欄位名稱 | 說明 |
|----------|------|
| IsDisplayInShoppingCart | 是否在購物車顯示活動訊息 |
| IsAddOnsThresholdEligible | 加購品是否可作為滿額門檻 |
| PayTypes / ShippingTypes | 限定的付款 / 配送方式 |
| Label | 活動標籤 |
| UpdatedUser / UpdatedDateTime | 更新者與更新時間 |
| IsNeedRecordIo | 是否需要記錄歷程（I/O） |
| Sort | 排序順序 |



## 開發要注意的項目

- Mapping 問題 (確保不會因為其他無相關操作資料被洗掉例如批次)
- 驗證要新舊相容
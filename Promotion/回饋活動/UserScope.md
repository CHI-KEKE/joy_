## MatchedUserScopes / VisibleUserScopes 節點說明

用來定義這檔活動要「對哪些會員卡等級」生效或顯示，與 Thresholds 配對使用。

```json
"MatchedUserScopes": [
  {
    "UserScopeType": "NineYi.Msa.Tagging.TagUserScope",
    "Tag": "CrmShopMemberCard:1"
  }
]
```

若 UserScopeType 是 TagUserScope 且 Tag 為 CrmShopMemberCard:{id}，則表示對應該會員卡生效。該 id 也必須存在於 Thresholds 內的 key。

## UserScope 邏輯

targetType = MemberTier 才設定以下

```json
{
  "UserScopeType": "TagUserScope",
  "Tag": "CrmShopMemberCard:{id}"
}
```

## TargetMemberType 驗證邏輯

TargetMemberType = MemberTier 做以下檢查

- CrmShopMemberCardIds 不得為空
- 新給點活動 CrmShopMemberCardId 只能有一張
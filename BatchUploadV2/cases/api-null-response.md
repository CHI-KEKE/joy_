# 案例：API 回傳 null 導致錯誤明細顯示異常

## 案例資訊

| 項目 | 值 |
|------|-----|
| PR | https://bitbucket.org/nineyi/nineyi.scm.nmqv2/pull-requests/15870/overview |
| 影響功能 | 批次作業的錯誤明細顯示 |

<br>

---

## 症狀

批次作業執行後，錯誤明細頁面顯示異常，訊息內容不正確或無法正常呈現。

<br>

---

## 根本原因

SCMAPIV2 回傳 `null` 時，程式未做 null 防護處理，導致後續組裝錯誤訊息（`BatchUploadMessage.Note`）時發生例外，使錯誤明細內容顯示錯誤。

<br>

---

## 解法

在 NMQ 子 Task 處理 API 回應時，加入 null 檢查，確保 API 回傳 null 時不影響錯誤訊息的正確呈現。

詳見 PR：https://bitbucket.org/nineyi/nineyi.scm.nmqv2/pull-requests/15870/overview

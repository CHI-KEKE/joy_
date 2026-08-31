# 訂單匯出「是否為加價購品」欄位 — 現狀分析總覽

> User Story：[#604561](https://91appinc.visualstudio.com/MoneyLogistic/_workitems/edit/604561)  
> Sprint：194 | Area：Commerce 5 | 負責人：Allen Lin

## 需求一句話

調整「所有線上訂單查詢」批次匯出檔案中「**是否為加價購品**」欄位，使其能區分 **滿額加價購** 與 **滿件加價購**。

---

## 文件索引

| 文件 | 說明 |
|---|---|
| [flow-overview.md](./flow-overview.md) | 完整功能流程（使用者觸發 → NMQ → 產檔完成） |
| [sp-analysis.md](./sp-analysis.md) | SP `csp_BatchExportSalesOrderData` 詳細分析 |
| [gap-analysis.md](./gap-analysis.md) | 現狀 vs User Story 需求差距分析 |
| [fix-proposal.md](./fix-proposal.md) | 修改方案（不改 SP） |

---

## 快速結論

```
現狀：滿額加價購 / 滿件加價購 → 都顯示「購物車滿額加價購」(無法區分)
需求：滿額加價購 → 「滿額加價購」
      滿件加價購 → 「滿件加價購」
```

**根本原因**：SP 輸出的 `SalesOrderSlave_IsExtra` 僅為 BIT (0/1)，`SalesOrderSlavePromotionEngine_TypeDef` 欄位（內含 CartReachPriceExtraPurchase / CartReachPieceExtraPurchase）從未被帶出。

**修改策略**：不改 SP，在 ScmNmq 應用層對 IsExtra=1 的明細 batch query `SalesOrderSlavePromotionEngine` 取得 TypeDef，再決定顯示文字。

---

## 涉及專案

| 專案 | 路徑 | 角色 |
|---|---|---|
| SMS | `C:\91APP\NineYi.Sms` | 使用者觸發端、建立 NMQ Task |
| ScmNmq | `C:\91APP\NMQ\nineyi.scm.nmqv2` | 消費 Task、產生 Excel |
| DB | `C:\91APP\DB\databases\ERPDB` | SP 查詢訂單資料 |

# Cybersource 異常紀錄 — RefundQuery 查詢循環異常

### 1. RefundQuery 查詢循環異常

#### 問題描述
退款請求完成後，系統持續進行 RefundQuery 查詢直到逾時中斷

#### 異常流程
| 步驟 | 狀態 | 說明 |
|------|------|------|
| 1 | 執行 Refund | 發起退款請求 |
| 2 | RefundProcessing | 每小時執行 RefundQuery 查詢 |
| 3 | Pending 4003 | 最後一次查詢結果仍為處理中 |
| 4 | 系統 Redo | 因逾時重新處理 |
| 5 | Transmitted | 最終退款狀態為成功 |

#### 原因
**Cybersource 退款查詢回應時間較長**，可能需要數小時才能取得最終結果

<br>


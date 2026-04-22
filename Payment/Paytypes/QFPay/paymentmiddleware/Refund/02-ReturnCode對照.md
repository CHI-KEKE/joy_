# QFPay — Refund ReturnCode 詳細對照

## QFPay ResponseCode 完整對照表

| QFPay ResponseCode | 常數名稱 | 含義 | PMW ReturnCode | 說明 |
|-------------------|---------|------|---------------|------|
| `0000` | `Success` | 退款成功 | **Success** | 退款完成 |
| `1124` | `SameDayPartialRefundRejected` | 當日部分退款拒絕 | **RefundPending** | 等待隔日重試 |
| `1145` | `RequestProcessing` | 處理中，請稍後 | **RefundPending** | 退款仍在處理，繼續輪詢 |
| `1143` | `OrderStatusUncertain` | 訂單狀態不確定 | **RefundPending** | 狀態未明，繼續輪詢 |
| `1265` | `RefundNotAllowedDuringBillingPeriod` | 結帳期間（23:30~00:30）禁止退款 | **RefundPending** | 等過了結帳期重試 |
| `1265` + 特殊描述 | — | FPS 超過可退款時限 | **Failed** | ⚠️ 覆蓋邏輯（見下方說明） |
| `1269` | `InsufficientRefundableBalance` | 可退款餘額不足 | **RefundPending** | 餘額問題，等待重試 |
| `1250` | `PayMeExceedRefundPeriod` | PayMe 超過退款期限 | **RefundPeriodExceeded** | 無法退款 |
| `1297` | `AliPayExceedRefundPeriod` | AliPay 超過退款期限 | **RefundPeriodExceeded** | 無法退款 |
| `1181` | `OrderExpired` | 訂單已過期 | **RefundFailed** | 退款失敗 |
| 其他 | — | 未知錯誤 | **Failed** | 一般失敗 |

---

## 特殊覆蓋邏輯：1265 的雙重含義

```
if responseCode == "1265":
    if responseDescription contains "該筆訂單被禁止退款，已經超出可退款時限":
        ReturnCode = Failed   ← 覆蓋原本的 RefundPending
    else:
        ReturnCode = RefundPending
```

**原因：**
- `1265` 原始語意：QFPay 結帳對帳期（每日 23:30~00:30）暫停退款，等待重試即可
- FPS（轉數快）超過 29 天退款期限，也回 `1265`，但描述文字包含「超出可退款時限」
- FPS 過期的情況**無法重試**，需強制轉為 Failed 避免系統無限輪詢

---

## 各支付管道的退款限制

| 支付管道 | 退款期限 | 超期 ReturnCode |
|---------|---------|----------------|
| PayMe | 不明確 | RefundPeriodExceeded（`1250`） |
| AliPay | 不明確 | RefundPeriodExceeded（`1297`） |
| FPS（轉數快） | 29 天 | Failed（`1265` + 特殊描述） |

---

## RefundQuery 與 Refund 的 ResponseCode 差異

RefundQuery 使用相同的狀態對應邏輯，**但少了 `OrderExpired (1181)` 的對應**：

| ResponseCode | Refund | RefundQuery |
|-------------|--------|------------|
| `0000` | Success | Success |
| `1124` / `1145` / `1143` / `1265` / `1269` | RefundPending | RefundPending |
| `1250` / `1297` | RefundPeriodExceeded | RefundPeriodExceeded |
| `1181` | **RefundFailed** | **Failed**（走 default） |
| 其他 | Failed | Failed |

> RefundQuery 的 `switch` 沒有列出 `1181`，因此走 `default → ReturnCodes.Failed`，  
> 與 Refund 的 `ReturnCodes.RefundFailed` 語意略有不同，但實際效果相近。

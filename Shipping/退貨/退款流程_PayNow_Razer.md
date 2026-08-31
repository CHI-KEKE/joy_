# 退款流程 — PayNow / Razer (PaymentMiddleware)

## 觸發入口

```
消費者前台申請退貨
    ↓
SMS 建立 CancelRequest（CancelOrderSlave）
    ↓
NMQ 訊息 → ThirdPartyCancelOrderProcess.cs
    ↓
PaymentMiddlewareCancelOrderService.DoCancel()
    ↓
確認條件：CancelOrderSlave = Finish AND PaymentRequest = Finish
    ↓
CreateRefundRequestByCancelFlow()
    ↓
ThirdPartyRefundRequestService（計算退款金額）
    ↓
呼叫 PaymentMiddleware → Razer API
```

---

## 退款金額計算

**`ThirdPartyRefundRequestService.cs` Line 376：**

```csharp
decimal amount = (Price × Qty) + Discount + CancelOrderSlave_RechargeReceiptAmount;
```

| 欄位 | 說明 |
|------|------|
| `Price × Qty` | 商品金額 |
| `Discount` | 折扣（負數） |
| `RechargeReceiptAmount` | 補收單金額（負數，預設 0） |

---

## PaymentMiddleware 對 Razer 的行為

- **完全信任** SCM 傳來的 Amount，不做額外驗證或重算
- 退款簽章：`md5(P + MerchantID + RefID + TxnID + Amount + SecretKey)`
- 支援 Fiuu 新節點（`IsFiuuEnable=true` 時切換端點）

---

## 退款單（RefundRequest）結構

| 欄位 | 說明 |
|------|------|
| `SourceDef` | 退款來源（見下表） |
| `Amount` | 退款金額 |
| `StatusUpdatedUser` | `PayNowRazerRefundRequestFinish`（自動）或操作者 email |
| `IsAgreedToDelegate` | 1 = 同意委託退款 |
| `PayTypeDef` | 付款方式，如 `PayNow_Razer` |

### SourceDef 對照

| SourceDef | 說明 |
|-----------|------|
| `ReturnGoodsOrderSlave` | 商品退款 |
| `SalesOrderFee` | 運費退款（自動） |
| `RechargeReceipt` | 補收單退款（此情況為退補收金額給消費者，較少見） |

---

## 案例解析：MG260424N00001 Elemis SG

### 訂單資訊

| 項目 | 金額 |
|------|------|
| 商品 | SGD 62 |
| 運費 | SGD 5 |
| **訂單總計** | **SGD 67** |
| 補收單 | SGD 5（商家建立） |

### 實際 RefundRequest 資料

| TradesOrderSlaveCode | Amount | SourceDef | StatusUpdatedUser |
|----------------------|--------|-----------|-------------------|
| MS260424N000001 | 57.00 | ReturnGoodsOrderSlave | PayNowRazerRefundRequestFinish |
| MS260424N000001 | 5.00 | SalesOrderFee | PayNowRazerRefundRequestFinish |
| MS260424N000002 | 0.00 | ReturnGoodsOrderSlave | williamteoh@nine-yi.com |
| MS260424N000003 | 0.00 | ReturnGoodsOrderSlave | williamteoh@nine-yi.com |
| MS260424N000004 | 0.00 | ReturnGoodsOrderSlave | williamteoh@nine-yi.com |

### 數字驗證

```
商品退款：62 + (-5) = SGD 57  ← 補收單已被扣除
運費退款：SGD 5（獨立一筆）
Razer 收到：57 + 5 = SGD 62
原始訂單 - 補收單：67 - 5 = SGD 62 ✅
```

> MS000002/003/004 均為 SGD 0，由 williamteoh 手動完成（清空佇列用）。

---

## 支援 PaymentMiddleware 的付款方式（部分）

`ThirdPartyCancelOrderProcess.cs` 白名單包含：

- `PayNow_Razer`
- `LinePay`、`JKOPay`、`Atome`
- Razer 系列（eWallet、CreditCard 等）
- 其他第三方支付共 25+ 種

## 3. ApplicationFee / Refund / TransferReversal

### 3.1 適用範圍
- **專用於**：DestinationCharge 付款模式
- **適用帳戶類型**：Custom 帳戶（使用 DestinationCharge 流程）

### 3.2 功能對比

| 功能 | 說明 | 資金流向 | 適用場景 |
|------|------|----------|----------|
| **Refund** | 退款給客戶 | 商家 → 客戶 | 客戶要求退貨、取消訂單 |
| **Application Fee Refund** | 退還平台手續費 | 平台 → 賣家 | 平台費用調整、爭議處理 |
| **Transfer Reversal** | 資金回收 | 賣家 → 平台 | 撤銷轉帳、資金重新分配 |

### 3.3 Transfer Reversal 詳細說明

#### 核心功能
從關聯帳戶（賣家）收回資金到平台帳戶，可同時決定是否退還相關的 Application Fee。

#### 執行結果
- **平台帳戶**：餘額增加
- **賣家帳戶**：餘額減少
- **Application Fee**：可選擇是否一併退還

### 3.4 限制條件

#### 金額限制
| 限制類型 | 說明 | 影響 |
|----------|------|------|
| **Destination Charge** | 撤銷金額 ≤ 原始收費金額 | 不能撤銷超過原本收費的金額 |
| **Transfer Group** | 目標帳戶需有足夠餘額 | 餘額不足時撤銷失敗 |

### 3.5 實際應用案例

#### 基礎交易情境
假設電商平台上的一筆交易：
- **商品價值**：$100
- **平台抽成**：10%（$10 Application Fee）
- **賣家實收**：$90

#### 操作案例對比

##### 情況 1：全額撤銷
```
執行動作：Transfer Reversal $90
選項：同時退還 $10 Application Fee

結果：
- 平台帳戶：+$90（可選 +$10）
- 賣家帳戶：-$90
- Application Fee：可選退還 $10
```

##### 情況 2：部分撤銷
```
執行動作：Transfer Reversal $45
選項：按比例退還 Application Fee

結果：
- 平台帳戶：+$45（可選 +$5）
- 賣家帳戶：-$45
- Application Fee：可選退還 $5（比例計算）
```

### 3.6 資金流向圖解

```
原始交易：
客戶 --$100--> 平台 --$90--> 賣家
              └─$10 Application Fee

Refund：
客戶 <--$100-- 平台（退款給客戶）

Application Fee Refund：
平台 --$10--> 賣家（退還手續費）

Transfer Reversal：
平台 <--$90-- 賣家（資金回收）
```

### 3.7 使用時機

| 操作類型 | 使用時機 | 業務場景 |
|----------|----------|----------|
| **Refund** | 客戶退貨/取消 | 正常退款流程 |
| **Application Fee Refund** | 費用調整 | 促銷活動、爭議處理 |
| **Transfer Reversal** | 資金重新分配 | 帳戶調整、錯誤修正 |

### 3.8 注意事項
- **執行順序**：建議先處理 Transfer Reversal，再進行 Refund
- **餘額檢查**：執行前需確認賣家帳戶有足夠餘額
- **費用處理**：Application Fee 的退還是可選項，需要明確指定

<br>

---


---

### 4.6 手續費退款處理

#### 4.6.1 退款判斷邏輯

**主要判斷條件**：
```csharp
request.ExtendInfo.IsRefundApplicationFee // 判斷 Stripe 是否需要退手續費
```

**檢查條件清單**：
| 條件 | 程式碼檢查 | 說明 |
|------|------------|------|
| 1 | `this.IsSalesOrderFee(refundRequest.RefundRequest_SourceDef)` | 判斷是否為運費相關退款 |
| 2 | `salesOrderSlaveDateTime >= new DateTime(2020, 7, 1) \|\| Config : Charge.SalesOrderFee.Shop` | 檢查訂單時間或商店設定 |
| 3 | `request.ExtendInfo.ApplicationFeeAmount > 0` | 確認有應用程式費用需要退還 |

#### 4.6.2 退款金額計算

**基本計算公式**：
```csharp
decimal refundApplicationFee = this.CalculateFee(refundAmount, feeRate);
```

**退貨子單特殊處理**：

當處理退貨子單且有補收單時，需要進行調整計算：
```csharp
// 取得補收金額及其手續費
var rechargeAmount = rechargeReceipt.RechargeReceipt_RechargeAmount;
var rechargeAmountFee = this.CalculateFee(rechargeAmount, feeRate);

// 計算實際退還的手續費
refundApplicationFee = totalPaymentFee - rechargeAmountFee;
```

#### 4.6.3 API 呼叫

**Stripe 退款 API 端點**：
```http
POST /v1/application_fees/{applicationFeeId}/refunds
```

**官方文件**：[Stripe Application Fee Refunds API](https://docs.stripe.com/api/fee_refunds/create)

#### 4.6.4 退款類型對比

| 退款類型 | 定義 | 資金流向 | 適用場景 |
|----------|------|----------|----------|
| **普通退款**<br>(Refund) | 將交易金額退還給客戶 | 商家賬戶 → 客戶支付方式 | 客戶退貨、取消訂單 |
| **應用程式費用退款**<br>(Application Fee Refund) | 退還平台收取的服務費用 | 平台賬戶 → 賣家賬戶 | 費用調整、爭議處理 |

#### 4.6.5 兩種退款的關鍵差異

| 比較項目 | 普通退款 | 應用程式費用退款 |
|----------|----------|-------------------|
| **資金流向** | 商家賬戶 → 客戶 | 平台賬戶 → 賣家賬戶 |
| **影響對象** | 直接影響客戶 | 影響平台和賣家，不直接影響客戶 |
| **關聯交易** | 與客戶的原始支付交易相關 | 與平台收取的費用相關 |
| **執行時機** | 客戶退款請求 | 平台費用政策調整 |

#### 4.6.6 實際應用場景

**基礎交易範例**：
```
客戶支付金額：$100
平台應用程式費用：$10 (10%)
賣家實際收到：$90
```

##### 場景 1：客戶要求全額退款
| 操作步驟 | 執行動作 | 金額 | 影響對象 |
|----------|----------|------|----------|
| 1 | 執行普通退款 | $100 | 客戶收到退款 |
| 2 | 執行應用程式費用退款 | $10 | 賣家收回手續費 |

**結果**：客戶拿回 $100，賣家收回被扣的 $10 手續費

##### 場景 2：平台費用調整
| 操作步驟 | 執行動作 | 金額 | 影響對象 |
|----------|----------|------|----------|
| 1 | 僅執行應用程式費用退款 | $5 | 賣家獲得部分手續費返還 |

**結果**：客戶不受影響，賣家獲得 $5 手續費減免

#### 4.6.7 退款限制與特點

| 特點 | 說明 | 注意事項 |
|------|------|----------|
| **部分退款** | 應用程式費用可分次退還 | 直到全額退還為止 |
| **退款上限** | 不能超過原始費用金額 | 超額退款會導致 API 錯誤 |
| **單次性** | 全額退還後不能再次操作 | 需要謹慎規劃退款策略 |

#### 4.6.8 商業價值

| 優勢 | 說明 |
|------|------|
| **彈性收費模式** | 平台可根據情況調整收入策略 |
| **爭議處理** | 有效處理商家與平台間的費用爭議 |
| **商家激勵** | 可作為獎勵優質商家的工具 |
| **關係維護** | 增強平台與商家的合作關係 |

---



---


---

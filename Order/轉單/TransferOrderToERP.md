


## 4. 檢查資料狀態 同步其他 Table

### 4.1 檢查請款單存在才長 csp_UpdateDataAfterTradesOrderTransToSalesOrderWithFlow

```csharp
var paymentRequestItem = this._paymentRequestRepository.Get(tradesOrderGroupCode);
```

<br>

### 4.2 csp_UpdateDataAfterTradesOrderTransToSalesOrderWithFlow 包覆大量子 csp

<br>

### 4.3 更新至OrderSlaveFlow 

[csp_UpdateERPDBOrderSlaveFlowByTradesOrderGroupId]
轉單確定完成後，將商店序號、供應商序號、GUID 更新至OrderSlaveFlow

<br>

### 4.4 更新成交手續費率及信用卡費費率

[csp_UpdateSalesOrderSlaveFeeRateByGroupId]
轉單確定完成後，再更新成交手續費率及信用卡費費率

<br>

### 4.5 更新徵信方式 

csp_UpdateCreditCheckTypeDefByGroupId
轉單確定完成後，再更新徵信方式

<br>

### 4.6 溫層商品訂單確認

[csp_ConfirmTemperatureOrderSlave]
轉單後溫層商品訂單確認

<br>

### 4.7 更新第三方支付相關單據

csp_UpdateThirdPartyPaymentByTradesOrderGroupId
更新第三方支付相關單據

<br>

### 4.8 寫入發票開立檔

csp_InsertSalesOrderSlaveElectronicInvoiceByTradesOrderGroupId

<br>

### 4.9 看是否建立 csp_SyncOrderSlaveFlowAfterOrderTransToERPDB

[SalesOrderGroup] + [SalesOrder] -> @thirdPartyPaymentTypeDef
[SalesOrderThirdPartyPayment] + @runId  + @thirdPartyPaymentTypeDef -> @thirdPartyPaymentStatusDef

```sql
SET @isNeedSyncFlow = IIF((@thirdPartyPaymentTypeDef = `JKOPay` AND @thirdPartyPaymentStatusDef = `WaitingToPay`),0,1)
```

<br>

---

## 5. 建立費用單

CreateExpenseOrderSlave 

條件是

```csharp
/// 代收：不需處理
/// 直收直付：訂單完成後產生費用單
ExpenseFlowEnum expenseFlow = this._configService.GetAppSetting("ExpenseFlow").ToEnum<ExpenseFlowEnum>();

if (expenseFlow != ExpenseFlowEnum.Direct)
{
    return;
}

//// 也要判定直收付金流
string payProfileTypeDef = this._salesOrderGroupRepository.GetSalesOrderGroupPayProfileTypeDef(tradesOrderGroupId);
bool isEnableCreateExpenseOrder = this._configService.GetAppSetting($"ExpenseOrder.PaymentProcFee.{payProfileTypeDef}.IsEnableCreateExpenseOrder", "false").ToBoolean();
```

<br>

---

## 6. GenerateMyInvoiceTask

NMQ : GenerateMyInvoiceTask

```csharp
if (this._configService.GetAppSetting("Default.Market") == "TW" && orderSlaveFlow.OrderSlaveFlow_ShippingProfileTypeDef == "Oversea")
```

<br>

---

## 7. HK CustomOfflinePayment 特殊邏輯

```csharp
if ("HK".Equals(market, StringComparison.OrdinalIgnoreCase) == true)
{
    string payProfileTypeDef = this._orderSlaveFlowRepository.GetOrderPayProfileTypeDef(tradesOrderGroupId);

    if (nameof(PayProfileTypeDefEnum.CustomOfflinePayment).Equals(payProfileTypeDef, StringComparison.OrdinalIgnoreCase) == true)
    {
        this._taskHelper.CreateTask(NMQJobNameConstants.CreateCustomOfflinePaymentVirtualPayment, tradesOrderGroupId.ToString());
    }
}
```

<br>

---

## 8. 轉單完成後呼叫 PayTypeMappingJob

PayTypeMappingJob 開關 in _shopStaticSetting
NMQ : PayTypeMappingJob

<br>
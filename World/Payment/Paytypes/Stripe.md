# Stripe 文件

## 目錄
1. [異常案例紀錄](#1-異常案例紀錄)
2. [帳戶類型](#2-帳戶類型)
3. [ApplicationFee / Refund / TransferReversal](#3-applicationfee--refund--transferreversal)
4. [系統使用費 / 金流手續費](#4-系統使用費--金流手續費)
5. [publishableKey](#5-publishablekey)
6. [App 設定值處理](#6-app-設定值處理)
7. [第三方金物流 pk + acct 設定根據帳戶類型差異](#7-第三方金物流-pk--acct-設定根據帳戶類型差異)
8. [快取](#8-快取)
9. [Stripe 後台操作](#9-stripe-後台操作)
10. [帳戶類型與 Key 整理](#10-帳戶類型與-key-整理)
11. [文件](#11-文件)
12. [前台 OAuth](#12-前台-oauth)
13. [信用卡付款](#13-信用卡付款)

<br>

---

## 1. 異常案例紀錄

### 1.1 [HK] 特定消費者無法完成信用卡付款_(28) CU APP

**VSTS**：https://91appinc.visualstudio.com/DailyResource/_workitems/edit/512563

<br>

**客戶序號**：25

<br>

**商店序號**：28

<br>

**商店名稱**：CU APP

<br>

**問題描述**

<br>

商戶回報有一位消費者使用 HSBC Master Card 嘗試付款時失敗，畫面顯示「暫不支援此信用卡，請更換信用卡重新結帳」，無法完成交易。消費者表示該卡在其他網購平台使用正常，在我們平台即使已重新輸入多次仍出現錯誤，顧客表示不理解為何只有在我們平台無法使用，情緒激動，希望協助查明原因，謝謝。

<br>

**會員電話號碼**：+852-91832120

<br>

**訂單時間**：2025/07/16 上午約 11:00 至 12:00 之間

<br>

**信用卡類型**：HSBC MasterCard（付款時有跳轉到HSBC銀行App同意授權)

<br>

#### 1.1.1 確認會員資料

```sql
USE WebStoreDB

SELECT *
FROM VipMemberInfo(NOLOCK)
WHERE VipMemberInfo_ValidFlag = 1
AND VipMemberInfo_CellPhone = '91832120'
AND VipMemberInfo_CountryCode = 852
AND VipMemberInfo_ShopId = 28
```

<br>

![alt text](./image.png)

<br>

#### 1.1.2 查詢三方消費紀錄

```sql
SELECT *
FROM TradesOrderThirdPartyPayment(NOLOCK)
WHERE TradesOrderThirdPartyPayment_ValidFlag = 1
AND TradesOrderThirdPartyPayment_ShopId = 28
AND TradesOrderThirdPartyPayment_TypeDef = 'CreditCardOnce_Stripe'
and TradesOrderThirdPartyPayment_CreatedDateTime >= '2025/07/16 00:00'
and TradesOrderThirdPartyPayment_CreatedDateTime <= '2025/07/17 00:00'
and TradesOrderThirdPartyPayment_CreatedUser = '1751836' -- MemberId
```

<br>

![alt text](./image-1.png)

<br>

#### 1.1.3 Stripe 後台資訊

![alt text](./image-2.png)

<br>

![alt text](./image-3.png)

<br>

![alt text](./image-5.png)

<br>

#### 1.1.4 Athena IIS Log 查看信用卡驗證紀錄

![alt text](./image-4.png)

<br>

```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_iislog"
WHERE date = '2025/07/16'
AND cs_uri_stem = '/webapi/CreditCard/Validate'
AND cs_uri_query LIKE '%ShopId=83%';
```

<br>

#### 1.1.5 會員沒有記住信用卡

```sql
select *
from PayTypeExpress(nolock)
where PayTypeExpress_ValidFlag = 1
and PayTypeExpress_ShopId = 28
AND PayTypeExpress_MemberId = 1751836
```

<br>

#### 1.1.6 釐清

交易時間應該是 10:00

<br>

看期限應該是還沒到(2032/01)，不過 Stripe 認定過期

<br>

需要詢問 Stripe 該交易的問題

<br>

---

## 2. 帳戶類型

### Customer vs Standard

<br>

**Customer：** 我們這邊收錢算好費率，用戶只要在osm按按鈕就可以開一個account建立shop

**Standard：** 大型商店，自己去談費率，ex. SASA

<br>

### 帳戶類型清單

<br>

- Custom
- CustomTest
- CustomUAT
- CustomUATTest
- Standard
- StandardUAT

<br>

### 查詢語法

<br>

```sql
select *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_ShopId = @shopId
and ShopDefault_Key = 'StripeAccountType'
```

<br>

### 2.4 前台取得帳號設定

```csharp
public StripeSettingsEntity GetShopStripeSetting(long shopId, bool cleanCache = false)
{
    IEnumerable<ShopDefaultEntity> shopDefault = this.ShopDefaultRepository.GetShopDefaultListByGroupTypeDef(shopId, new List<string> { "Stripe" });

    return new StripeSettingsEntity
    {
        StripeAccountType = shopDefault.SingleOrDefault(x => x.Key == "StripeAccountType").NewValue,
        StripeAccountSettingType = shopDefault.SingleOrDefault(x => x.Key == "StripeAccountSettingType").NewValue,
        StripeSubAccount = shopDefault.SingleOrDefault(x => x.Key == "StripeSubAccount").NewValue,
        StripeCustomSubAccount = shopDefault.SingleOrDefault(x => x.Key == "StripeCustomSubAccount").NewValue,
        StripeCustomTestSubAccount = shopDefault.SingleOrDefault(x => x.Key == "StripeCustomTestSubAccount").NewValue,
        EnableCustomDate = shopDefault.SingleOrDefault(x => x.Key == "EnableCustomDate").NewValue,
    };
}
```

<br>

**執行時期的 Account Type**

<br>

```csharp
private string GetRuntimeAccountType()
{
    //// 確保 Account Type 不會因為時間差而有所變動
    if (string.IsNullOrEmpty(this._runtimeStripeAccountType))
    {
        this._runtimeStripeAccountType = this._stripeAccountType;

        //// Account Type 由 Custom 改為 Standard 的條件
        //// EnableCustomDate 未指定時間
        //// EnableCustomDate 未達指定時間
        if (this._runtimeStripeAccountType.StartsWith(StripeAccountTypeConstants.Custom, StringComparison.OrdinalIgnoreCase)
            && (DateTime.TryParse(this.EnableCustomDate, out DateTime enableCustomDate) == false
                || enableCustomDate > DateTime.Now))
        {
            this._runtimeStripeAccountType = this._runtimeStripeAccountType.Replace(StripeAccountTypeConstants.Custom, StripeAccountTypeConstants.Standard);
            this._runtimeStripeAccountType = this._runtimeStripeAccountType.Replace(StripeAccountTypeConstants.Test, string.Empty);
        }
    }

    return this._runtimeStripeAccountType;
}
```

<br>

**是否為 Custom 類型帳號**

<br>

```csharp
public bool IsCustomAccountType() => this.StripeAccountType.StartsWith(StripeAccountTypeConstants.Custom, StringComparison.OrdinalIgnoreCase) == true;
```

<br>

**根據商店帳號類型取得付款流程**

<br>

```csharp
public string GetStripePaymentFlow()
{
    return this.IsCustomAccountType() == true ? "DestinationCharge" : "DirectCharge";
}
```

<br>

**根據商店 Stripe 帳號類型取得子帳號**

<br>

```csharp
public string GetSubAccount()
{
    return this.IsCustomAccountType() == true ? (this.IsTestMode() ? this.StripeCustomTestSubAccount : this.StripeCustomSubAccount) : this.StripeSubAccount;
}
```

<br>

### 2.5 前台 GetStripeApiKey

```csharp
private string GetStripeApiKey(long shopId, string accountType)
{
    switch (accountType)
    {
        case StripeAccountTypeConstants.Custom:
            return this._stripeConfigurations.CustomAcctLiveSecretKey;

        case StripeAccountTypeConstants.CustomTest:
            return this._stripeConfigurations.CustomAcctTestSecretKey;

        case StripeAccountTypeConstants.CustomUAT:
            return this._stripeConfigurations.CustomUATAcctLiveSecretKey;

        case StripeAccountTypeConstants.CustomUATTest:
            return this._stripeConfigurations.CustomUATAcctTestSecretKey;

        case StripeAccountTypeConstants.Standard:
            return this._stripeConfigurations.StandardAcctLiveSecretKey;

        case StripeAccountTypeConstants.StandardUAT:
            return this._stripeConfigurations.StandardUATAcctLiveSecretKey;

        default:
            return this._stripeConfigurations.StandardAcctLiveSecretKey;
    }
}
```

<br>

---

## 3. ApplicationFee / Refund / TransferReversal

### 適用範圍

<br>

- DestinationCharge 專用

<br>

### 功能說明

<br>

**Refund：** 退款給客戶

<br>

**Transfer Reversal：** 從關聯賬戶收回資金到平台賬戶，可以同時指定是否退還相關的 Application Fee，結果會是增加平台餘額，減少目標賬戶餘額

<br>

### 限制條件

<br>

- 對於目標收費（destination charge），撤銷金額不能超過原始收費金額
- 對於轉賬組（transfer_group），只有在目標賬戶有足夠餘額時才能撤銷

<br>

### 實際案例

<br>

一個電商平台，賣家售出商品價值 $100：

<br>

- 平台收取 10% 應用程式費用（$10）
- 轉給賣家 $90

<br>

**情況 1：需要全額撤銷**

<br>

執行 Transfer Reversal：從賣家賬戶撤回 $90，可以選擇是否同時退還 $10 應用程式費用

<br>

**情況 2：部分撤銷**

<br>

執行部分 Transfer Reversal：比如從賣家賬戶撤回 $45，可以選擇是否按比例退還部分應用程式費用

<br>

### 資金流向說明

<br>

- **Refund：** 客戶 ↔ 商家
- **Application Fee Refund：** 平台 → 賣家（僅涉及費用）
- **Transfer Reversal：** 賣家 → 平台（可包括主要金額和費用）

<br>

---

## 4. 系統使用費 / 金流手續費

![alt text](./image-24.png)

### 4.1 取得 系統使用費 & 金流手續費位置

<br>

**GetOrderProcessingFeeProcessor**

<br>

### 4.2 取得費率的方式

<br>

#### 系統使用費：SalesFeeInfo

<br>

**影響因素**

<br>

- sourceCategoryId
- SupplierId

<br>

**預設：** decimal salesFeeRate = 0.05m

<br>

**Table**

<br>

- SupplierContract
- SupplierContract_IsDefaultSalesFeeRate
- SupplierContractSalesFeeRate
- SupplierContractSalesFeeRate_Rate

<br>

#### 金流手續費：PayProfileFeeInfo

<br>

**影響因素**

<br>

- shopId
- payprofile
- country
- brand

<br>

**CSP：** csp_GetPayProfileProcessingFee

<br>

**最底限會撈 PayProfile**

<br>

- PayProfileProcessingFee_SupplierFeeRate
- PayProfileProcessingFee_SupplierFixedFee

<br>

### 4.3 MWeb 計算費率後帶到 PaymentMiddleware

<br>

**位置：** StripePayChannelService.GetStripeApplicationFee

<br>

將資料帶到 ExtendInfo.application_fee_amount

<br>

**系統使用費：** 加總 SalePageGroup.TotalPayment * salesProcessingFee.Rate（一般是 0.05）

<br>

**運費使用費：** TradesOrderGroup.TradesOrderGroup_TotalFee * salesProcessingFee.Rate

<br>

若是 Custom Type 帳戶類型會再加上金流手續費：

<br>

```
(TradesOrderGroup_TotalPayment * payProfileProcessingFee.Rate) + payProfileProcessingFee.FixedFee
```

<br>

### 4.4 轉單後資料處理

<br>

**轉單 Job：** TransferOrderToERP

<br>

**Step1：** 將WebStoreDB資料轉移至ERPDB暫存表資料表 csp_ImportWebStoreDBTradesOrdersToERPDBSourceTablesByOrderId_Mall

<br>

**Step2：** 壓SalseOrderGroup：dbo.csp_TradesOrderTransToSalesOrderWithFlow_Mall

<br>

**Step3：** 壓SalesOrder：ERPDB.dbo.csp_UpdateDataAfterTradesOrderTransToSalesOrderWithFlow

<br>

#### 帶入參數

<br>

帶 TradesOrderGroupId：

<br>

- @shopPayProfileSupplierFixedFee：PayProfile_SupplierFixedFeePayProfile_SupplierFixedFee 抓這個
- PayProfileProcessingFee_SupplierFixedFee 為主
- @salesOrderGroupPaymentFixedFe
- @cardIssueCountry
- @cardBrand

<br>

#### 資料對應

<br>

執行 UpdateSalesOrderSlaveFeeRateByGroupId 時，會取 ThirdPartyPayment_Info 並 Mapping 為以下：

<br>

- **FeeRate** => SalesOrderSlave_SCMCreditCardFeeRate, SalesOrderSlave_SCMCreditCardFeeRate2
- **SCMSalesFeeRate** => SalesOrderSlave_SCMSalesFeeRate
- **FixedFee** => SalesOrderGroup_PaymentFixedFee

https://bitbucket.org/nineyi/nineyi.databases/pull-requests/7564/diff

<br>

#### 費用單生成

<br>

接著長費用單（ExpenseOrder），費用單（ExpenseOrder）會長其他報表

<br>

### 4.8 手續費退款

#### 4.8.1 判斷是否需要退手續費

<br>

**主要判斷條件**

<br>

```csharp
request.ExtendInfo.IsRefundApplicationFee // Stripe 是否需要退手續費
```

<br>

**檢查條件**

<br>

1. `this.IsSalesOrderFee(refundRequest.RefundRequest_SourceDef)` - 判斷是否為運費
2. `salesOrderSlaveDateTime >= new DateTime(2020, 7, 1) || Config : Charge.SalesOrderFee.Shop`
3. `request.ExtendInfo.ApplicationFeeAmount > 0`

<br>

#### 4.8.2 計算退手續費金額

<br>

**基本計算**

<br>

```csharp
decimal refundApplicationFee = this.CalculateFee(refundAmount, feeRate);
```

<br>

**退貨子單特殊處理**

<br>

若是退貨子單需考慮補收單：

<br>

```csharp
var rechargeAmount = rechargeReceipt.RechargeReceipt_RechargeAmount;
var rechargeAmountFee = this.CalculateFee(rechargeAmount, feeRate);
// 退續費 = TS手續費 - 補收手續費
refundApplicationFee = totalPaymentFee - rechargeAmountFee;
```

<br>

#### 4.8.3 API 調用

<br>

**Stripe API**

<br>

```
[Post("/v1/application_fees/{applicationFeeId}/refunds")]
```

<br>

**文件參考**：https://docs.stripe.com/api/fee_refunds/create

<br>

#### 4.8.4 退款類型差異

<br>

**普通退款 (Refund)**

<br>

- **定義**：將交易金額退還給客戶
- **資金流向**：商家賬戶 → 客戶的支付方式（如信用卡）
- **用途**：客戶要求退款或取消訂單時使用

<br>

**應用程式費用退款 (Application Fee Refund)**

<br>

- **定義**：退還平台從關聯賬戶（如賣家）收取的費用
- **資金流向**：平台賬戶 → 關聯賬戶（賣家）
- **用途**：調整平台和賣家之間的費用分配

<br>

#### 4.8.5 主要區別

<br>

**資金來源和目的地**

<br>

- 普通退款：商家賬戶 → 客戶
- 應用程式費用退款：平台賬戶 → 賣家賬戶

<br>

**影響對象**

<br>

- 普通退款：直接影響客戶
- 應用程式費用退款：影響平台和賣家，不直接影響客戶

<br>

**關聯交易**

<br>

- 普通退款：與客戶的原始支付交易相關
- 應用程式費用退款：與平台收取的費用相關

<br>

#### 4.8.6 使用場景示例

<br>

假設有一筆 $100 的交易，平台收取 10% 的應用程式費用：

<br>

- 客戶支付：$100
- 平台收取應用程式費用：$10
- 賣家收到：$90

<br>

**情況 1：普通退款**

<br>

- 客戶要求全額退款
- 執行普通退款：$100 返還給客戶
- 平台可能同時執行應用程式費用退款：$10 返還給賣家

<br>

**情況 2：僅應用程式費用退款**

<br>

- 平台決定減少收費
- 執行應用程式費用退款：例如 $5 從平台返還給賣家
- 客戶不受影響，交易金額保持不變

<br>

#### 4.8.7 特點與限制

<br>

- **可部分退款**：應用程式費用可以多次部分退款，直到全額退還
- **退款限制**：一旦全額退還，就不能再次退款
- **錯誤處理**：嘗試退還超過原始費用金額會導致錯誤

<br>

#### 4.8.8 在平台經濟中的重要性

<br>

- 允許平台靈活調整其收入模型
- 有助於處理爭議或特殊情況
- 可用於激勵或獎勵表現良好的賣家

<br>

---

## 5. publishableKey

### 5.1 Apple Pay 設定

<br>

**Apple Pay：** Configure the SDK with your Stripe publishable key on app start. This enables your app to make requests to the Stripe API.

<br>

### 5.2 Google Pay 設定

<br>

**Google Pay：** To initialize Stripe in your React Native app, either wrap your payment screen with the StripeProvider component, or use the initStripe initialization method. Only the API publishable key in publishableKey is required. The following example shows how to initialize Stripe using the StripeProvider component.

<br>

### 5.3 publishableKey 功能說明

<br>

publishableKey 是 Stripe SDK 與 Stripe 後台互動的「公開金鑰」。

<br>

Stripe SDK（不論是 Apple Pay、Google Pay 或其他）都需要連接到 Stripe 的伺服器來：

<br>

- **確認商家身份**：確認你是哪一個商家（Merchant）
- **載入付款設定**：根據商家的設定載入相應的付款選項（例如你啟用了 Apple Pay、信用卡等）
- **執行付款操作**：允許產生 PaymentMethod、Token、PaymentIntent 等操作

<br>

而 publishableKey 就是識別你是哪一位商家的方式。

<br>

---

## 6. App 設定值處理

### 6.1 App 設定檔 API

<br>

**API 路徑**：https://shop2.shop.qa1.hk.91dev.tw/webapi/AppNotification/GetMobileAppSettings/2?r=t

<br>

設定值放置於 `ExtendInfo.StripeConfiguration` 節點

<br>

![alt text](./image-2.png)

<br>

### 6.2 PublishableKey

<br>

**資料庫密鑰設定**

<br>

- **DB**：WebstoreDB
- **Table**：ShopSecret
- **Group**：Stripe
- **Key**：{accountType}PublishableKey

<br>

### 6.3 帳戶類型

<br>

**資料庫設定**

<br>

- **DB**：WebStore
- **Table**：ShopDefault
- **Column**：ShopDefault_NewValue

<br>

#### 6.3.1 帳戶類型覆寫問題

<br>

有遇過一次帳戶撈出來是 Custom，但根據程式碼邏輯會被覆寫成 Standard。

<br>

**原因**：Entity 的設定邏輯會確認是否開啟 EnableCustomDate

<br>

**釐清 VSTS**：https://91appinc.visualstudio.com/G11n/_workitems/edit/433159

<br>

#### 6.3.2 Google Pay 帳戶確認

<br>

確認 googlepay CutomUATTest 是否「已激活」

<br>

### 6.4 CountryCode

<br>

**資料庫設定**

<br>

- **Table**：shopStaticSetting

<br>

**設定方式**

<br>

- **一般商店**：shopId = 0, groupName = Stripe, key = ConutryCode
- **美金站**：shopId = 125, groupName = Stripe, key = ConutryCode

<br>

### 6.5 Currency

<br>

**資料來源**：Supplier.SalesMarketCuerrency

<br>

## 6.6 快取

<br>

- **APP 設定值 Server 端的快取**：可以透過 r=t 處理

HK_QA : 

2 號店
https://shop2.shop.qa1.hk.91dev.tw/webapi/AppNotification/GetMobileAppSettings/2?lang=zh-TW&shopId=2&r=t

5 號店
https://cccrrrmmm1.shop.qa1.hk.91dev.tw//webapi/AppNotification/GetMobileAppSettings/5?lang=zh-TW&shopId=5&r=t

125 (美金站)
https://usdshop.shop.qa1.hk.91dev.tw/webapi/AppNotification/GetMobileAppSettings/125?lang=en-US&shopId=125&r=t




- **APP 設定值 BFF 快取**：約 5 分鐘左右

<br>

---

## 7. 第三方金物流 pk + acct 設定根據帳戶類型差異

### 7.1 Custom 帳戶類型

<br>

![alt text](./image-5.png)

<br>

### 7.2 Standard 帳戶類型

<br>

![alt text](./image-6.png)

<br>

---

## 9. Stripe 後台操作

### 9.1 查看 GooglePay / ApplePay 是否 Active

<br>

![alt text](./image-7.png)

<br>

### 9.2 查看帳戶資訊

<br>

![alt text](./image-8.png)

<br>

### 9.3 查看 apiKey

<br>

![alt text](./image-9.png)

<br>

### 9.4 查看 log

<br>

![alt text](./image-10.png)

<br>

### 9.5 目前 QA 連結狀況

<br>

shop = 11 CustomUATTest, 在 QA Custom UAT 的 ConnectedAccount
shop = 2  Standard, 在 91APP HK UAT 的 ConnectedAccount, 帳戶名稱為 91APP HK Limited(ShopId2)


<br>

### 9.6 關鍵字

开发人员
API 密钥
令牌

### 9.7 切換測試模式

![alt text](./image-11.png)

<br>

---

## 10. 帳戶類型與 Key 整理

### 10.1 HK QA 測試商店配置表

<br>

參考 Notion

https://www.notion.so/STRIPE-24e558dd52a9800fb4cfefcc627101d9

<br>

### 10.2 Secret Key 設定位置

<br>

**Config 檔案路徑**：MachineConfig/Frontend/AppSettings.QA300.config

<br>

### 10.3 帳號整理文件

<br>

**參考文件**：https://docs.google.com/spreadsheets/d/1Wc3SB8I2qlHJ5xw2JzrOuXZVUpKAAHs7OsbJAUVeG0A/edit?gid=0#gid=0

<br>

---

## 11. 文件

### 11.1 Stripe Custom Account 需求規劃

<br>

[Stripe Custom Account-需求規劃](https://docs.google.com/presentation/d/1rf8MKdV2Vh6ofZq6repFjUd-2DUYjhpv9I2ppT0rSYI/edit?slide=id.p#slide=id.p)

<br>

---

## 12. 前台 OAuth

StripeOAuthController

<br>

https://connect.stripe.com/oauth/token?client_secret={secret}&code={code}&grant_type=authorization_code

<br>

---

## 13. 信用卡付款

### 13.1 付款前會先檢查卡號

**檔案路徑**：C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\WebAPI\Controllers\CreditCardController.cs

<br>

```csharp
public CreditCardValidationResponseEntity CreditCardValidation(CreditCardValidationRequestEntity infoDate)
{
    CreditCardValidationResponseEntity resultResponse = null;

    var key = this._stripeConfigurations.StandardAcctLiveSecretKey;

    if (infoDate.ShopId.HasValue && infoDate.ShopId.Value > 0)
    {
        StripeSettingsEntity settings = this._shopDefaultService.GetShopStripeSetting(infoDate.ShopId.Value);
        key = this.GetStripeApiKey(infoDate.ShopId.Value, settings.StripeAccountType);
    }

    var pairs = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("type", infoDate.Type),
            new KeyValuePair<string, string>("card[number]", infoDate.Number),
            new KeyValuePair<string, string>("card[exp_month]", infoDate.ExpiryMonth),
            new KeyValuePair<string, string>("card[exp_year]", infoDate.ExpiryYear),
            new KeyValuePair<string, string>("card[cvc]", infoDate.CVC)
        };

    var content = new FormUrlEncodedContent(pairs);
    var apiUrl = new Uri("https://api.stripe.com/v1/payment_methods");
    httpClient.DefaultRequestHeaders.Accept.Clear();
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

    var result = httpClient.PostAsync(apiUrl, content);
    var response = result.Result.Content.ReadAsStringAsync();
    resultResponse = JsonConvert.DeserializeObject<CreditCardValidationResponseEntity>(response.Result);

    return resultResponse;
}
```

<br>

---
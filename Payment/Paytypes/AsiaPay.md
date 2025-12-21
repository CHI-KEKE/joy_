# AsiaPay 文件

## 目錄
1. [PayDollar 整合方式整理](#1-paydollar-整合方式整理)

<br>

---

## 1. PayDollar 整合方式整理

PayDollar 提供三種主要的付款整合方式，這裡先整理前兩種（2.1 與 2.2）。

<br>

### 2.1 Client Post Through Browser（瀏覽器表單提交）

<br>

**概念**：

<br>

商戶直接將客戶導向到 PayDollar 的標準付款頁面，由 PayDollar 完全處理整個付款流程。

<br>

**特點**：

<br>

- 商戶只需建立 HTML 表單，透過 POST 送出必要參數即可
- 不需在自己的網站處理信用卡資訊，安全性由 PayDollar 負責
- 適合想快速上線、無需複雜整合的商戶

<br>

**整合步驟**：

<br>

建立 HTML 表單：

<br>

```html
<form name="payFormCcard" method="post" action="https://test.paydollar.com/b2cDemo/eng/payment/payForm.jsp">
   <input type="hidden" name="merchantId" value="1">
   <input type="hidden" name="amount" value="3000">
   <input type="hidden" name="orderRef" value="000000000014">
   <input type="hidden" name="currCode" value="344">
   <input type="hidden" name="mpsMode" value="NIL">
   <input type="hidden" name="successUrl" value="http://www.yourdomain.com/Success.html">
   <input type="hidden" name="failUrl" value="http://www.yourdomain.com/Fail.html">
   <input type="hidden" name="cancelUrl" value="http://www.yourdomain.com/Cancel.html">
   <input type="hidden" name="payType" value="N">
   <input type="submit" value="Pay Now">
</form>
```

<br>

**URL**：

<br>

- **測試環境**：https://test.paydollar.com/b2cDemo/eng/payment/payForm.jsp
- **正式環境**：https://www.paydollar.com/b2c2/eng/payment/payForm.jsp

<br>

### 2.2 Direct Client Side Connection（直接在商戶頁面收集信用卡資訊）

<br>

**概念**：

<br>

商戶希望在自己的網頁收集客戶信用卡資訊，而不是導向 PayDollar 的標準付款頁面。

<br>

**特點**：

<br>

- 必須在商戶網站安裝 SSL 憑證（128 位 SSL）
- 若信用卡支持 3-D Secure（VISA、MasterCard、JCB、AMEX），客戶可能需要輸入靜態密碼或一次性密碼（OTP）
- 商戶負責前端收集資訊，但資料加密由 SSL 保護
- 適合有能力處理信用卡資訊且需要自訂付款頁面外觀的商戶

<br>

**整合步驟**：

<br>

**1. 前端 (Razor View 範例)**

<br>

```csharp
@model YourViewModelNamespace.YourViewModel

@{
    ViewBag.Title = "Direct Client Side Connection";
}

<!DOCTYPE html>
<html>
<head>
    <title>Direct Client Side Connection</title>
</head>
<body>
    @using (Html.BeginForm("PayComp", "Payment", FormMethod.Post, new { @id = "payForm" }))
    {
        @Html.HiddenFor(model => model.MerchantId)
        @Html.HiddenFor(model => model.Amount)
        @Html.HiddenFor(model => model.OrderRef)
        @Html.HiddenFor(model => model.CurrCode)
        @Html.HiddenFor(model => model.PMethod)
        @Html.HiddenFor(model => model.CardNo)
        @Html.HiddenFor(model => model.SecurityCode)
        @Html.HiddenFor(model => model.CardHolder)
        @Html.HiddenFor(model => model.EpMonth)
        @Html.HiddenFor(model => model.EpYear)
        @Html.HiddenFor(model => model.PayType)
        @Html.HiddenFor(model => model.SuccessUrl)
        @Html.HiddenFor(model => model.FailUrl)
        @Html.HiddenFor(model => model.ErrorUrl)
        @Html.HiddenFor(model => model.Lang)
        @Html.HiddenFor(model => model.SecureHash)
        <input type="submit" value="Pay Now" />
    }
</body>
</html>
```

<br>

**2. 後端 (C# Controller 範例)**

<br>

```csharp
using System.Web.Mvc;
using YourNamespace.YourViewModelNamespace;

public class PaymentController : Controller
{
    public ActionResult DirectClientSideConnection()
    {
        var viewModel = new YourViewModel
        {
            // 初始化資料
        };

        return View(viewModel);
    }

    [HttpPost]
    public ActionResult PayComp(YourViewModel viewModel)
    {
        // 處理付款資料，例如呼叫 PayDollar API
        // viewModel 會包含表單送出的所有資料

        return View();
    }
}
```

<br>

**3. URL**

<br>

- **測試環境**：https://test.paydollar.com/b2cDemo/eng/dPayment/payError.jsp?Ref=&errorMsg=Fail%20Url%20Incorrect
- **正式環境**：https://www.paydollar.com/b2c2/eng/dPayment/payError.jsp?Ref=&errorMsg=Fail%20Url%20Incorrect

<br>

---
# OSM 文件

## 目錄
1. [三方金物流設定相關](#1-三方金物流設定相關)
2. [加解密](#2-加解密)

<br>

---

## 1. 三方金物流設定相關

### 1.1 API

<br>

**儲存**：https://sms.qa1.hk.91dev.tw/Api/ThirdPartyServices/Payment/UpdateSettingDetails

<br>

**顯示**：https://sms.qa1.hk.91dev.tw/Api/ThirdPartyServices/Payment/GetPaymentSettingsInfo?shopId=2

<br>

### 1.2 設計稿

<br>

https://www.figma.com/design/5DvenyhpnBX5ftehBigTEp/Stripe-Apple-Pay-%26-Google-Pay%E4%B8%B2%E6%8E%A5?t=Wn2ZesEZrfsYrld5-0

<br>

### 1.3 新增 GooglePay / ApplePay 與 Stripe 共用

<br>

**PR**：https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33808/diff

<br>

### 1.4 新增 ApplePay 中間狀態

<br>

**PR**：
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33823/diff
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33832/diff

<br>

**設定項目**：Stripe-IsApplePayActive-是否開通ApplePay

<br>

**前端實現**：加 switch case by Key (BL.BE.PayProfile.PayProfileTypeEnum) 顯示「填寫完成，請聯繫專人開通」/「已開通」

<br>

**節點設定**：
- IsSettingsValid === false
- SettingsStatus === NotActive

<br>

**ShopDefault 設定**：
- **Group**：Stripe
- **Key**：IsGooglePayActive  
- **Desc**：是否開通GooglePay

<br>

**修改 GooglePay 邏輯**：
- **PR**：https://bitbucket.org/nineyi/nineyi.sms/pull-requests/34626

<br>

### 1.5 調整 Stripe 設定是否有效判斷

<br>

**PR**：https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33868/diff

<br>

### 1.6 新增 PaymentMethodSetting

<br>

新增以下設定類別：
- GooglePayStripePaymentMethodSetting
- ApplePayStripePaymentMethodSetting
- CreditCardOnceStripePaymentMethodSetting

<br>

**PR**：
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33845/diff
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33907/diff
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33918/diff
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33981/diff

<br>

### 1.7 ShopSecret 歷程

<br>

**PR**：https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33859/diff

<br>

### 1.8 三方金物流設定開關、PayProfile 開關

<br>

**PR**：https://bitbucket.org/nineyi/nineyi.configuration/pull-requests/4016/overview

<br>

**Key**：
- Prod.PayProfileType.GooglePay.EnabledShopIds
- Prod.ThirdParty.PayProfile.EnbaleSetting

<br>

---

## 2. 加解密

### 2.1 加密

<br>

**使用方式**

<br>

```csharp
var siteKey = Aes.Encode(response.KeyName.KeyId);
```

<br>

**加密方法實作**

<br>

```csharp
/// <summary>
/// 取得加密字串
/// </summary>
/// <param name="inputData">欲加密的字串</param>
/// <returns>加密後字串</returns>
public static string Encode(string inputData)
{
    if (string.IsNullOrWhiteSpace(inputData))
    {
        return inputData;
    }

    Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(Key(KeyVersion), new byte[256 / 8]);

    aes.Key = rfc.GetBytes(aes.KeySize / 8);
    aes.IV = rfc.GetBytes(aes.BlockSize / 8);

    byte[] cipherText = null;
    byte[] rawPlaintext = Encoding.Unicode.GetBytes(inputData);
    using (MemoryStream ms = new MemoryStream())
    {
        using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
        {
            cs.Write(rawPlaintext, 0, rawPlaintext.Length);
        }

        cipherText = ms.ToArray();
    }

    string s = Convert.ToBase64String(cipherText);

    return s;
}
```

<br>

### 2.2 解密

<br>

**解密方法實作**

<br>

```csharp
/// <summary>
/// 取得解密字串
/// </summary>
/// <param name="inputData">欲解密的字串</param>
/// <param name="keyVersion">key 版本</param>
/// <returns>解密後字串</returns>
private static string Decode(string inputData, string keyVersion)
{
    string key = Key(keyVersion);
    if (string.IsNullOrEmpty(key))
    {
        return null;
    }

    var keyAndIVCacheName = $"AES-KeyVersion-{KeyVersion}-keyAndIV";

    var keyAndIV = GetCache<Tuple<byte[], byte[]>>(keyAndIVCacheName, () =>
    {
        Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(key, new byte[256 / 8]);
        var aesKey = rfc.GetBytes(_aes.KeySize / 8);
        var aesIV = rfc.GetBytes(_aes.BlockSize / 8);
        return new Tuple<byte[], byte[]>(aesKey, aesIV);
    }, 60);

    _aes.Key = keyAndIV.Item1;
    _aes.IV = keyAndIV.Item2;

    byte[] cipherText = Convert.FromBase64String(inputData);
    byte[] plainText = null;
    using (MemoryStream ms = new MemoryStream())
    {
        try
        {
            using (CryptoStream cs = new CryptoStream(ms, _aes.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(cipherText, 0, cipherText.Length);
            }
        }
        catch
        {
            ////無法解密
            return null;
        }

        plainText = ms.ToArray();
    }

    string s = Encoding.Unicode.GetString(plainText);
    return s;
}
```

<br>

---
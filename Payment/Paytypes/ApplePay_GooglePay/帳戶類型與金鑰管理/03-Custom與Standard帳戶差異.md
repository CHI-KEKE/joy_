# Custom 與 Standard 帳戶差異

## 帳戶類型對比

| 項目 | Custom 帳戶 | Standard 帳戶 |
|------|------------|---------------|
| **付款流程** | DestinationCharge | DirectCharge |
| **費用處理** | 支援 ApplicationFee | 無 ApplicationFee |
| **帳戶設定** | 需要指定子帳戶 (SubAccount) | 直接使用主帳戶 |

---

## PublishableKey 對應表

| 帳戶類型 | 環境 | PublishableKey |
|----------|------|----------------|
| Custom | Live | `CustomPublishableKey` |
| Custom | Test | `CustomTestPublishableKey` |
| Custom | UAT | `CustomUATPublishableKey` |
| Standard | Live | `StandardPublishableKey` |
| Standard | UAT | `StandardUATPublishableKey` |

---

## SecretKey 對應表

| 帳戶類型 | 環境 | SecretKey |
|----------|------|-----------|
| Custom | Live | `CustomAcctLiveSecretKey` |
| Custom | Test | `CustomAcctTestSecretKey` |
| Custom | UAT | `CustomUATAcctLiveSecretKey` |
| Standard | Live | `StandardAcctLiveSecretKey` |
| Standard | UAT | `StandardUATAcctLiveSecretKey` |

---

## Custom 帳戶設定細節

![alt text](../../Img/image-5.png)

| 設定項目 | 值 | 說明 |
|----------|-----|------|
| **PublishableKey** | `CustomPublishableKey` | 前端 SDK 使用的公開金鑰 |
| **SecretKey** | `CustomAcctLiveSecretKey` | 後端 API 使用的私密金鑰 |
| **SubAccount** | `StripeCustomSubAccount` | 指定的子帳戶 ID |
| **PaymentFlow** | `DestinationCharge` | 使用目的地收費模式 |

---

## Standard 帳戶設定細節

| 設定項目 | 值 | 說明 |
|----------|-----|------|
| **PublishableKey** | `StandardPublishableKey` | 前端 SDK 使用的公開金鑰 |
| **SecretKey** | `StandardAcctLiveSecretKey` | 後端 API 使用的私密金鑰 |
| **SubAccount** | `StripeSubAccount` | 標準帳戶 ID |
| **PaymentFlow** | `DirectCharge` | 使用直接收費模式 |

---

## 動態帳戶類型切換邏輯

系統會根據 `EnableCustomDate` 設定，動態決定實際使用的帳戶類型：

```csharp
public string GetRuntimeAccountType()
{
    var currentType = GetConfiguredAccountType();

    //// 檢查 EnableCustomDate 設定，若未達指定時間則自動切換為 Standard
    if (currentType.StartsWith("Custom") && !IsCustomDateEnabled())
    {
        return currentType.Replace("Custom", "Standard").Replace("Test", "");
    }

    return currentType;
}
```

| 切換情況 | 原類型 | 實際使用類型 | 影響 |
|----------|--------|-------------|------|
| EnableCustomDate 時間未到 | Custom | Standard | 金鑰、付款流程自動切換 |
| EnableCustomDate 時間已到 | Custom | Custom | 維持原設定 |
| 標準帳戶 | Standard | Standard | 無影響 |

> **相關問題追蹤**：[VSTS #433159](https://91appinc.visualstudio.com/G11n/_workitems/edit/433159)

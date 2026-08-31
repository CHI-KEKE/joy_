# Bug：AWSSDK.SecurityToken 缺失導致 QA DynamoDB 查詢失敗

## 環境

- **專案**：`nine1.regularpurchase.v2.worker`
- **Job**：`RegularOrderInitJob`
- **環境**：QA（K8s）
- **發現日期**：2026-06-15

---

## 現象

QA 環境執行 `RegularOrderInitJob` 時，呼叫 DynamoDB 查詢 `ShareSecretManagement` 失敗，Log 報錯：

```
System.InvalidOperationException: Assembly AWSSDK.SecurityToken could not be found or loaded.
This assembly must be available at runtime to use Amazon.Runtime.AssumeRoleAWSCredentials.
 ---> System.TypeLoadException: Could not load type 'Amazon.Runtime.StsRegionalEndpointsValue'
 from assembly 'AWSSDK.Core, Version=4.0.0.0'
```

完整 Log：

```json
{
  "_ts": "2026-06-15T02:27:05.8337764Z",
  "_msg": "查詢共享密鑰管理資料發生錯誤，ShopId: 10230, PK: \"Shop#10230\", SK: \"SupplierApiProfile#0\"",
  "_lvl": "Error",
  "_ext": "System.InvalidOperationException",
  "_ex": "System.InvalidOperationException: Assembly AWSSDK.SecurityToken could not be found or loaded..."
}
```

> 本機（Dev）執行**不會**發生此錯誤。

---

## 根本原因

### 錯誤鏈

```
QA K8s Pod 使用 IRSA（IAM Role for Service Account）
    │
    ▼
K8s 將 JWT Token 注入 Pod 環境變數
（AWS_WEB_IDENTITY_TOKEN_FILE / AWS_ROLE_ARN）
    │
    ▼
AWS SDK 偵測到環境變數，自動選擇
AssumeRoleWithWebIdentityCredentials
    │
    ▼
該 Credential Provider 需要動態載入 AWSSDK.SecurityToken
    │
    ▼
❌ csproj 沒有引用 AWSSDK.SecurityToken → Runtime 找不到 → 爆錯
```

### 為什麼本機不爆

本機設定 `N1_LOCAL_MODE=True`，AWS SDK 直接讀 `_N1SECRETS:AWS:DynamoDB:AccessKey/SecretKey`，**跳過 IRSA 流程**，不需要 SecurityToken。

### 套件職責說明

| 套件 | 職責 |
|------|------|
| `AWSSDK.DynamoDBv2` | 操作 DynamoDB 的 API |
| `AWSSDK.KeyManagementService` | 操作 KMS 的 API |
| `AWSSDK.SecurityToken` | **負責向 AWS STS 交換臨時憑證**，是 IRSA 流程的必要前置步驟 |

`DynamoDBv2` 和 `KeyManagementService` 本身不含 STS 邏輯，必須由 `SecurityToken` 提供。

---

## 解法

在 `Nine1.Regularpurchase.V2.Worker.Console.NMQv3Worker.csproj` 補上：

```xml
<PackageReference Include="AWSSDK.SecurityToken" Version="4.0.7.4" />
```

> **版本說明**：`AWSSDK.SecurityToken` 各套件版本號各自獨立，`4.0.7.4` 為目前 NuGet 上可用的最新版本（`DynamoDBv2` 為 `4.0.10.1`，版號不需一致）。

### 修改後 csproj 相關片段

```xml
<PackageReference Include="AWSSDK.DynamoDBv2" Version="4.0.10.1" />
<PackageReference Include="AWSSDK.Extensions.NETCore.Setup" Version="4.0.3.14" />
<PackageReference Include="AWSSDK.KeyManagementService" Version="4.0.8.3" />
<PackageReference Include="AWSSDK.SecurityToken" Version="4.0.7.4" />
```

---

## 驗證

加入套件後執行 `dotnet build`，0 Error，部署至 QA 後 DynamoDB 查詢正常。

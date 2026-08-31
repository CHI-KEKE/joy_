# Enum 重構完成報告

## 🎯 重構目標
將硬編碼的通知類型字符串統一改用 **Enum**，提升代碼的類型安全性和可維護性。

---

## ✅ 完成項目

### 1. 創建 RegularOrderNotificationType Enum
**位置**：`Contract/RegularOrderNotificationType.cs`

#### Enum 定義（5 種通知類型）
```csharp
public enum RegularOrderNotificationType
{
    /// <summary>信用卡訂單通知 - D-3 提醒扣款</summary>
    CreditCardOrderNotify,

    /// <summary>貨到付款訂單通知 - D-3 提醒準備現金</summary>
    CashOnDeliveryOrderNotify,

    /// <summary>信用卡失敗通知 - D-1 提醒失敗</summary>
    CreditCardFailedNotify,

    /// <summary>建單失敗通知 - D-1 提醒失敗</summary>
    CreateFailedNotify,

    /// <summary>庫存預估通知 - 每週一給商店</summary>
    StockNotify
}
```

#### 擴展方法
- `ToApiString()`：將 Enum 轉換為 API 字符串（例如 `"CreditCardOrderNotify"`）
- `FromApiString(string)`：從 API 字符串反向轉換為 Enum

### 2. 更新 RegularCreateOrderNotifyJob

#### 移除常數字符串
```csharp
// ❌ 舊做法
private const string CreditCardNotificationType = "CreditCardOrderNotify";
private const string CashOnDeliveryNotificationType = "CashOnDeliveryOrderNotify";
```

#### 更新方法簽名
```csharp
// ✅ 新做法
private (RegularOrderNotificationType? notificationType, DateTime payDateTime) 
    DetermineNotificationTypeAndPayDate(RegularOrderForNotificationDto order)
{
    if (order.PayType.Contains("CreditCardOnce"))
        return (RegularOrderNotificationType.CreditCardOrderNotify, payDateTime);
    
    if (order.PayType.Contains("CashOnDelivery"))
        return (RegularOrderNotificationType.CashOnDeliveryOrderNotify, payDateTime);
    
    return (null, DateTime.MinValue);
}
```

#### 使用位置更新
1. **生成通知時**
   ```csharp
   var notificationTypeStr = notificationType.Value.ToApiString();
   var existingQueue = await _queueRepo.GetByOrderAndTypeAsync(
       order.RegularOrderPk.ToString(), notificationTypeStr, ct);
   ```

2. **發送通知時**
   ```csharp
   var creditCardNotifications = await _queueRepo.GetUnsentNotificationsAsync(
       RegularOrderNotificationType.CreditCardOrderNotify.ToApiString(), 
       MaxNotificationsPerRun, ct);
   
   var cashOnDeliveryNotifications = await _queueRepo.GetUnsentNotificationsAsync(
       RegularOrderNotificationType.CashOnDeliveryOrderNotify.ToApiString(),
       MaxNotificationsPerRun, ct);
   ```

---

## 🔄 代碼流程變化

### 原始流程（字符串硬編碼）
```
Determine PayType
  ├─ 返回: ("CreditCardOrderNotify", payDateTime) 
  └─ 返回: ("CashOnDeliveryOrderNotify", payDateTime)
      ↓
傳入 Repository 查詢
  └─ 字符串匹配（容易拼寫錯誤）
      ↓
序列化到 JSON
  └─ 直接使用字符串
```

### 重構後流程（Enum 安全）
```
Determine PayType
  ├─ 返回: (RegularOrderNotificationType.CreditCardOrderNotify, payDateTime)
  └─ 返回: (RegularOrderNotificationType.CashOnDeliveryOrderNotify, payDateTime)
      ↓
轉換為 API 字符串
  ├─ notificationType.ToApiString()
  └─ 型別安全 + 自動轉換
      ↓
傳入 Repository 查詢
  └─ 使用 API 字符串（統一管理）
      ↓
序列化到 JSON
  └─ 使用 API 字符串
```

---

## 💡 優勢對比

| 面向 | 舊做法（字符串） | 新做法（Enum） |
|------|------------------|----------------|
| **類型安全** | ❌ 無 | ✅ 編譯時檢查 |
| **拼寫錯誤** | ⚠️ 容易出錯 | ✅ 不可能出錯 |
| **自動完成** | ❌ 需手動輸入 | ✅ IDE 自動完成 |
| **擴展性** | ⚠️ 容易遺漏位置 | ✅ 集中定義 |
| **重構安全** | ❌ 需全文本搜索 | ✅ IDE 自動重構 |
| **文檔** | ⚠️ 分散各地 | ✅ Enum 集中文檔 |

---

## 🧪 測試範圍

### 需要驗證的案例

```csharp
// Enum 到 API 字符串轉換
[Test]
public void ToApiString_CreditCardOrderNotify_ReturnsCorrectString()
{
    Assert.AreEqual("CreditCardOrderNotify", 
        RegularOrderNotificationType.CreditCardOrderNotify.ToApiString());
}

[Test]
public void ToApiString_CashOnDeliveryOrderNotify_ReturnsCorrectString()
{
    Assert.AreEqual("CashOnDeliveryOrderNotify", 
        RegularOrderNotificationType.CashOnDeliveryOrderNotify.ToApiString());
}

// API 字符串到 Enum 反向轉換
[Test]
public void FromApiString_CreditCardOrderNotify_ReturnsCorrectEnum()
{
    var result = RegularOrderNotificationTypeExtensions.FromApiString("CreditCardOrderNotify");
    Assert.AreEqual(RegularOrderNotificationType.CreditCardOrderNotify, result);
}

// Job 中的 DetermineNotificationTypeAndPayDate 方法
[Test]
public void DetermineNotificationTypeAndPayDate_CreditCardOrder_ReturnsCreditCardType()
{
    var (notificationType, payDateTime) = job.DetermineNotificationTypeAndPayDate(creditCardOrder);
    Assert.AreEqual(RegularOrderNotificationType.CreditCardOrderNotify, notificationType);
}

[Test]
public void DetermineNotificationTypeAndPayDate_CashOnDeliveryOrder_ReturnsCashOnDeliveryType()
{
    var (notificationType, payDateTime) = job.DetermineNotificationTypeAndPayDate(codOrder);
    Assert.AreEqual(RegularOrderNotificationType.CashOnDeliveryOrderNotify, notificationType);
}
```

---

## 📋 編譯驗證

```
✅ dotnet build 成功
✅ 無編譯錯誤
✅ 所有引用已正確更新
```

---

## 🚀 未來擴展

此 Enum 設計已為 Phase 2 和 Phase 3 預留空間：

```csharp
// Phase 2（已在 Enum 中定義）
RegularOrderNotificationType.CreditCardFailedNotify
RegularOrderNotificationType.CreateFailedNotify

// Phase 3（已在 Enum 中定義）
RegularOrderNotificationType.StockNotify
```

當實現後續 Phase 時，只需：
1. ✅ 新建 Job 類別
2. ✅ 使用 Enum 而無需修改 Enum 定義
3. ✅ 自動獲得類型安全保障

---

## 📄 相關文件

| 文件 | 用途 | 狀態 |
|------|------|------|
| `Contract/RegularOrderNotificationType.cs` | Enum 定義 + 擴展方法 | ✅ 新建 |
| `Jobs/RegularCreateOrderNotifyJob.cs` | 使用 Enum 的 Job | ✅ 更新 |

---

**版本**：1.1
**完成日期**：2026-06-23
**改進**：字符串常數 → Enum（型別安全）

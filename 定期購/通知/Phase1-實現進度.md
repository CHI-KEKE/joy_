# Phase 1：建單前通知 Job - 實現進度

## 🎯 目標
翻新 Job 名稱，統一信用卡和貨到付款的通知邏輯為單一 Job。

## ✅ 完成清單

### 1. Job 類別重命名
- **舊名稱**：`RegularOrderCreditCardNotifyJob`
- **新名稱**：`RegularCreateOrderNotifyJob`
- **位置**：`Jobs/RegularCreateOrderNotifyJob.cs`
- **狀態**：✅ 新文件已創建

### 2. 新增貨到付款邏輯
- ✅ 提取核心方法：`DetermineNotificationTypeAndPayDate()`
  - 根據 `PayType` 自動判斷通知類型
  - 自動計算 `PayDateTime`（信用卡 D-2、貨到付款 D）
  - 返回 `(notificationType, payDateTime)` 元組

- ✅ 信用卡邏輯
  - 條件：`PayType.Contains("CreditCardOnce")`
  - 通知類型：`CreditCardOrderNotify`
  - PayDateTime：`ShippingDate - 2 天`（刷卡日期）

- ✅ 貨到付款邏輯
  - 條件：`PayType.Contains("CashOnDelivery")`
  - 通知類型：`CashOnDeliveryOrderNotify`
  - PayDateTime：`ShippingDate`（收貨日期）

### 3. 統一化佇列生成流程
- ✅ 單一 `GenerateNotifyQueuesAsync()` 方法處理兩種支付方式
- ✅ 根據 `DetermineNotificationTypeAndPayDate()` 結果生成對應通知
- ✅ 共用防重複檢查邏輯

### 4. 統一化通知發送流程
- ✅ 單一 `SendNotificationsAsync()` 方法處理兩種通知類型
- ✅ 同時查詢兩種未發送通知
- ✅ 逐筆發送，自動從 JSON 讀取 `Type` 欄位

### 5. DI Attribute 更新
- **舊名稱**：`RegularOrderCreditCardNotifyJobDIAttribute`
- **新名稱**：`RegularCreateOrderNotifyJobDIAttribute`
- **位置**：`DI/RegularCreateOrderNotifyJobDIAttribute.cs`
- **狀態**：✅ 新文件已創建

### 6. 編譯驗證
- ✅ `dotnet build` 成功
- ✅ 無編譯錯誤
- ✅ 無與新 Job 相關的警告

---

## 📝 關鍵變更

### 方法簽名變更
```csharp
// 新增核心判斷方法
private (string notificationType, DateTime payDateTime) DetermineNotificationTypeAndPayDate(RegularOrderForNotificationDto order)
{
    if (order.PayType.Contains("CreditCardOnce"))
    {
        var payDate = order.NextShipDate.AddDays(-2);
        return (CreditCardNotificationType, new DateTime(payDate.Year, payDate.Month, payDate.Day));
    }
    
    if (order.PayType.Contains("CashOnDelivery"))
    {
        return (CashOnDeliveryNotificationType, new DateTime(order.NextShipDate.Year, order.NextShipDate.Month, order.NextShipDate.Day));
    }
    
    return (null, DateTime.MinValue);
}
```

### 使用方式
```csharp
var (notificationType, payDateTime) = DetermineNotificationTypeAndPayDate(order);
if (notificationType == null)
{
    // 跳過不支持的支付方式
    continue;
}
```

---

## 🏗️ 架構變化

### 舊架構（分離）
```
RegularOrderCreditCardNotifyJob
├─ 只處理信用卡訂單
├─ 固定查詢 D-2
└─ 產生 CreditCardOrderNotify

(缺少貨到付款支持)
```

### 新架構（統一）
```
RegularCreateOrderNotifyJob
├─ 查詢 D-3 訂單（為建單預告）
├─ 根據支付方式分流
│  ├─ CreditCardOnce → CreditCardOrderNotify (D-2)
│  └─ CashOnDelivery → CashOnDeliveryOrderNotify (D)
└─ 統一防重複、發送流程
```

---

## 🧪 測試要點

### 單元測試
- [ ] `DetermineNotificationTypeAndPayDate()` 返回正確類型和日期
  - 信用卡訂單 → (CreditCardOrderNotify, ShippingDate-2)
  - 貨到付款訂單 → (CashOnDeliveryOrderNotify, ShippingDate)
  - 未知支付方式 → (null, DateTime.MinValue)

### 集成測試
- [ ] 信用卡訂單正確寫入 notify_queue
  - Type = CreditCardOrderNotify
  - PayDateTime = D-2
  
- [ ] 貨到付款訂單正確寫入 notify_queue
  - Type = CashOnDeliveryOrderNotify
  - PayDateTime = D
  
- [ ] 混合訂單正確分流
  - 多個信用卡訂單 + 多個貨到付款訂單
  - 驗證各自被正確標記和發送

- [ ] 防重複邏輯生效
  - 同一訂單同類型只寫一次

---

## 📄 相關文件

| 文件 | 用途 | 狀態 |
|------|------|------|
| `Jobs/RegularCreateOrderNotifyJob.cs` | Job 主類別 | ✅ 新建 |
| `DI/RegularCreateOrderNotifyJobDIAttribute.cs` | DI 註冊 | ✅ 新建 |
| `Jobs/RegularOrderCreditCardNotifyJob.cs` | 舊文件（保留參考） | 📌 待刪除 |
| `DI/RegularOrderCreditCardNotifyJobDIAttribute.cs` | 舊 DI（保留參考） | 📌 待刪除 |

---

## 🚀 下一步

### 立即跟進
1. ✅ 刪除舊的 `RegularOrderCreditCardNotifyJob.cs` 和相應 DI
2. ✅ 確認應用程式註冊新的 DI Attribute
3. ✅ 執行單元測試

### 後續 Phase
4. **Phase 2**：RegularOrderNotifyFailedJob（D-1 失敗通知）
5. **Phase 3**：RegularOrderStockEstimateNotifyJob（週一庫存通知）

---

## 📊 編譯結果

```
Build: Successful ✅
Errors: 0
Warnings: (NU1608 - 預期的依賴版本不一致，不影響功能)
```

---

**版本**：1.0
**實現日期**：2026-06-23
**實現者**：Copilot CLI

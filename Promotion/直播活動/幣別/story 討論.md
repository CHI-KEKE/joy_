# 幣別支援 Story 討論

## 核心目標

將直播站台從「港幣寫死」的架構，轉向「**動態市場判斷**」的多幣別架構。

---

## 覆蓋範圍

- **場景**：活動前（管理頁）、活動中（直播間、留言訊息）、活動後（成效報表）
- **深度**：不僅是顯示幣別符號（HK$ / NT$ / RM），更包含後端數值計算的精度控制（台幣 0 位、港馬幣 2 位）與千分位格式化
- **判斷基準**：以「商家銷售市場」作為顯示邏輯的切換開關，目前支援香港、台灣、馬來西亞三地
- **技術關鍵**：需確保前端顯示（Figma 設計稿實作）與後端傳遞的數值（精確度與四捨五入邏輯）在各報表與彈窗中保持一致

---

## 待討論問題

### 1. 新幣站的幣別顯示邏輯

新幣站（例如 SGD）是否沿用 Shopping 現有的幣別顯示邏輯？

### 2. 折扣精度

當金額因折扣計算產生小數（例如 $99.5 元），但該商店市場為「台灣 (TWD)」（應為整數），處理方式應為：無條件捨去、四捨五入、還是無條件進位？

> 目前格式化方式：`decimal.ToString("N2")` → 兩位小數；`"N0"` → 0 位小數（整數）

### 3. 幣別符號與金額間的空格規範

在「商品推薦訊息」與「加入購物車訊息」等純文字訊息中，幣別符號與金額之間是否有空格？

| 市場 | 無空格 | 有空格 |
|------|--------|--------|
| 香港 | `HK$100.00` | `HK$ 100.00` |
| 馬來西亞 | `RM100.00` | `RM 100.00` |

---

## 實作方向

`salesMarket` 套件已提供所需資訊（`digit` / `symbol` / `market`），可直接取用，不需要自行維護。

### ToCurrencyAsync 方法

```csharp
/// <summary>
/// 將金額轉換為包含幣別符號的字串
/// </summary>
/// <param name="shopId">商店序號</param>
/// <param name="amount">金額數字</param>
/// <param name="enableSymbol">是否顯示貨幣符號</param>
/// <returns>包含幣別符號的金額字串</returns>
/// <example>NT$1,000</example>
public async Task<string> ToCurrencyAsync(
    long shopId,
    decimal amount,
    bool enableSymbol = true)
{
    var salesMarketSettings = (await this._marketModuleAgentService.GetShopSalesMarketSettingsCacheAsync(shopId))!;
    var amountString = amount.ToString($"N{salesMarketSettings.CurrencyDecimalDigits}");
    var symbol = enableSymbol ? salesMarketSettings.CurrencySymbol : string.Empty;

    return $"{symbol}{amountString}";
}
```

---

## 幣別對照表

| Currency Code | Currency Symbol |
|---------------|-----------------|
| TWD | NT$ |
| USD | US$ |
| HKD | HK$ |
| MOP | P |
| CNY | ¥ |
| MYR | RM |
| SGD | S$ |
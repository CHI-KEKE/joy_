# Bug 案例：AdditionalDiscount 負值導致結帳失敗

## 問題描述

**錯誤訊息：**
```
ValidationException: 'Trades Order Slave_ Total Discount' must be less than or equal to '0'.
```

**發生環境：** HK ShopId:17  
**用戶：** MemberId:3828207  
**觸發路徑：** 用戶在 webstore App 結帳（`/api/checkout/complete`）

---

## 問題的 Bundle 數據

### 正常組合（其他 4 組）

| Bundle | 子商品原價合計 | Bundle 售價 | additionalDiscount | 結果 |
|--------|------------|------------|-------------------|------|
| 590    | 72         | 64         | **+8**            | ✅   |
| 436    | 220        | 180        | **+40**           | ✅   |
| 533    | 144        | 132        | **+12**           | ✅   |
| 588    | 144        | 135        | **+9**            | ✅   |

### 問題組合（Bundle 638）

| 角色 | SkuId | Price | Qty | 小計 | SuggestPrice |
|------|-------|-------|-----|------|-------------|
| 主商品 (Major) | 4104441 | 160 | 2 | **320** | 276 |
| 子商品 (Sub) | 4090706 | 79 | 4 | **316** | 138 |

```
additionalDiscount = 子商品合計 - 主商品合計
                   = 316 - 320
                   = -4  ← 負值！
```

子商品促購 API 回傳：
```json
{
  "additionalDiscount": -4,
  "totalPrice": 316.00,
  "totalPayment": 320.00  ← totalPayment > totalPrice，代表要加費
}
```

---

## 根本原因：定價設定問題

子商品的 SuggestPrice 原本是 138，現在實際售價只剩 79（打約 57 折）。  
但 bundle 的主商品售價（320）是當初設定的，**沒有跟著子商品降價更新**。

```
SuggestPrice 時代（正常）：
子商品合計 138 × 4 = 552  >  bundle 售價 320  → 有折扣

實際售價（問題）：
子商品合計  79 × 4 = 316  <  bundle 售價 320  → 反而要加費 4 元
```

> **買這個 bundle 比單獨買子商品還貴 4 元**，出現了加費（負折扣）情境。

---

## Bug 傳導鏈

```
束 bundle 設定異常：子商品單品售價合計(316) < 主商品 bundle 定價(320)
  ↓
PromotionFrontend API
  SetSalepageBundleAdditionalDiscount():
  additionalDiscount = 316 - 320 = -4

  ↓
Cart AmortizeSalepageBundleProcessor line 198:
  salepage.AdditionalDiscount = -4   ← 直接賦值，無保護

  ↓
OldCartMapperService line 882:
  AdditionalDiscount *= -1
  -4 × -1 = +4   ← 本來是「加費」，negation 後變成「正折扣」，方向錯誤！

  ↓
mweb TotalDiscount = ... + (+4) > 0

  ↓
TradesOrderSlaveEntityValidator line 44:
  TotalDiscount <= 0  → 驗證失敗！💥
  拋出 ValidationException
```

---

## 設計假設衝突

| 元件 | 假設 |
|------|------|
| `OldCartMapperService` negation | Cart 端折扣欄位**一定是正值** |
| `AmortizeSalepageBundleProcessor` | 直接接受促購 API 的值，**無符號驗證** |
| 促購服務 `SetSalepageBundleAdditionalDiscount` | 負值為合理計算結果（加費情境） |
| `TradesOrderSlaveEntityValidator` | `TotalDiscount` **一定 <= 0** |

---

## 可能的修復方向

### 方案 A：在 Cart 端保護（AmortizeSalepageBundleProcessor）
```csharp
// 確保 AdditionalDiscount 不為負值
salepage.AdditionalDiscount = Math.Max(0, salepageSku.AdditionalDiscount);
```
缺點：掩蓋了問題，加費 4 元被吃掉，定價邏輯不正確。

### 方案 B：在 OldCartMapper negation 前加判斷
```csharp
// 若已是負值則不 negate（代表本來就是加費）
if (salePage.AdditionalDiscount > 0)
    salePage.AdditionalDiscount *= -1;
```
缺點：改變了 negation 的語義，影響範圍需評估。

### 方案 C：修正促購服務，加費情境不走 additionalDiscount
促購服務對「子商品合計 < 主商品定價」的情況做特別處理，不允許負值回傳。

### 方案 D（治本）：後台修正 bundle 定價
確保 bundle 售價永遠 ≤ 子商品單品售價合計，避免加費情境發生。

---

## 相關 Validator 歷史問題

不同版本的 Helper 對 TotalDiscount 方向的期望**相反**：

| 位置 | TotalDiscount 期望 | 說明 |
|------|---------------------|------|
| `TradesOrderHelper` line 500（舊） | `>= 0`（正數） | 舊版 |
| `TradesOrderHelperV2` line 716 | `<= 0`（負數） | 新版 |
| `TradesOrderSlaveEntityValidator` line 44 | `<= 0`（負數） | 本次失敗 |

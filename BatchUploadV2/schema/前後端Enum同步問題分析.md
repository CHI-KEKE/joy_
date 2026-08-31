# 前後端 Enum 同步問題分析

> 本文件分析 BatchUpload 批次類型（`BatchUploadTypeDefEnum`）在前後端的傳遞機制，說明現有設計的成因、現狀問題，以及各種改善方向。

---

## 一、現況：三個地方各自存一份

```bash
C# BatchUploadTypeDefEnum          TypeScript Enums.ts             DB Definition Table
(自動遞增 int，不寫死)              (明確寫死每個 int 值)            (Definition_Code = 字串)
──────────────────────             ─────────────────────           ───────────────────────
SalePageCreate,          ←──→    SalePageCreate = 0,      X──→   "SalePageCreate"
YMallSalePageCreate,     ←──→    YMallSalePageCreate = 1, X──→   "YMallSalePageCreate"
...                               ...
BatchCreateBlackList,    ←──→    BatchCreateBlackList = 23,       "BatchCreateBlackList"
```

- **C# ↔ TypeScript**：靠人工同步，API payload 傳遞 **int（如 23）**
- **TypeScript ↔ DB**：不直接互動；DB Definition 只給清單查詢用（字串）
- **C# ↔ DB**：DB 存字串，C# enum 的 `.ToString()` 對應

---

## 二、成因脈絡

### 2.1 為什麼 API payload 用 int？

ASP.NET 預設的 `Newtonsoft.Json` 對 enum 欄位的序列化行為是 **int**，除非額外加 `[JsonConverter(typeof(StringEnumConverter))]`。

`BatchRequestEntity.BatchUploadType` 的型別是 `BatchUploadTypeDefEnum`，**沒有加任何 Converter**，因此：

```csharp
// BatchRequestEntity.cs
public BatchUploadTypeDefEnum BatchUploadType { get; set; }
// 序列化結果 → {"BatchUploadType": 23}，而非 {"BatchUploadType": "BatchCreateBlackList"}
```

### 2.2 為什麼 TypeScript 要有一份 Enums.ts？

前端程式碼這樣寫：

```typescript
// batchCreateBlackList 頁面
BatchUploadType: BL.BE.BatchUploads.Enums.BatchUploadTypeDefEnum.BatchCreateBlackList
```

TypeScript 需要知道 `BatchCreateBlackList` 對應的數字是多少，才能在 API payload 送出正確的 int。  
所以 `Scripts/Enums.ts` 手動維護了一份完整的 TypeScript enum，明確寫死每個 int 值：

```typescript
// Scripts/Enums.ts（手工維護，共數百個 enum member）
module NineYi.Sms.BL.BE.BatchUploads.Enums {
    export enum BatchUploadTypeDefEnum {
        SalePageCreate = 0,
        ...
        BatchCreateBlackList = 23,
        ...
    }
}
```

### 2.3 為什麼 C# enum 沒有明確寫死 int 值？

C# enum 如果不寫值，編譯器自動從 0 遞增。歷史上這批 Enum 建立時沒有考慮到 API 合約穩定性，直接使用預設行為。目前 C# 端 **沒有任何一個成員明確寫 int 值**，完全依賴排列順序。

---

## 三、現有設計的問題

### 問題一：三個地方必須手動同步

新增一個批次類型，必須同時改：

| 位置 | 改什麼 |
|------|--------|
| C# `BatchUploadTypeDefEnum` | 加 enum member（接在最後） |
| TypeScript `Enums.ts` | 加同名 member 並寫正確的 int 值 |
| DB `Definition` Table | INSERT 一筆 Definition_Code（字串） |

**沒有任何編譯期或測試期的保護機制**確保三者一致。

### 問題二：C# enum 順序是 API 合約的一部分

因為 C# enum 沒有明確 int 值，**成員的排列順序直接決定 API 的行為**：

```bash
❌ 錯誤示範：在中間插入新類型
  BatchRemoveSalePage = 21
  ★ MyNewType = 22  （插在這裡）
  BatchUpdateCuratorSort = 23  （原本 22，現在變 23）
  BatchCreateBlackList = 24   （原本 23，現在變 24）

→ 前端 Enums.ts 還是寫 BatchCreateBlackList = 23
→ 前端以為在建立黑名單，實際上變成 UpdateCuratorSort
→ 靜默 bug，無任何錯誤提示
```

### 問題三：Enums.ts 打錯數字，靜默失敗

`Enums.ts` 是手工維護，若數字打錯（或 copy-paste 錯誤）：

```typescript
BatchCreateBlackList = 32,  // 應該是 23，手滑打成 32
```

TypeScript 編譯**完全不會報錯**，前端送出 `32`（`BatchModifyLoyaltyPoint`），靜默執行錯誤的批次類型。

### 問題四：API 可讀性差

Log 中看到 `BatchUploadType: 23`，需要去查 Enum 才知道是什麼類型，不利 debug 與監控。

---

## 四、改善方向

### 方向一：API 改成傳字串（根本解法）

**概念：** 在後端加上 `StringEnumConverter`，讓 API payload 傳遞字串而非 int。

```csharp
// BatchRequestEntity.cs
[JsonConverter(typeof(StringEnumConverter))]
public BatchUploadTypeDefEnum BatchUploadType { get; set; }
```

**前端改法：**
```typescript
// 不再需要 Enums.ts 的數字
// 可以直接用字串常數，或保留 TypeScript string enum
BatchUploadType: "BatchCreateBlackList"
```

**效益：**
- ✅ `Enums.ts` 的數字 sync 問題完全消失
- ✅ 新增 Enum 不再怕順序問題
- ✅ API Log 可讀性提升（`"BatchCreateBlackList"` vs `23`）
- ✅ 前端可直接用字串，不再依賴 TypeScript enum 的數字

**代價：**
- ❌ **Breaking change**：現有所有前端頁面的 `CreateBatchTask` 呼叫都需要改
- ❌ 需要完整的回歸測試
- ❌ 若有外部系統也在呼叫此 API，需一併通知

**適合時機：** 做大版本改版或新架構時一起處理。

---

### 方向二：Build Script 自動產生 Enums.ts（消除手動 sync）

**概念：** 建立一個 build script（PowerShell / T4 Template / Roslyn Source Generator），在每次 C# 編譯後自動讀取 enum 定義並產生 TypeScript 檔案。

```bash
C# BatchUploadTypeDefEnum（唯一真相）
       ↓ build script（自動）
TypeScript Enums.ts（自動產生，禁止手動修改）
```

**產生規則：** 讀取 C# enum 成員順序，自動計算 int 值後輸出 TypeScript：

```typescript
// AUTO-GENERATED - DO NOT EDIT
export enum BatchUploadTypeDefEnum {
    SalePageCreate = 0,
    BatchCreateBlackList = 23,
    // ...
}
```

**效益：**
- ✅ 消除 C# ↔ TypeScript 手動 sync 問題
- ✅ 不改 API 合約，現有邏輯不動
- ✅ 對前端透明，不需要改任何 TS controller

**代價：**
- ❌ C# enum 順序問題仍存在（中間插入仍會破壞）
- ❌ 需要建置流程改動（CI/CD pipeline）
- ❌ 若有人在沒跑 build script 的環境直接改 TypeScript，會被下次 build 覆蓋掉

**適合時機：** 不想改 API 合約，但想消除 sync 風險時。

---

### 方向三：C# Enum 明確寫死 int 值（最小成本的緩解）

**概念：** 在 C# enum 每個成員明確寫 int 值，新增時跳號（如每次 +10 或用較大的數字）。

```csharp
public enum BatchUploadTypeDefEnum
{
    SalePageCreate = 0,
    YMallSalePageCreate = 1,
    // ... 現有類型保持原值
    BatchCreateBlackList = 23,
    // 新增類型從 200 開始，避免與舊的衝突
    MyNewBatchType = 200,
}
```

**效益：**
- ✅ **防止中間插入導致位移**（即使誰插在中間，後面的 int 值不變）
- ✅ 成本最低，只改 C# enum，不改 API 合約
- ✅ 讓 Enum 的 int 值從「隱性依賴順序」變成「顯性合約」

**代價：**
- ❌ Enums.ts 仍需手動 sync（只是降低出錯機率）
- ❌ 是治標，三個地方各自維護的根本問題沒解決

**適合時機：** 最快速的風險降低手段，可以馬上做。

---

### 方向四：OpenAPI / Swagger 產生 TypeScript 型別（長期架構）

**概念：** 後端提供 OpenAPI spec，前端從 spec 自動產生 TypeScript 型別定義（包含 enum）。

```bash
後端 OpenAPI spec（自動產生）
       ↓ openapi-generator / swagger-codegen
TypeScript API client + type definitions（自動產生）
```

**效益：**
- ✅ API 合約是唯一真相，前端型別完全自動同步
- ✅ 長期架構最正確的做法

**代價：**
- ❌ 需要整體前後端架構調整，工程量大
- ❌ 現有 AngularJS + TypeScript module 架構需配合改動

**適合時機：** 做整體前後端架構翻新時。

---

## 五、建議優先順序

```bash
立即可做（降低風險）
  └─ 方向三：C# Enum 明確寫死 int 值
             → 防止中間插入的靜默 bug，一個 PR 搞定

中期規劃（消除 sync 風險）
  └─ 方向二：Build Script 自動產生 Enums.ts
             → 不改 API 合約，但 sync 問題消失

長期目標（根本解法，選其一）
  ├─ 方向一：API 改字串
  │          → 最徹底，但需要配合大版本改版
  └─ 方向四：OpenAPI 產生型別
             → 配合前後端架構翻新時一起做
```

---

## 六、新增批次類型時的注意事項（現況下）

在未改善前，新增批次類型時必須嚴格遵守：

1. **C# enum 只能加在最後面**，絕對不能插在中間
2. **Enums.ts 的 int 值必須與 C# enum 的實際索引一致**，加入後需人工驗證
3. **DB Definition Table 的 Definition_Code 必須與 enum member 名稱完全相同**（大小寫一致）
4. 建議在 PR review 時，請 reviewer 同時確認這三個地方的一致性

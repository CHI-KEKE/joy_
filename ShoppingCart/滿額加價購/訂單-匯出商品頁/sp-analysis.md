# SP 詳細分析：csp_BatchExportSalesOrderData 系列

## SP 清單

| SP 名稱 | 大小 | 用途 |
|---|---|---|
| `csp_BatchExportSalesOrderData.sql` | 72 KB | 舊版（v1） |
| `csp_BatchExportSalesOrderDataV2.sql` | 62 KB | 新版主 SP，整合子 SP 結果並輸出欄位 |
| `csp_BatchExportSalesOrderDataV2Sub.sql` | 88 KB | 新版子 SP，**核心查詢邏輯** |
| `csp_BatchExportSalesOrderDataV2SubAr.sql` | 88 KB | 新版子 SP（AR 版本，多國支援） |

路徑：`C:\91APP\DB\databases\ERPDB\Programmability\StoredProcedures\`

---

## 輸入參數（csp_BatchExportSalesOrderDataV2Sub）

```sql
@SalesOrderSlaveSupplierId  INT
@SalesOrderSlaveShopId      INT
@StartDateTime              DATETIME
@EndDateTime                DATETIME
@DateTimeType               VARCHAR(30)
@SalesOrderStatus           VARCHAR(30) = null
@ShippingOrderStatus        VARCHAR(30) = null
@PayProfileStatisticsTypeDef        VARCHAR(30) = null
@ShippingProfileStatisticsTypeDef   VARCHAR(30) = null
@ShippingProfileDistributorDef      VARCHAR(30) = null
@TemperatureTypeDef         VARCHAR(30) = null
@CancelOrderSlaveCauseDef   VARCHAR(30) = null
@ConfirmTypeDef             varchar(30) = null
@shippingTypeDef            SMALLINT = null
```

---

## 「是否為加價購品」欄位來源追蹤

### Step 1｜子 SP 查詢來源

```sql
-- csp_BatchExportSalesOrderDataV2Sub.sql 第 273 行
SalesOrderSlave_IsExtra AS SalesOrderData_SalesOrderSlaveIsExtra
```

- **資料表**：`dbo.SalesOrderSlave`
- **欄位型態**：`BIT`（0 = 否、1 = 是）
- **意義**：此商品是否為購物車加價購的加購品
- **無型態資訊**：只有「是/否」，無法得知是滿額還是滿件

### Step 2｜主 SP 輸出轉換

```sql
-- csp_BatchExportSalesOrderDataV2.sql 第 927 行
CASE WHEN SalesOrderData_SalesOrderSlaveIsExtra = 1 THEN @yes ELSE @no END
-- 輸出：「是」或「否」（多語系變數）
```

---

## ItemType 欄位說明

```sql
-- csp_BatchExportSalesOrderDataV2Sub.sql 第 307 行
SalesOrderSlave_ItemType AS SalesOrderData_SalesOrderSlaveItemType
```

ItemType 是複合 mode code 字串，包含以下旗標：

| Mode Code | Enum | 意義 |
|---|---|---|
| `"1"` | `Major` | 主商品 |
| `"2"` | `Sub` | 子商品 |
| `"A"` | `AddOnsSalepageExtraPurchase` | 商品加價購 |
| `"B"` | `SalepageBundle` | 組合商品 |
| `"C"` | `CartExtraPurchase` | 購物車加價購（**滿額/滿件共用**） |

> ⚠️ `"C"` 同時涵蓋滿額加價購與滿件加價購，**無法從 ItemType 區分兩者**。

---

## SalesOrderSlavePromotionEngine 表（關鍵）

```sql
-- 表結構：C:\91APP\DB\databases\ERPDB\Tables\SalesOrderSlavePromotionEngine.sql
CREATE TABLE [dbo].[SalesOrderSlavePromotionEngine] (
    [SalesOrderSlavePromotionEngine_Id]            BIGINT IDENTITY(1,1) NOT NULL,
    [SalesOrderSlavePromotionEngine_PromotionEngineId]  INT NOT NULL,
    [SalesOrderSlavePromotionEngine_SalesOrderSlaveId]  BIGINT NOT NULL,
    [SalesOrderSlavePromotionEngine_TypeDef]        VARCHAR(30) NOT NULL,  -- ← 活動折扣型態
    [SalesOrderSlavePromotionEngine_PromotionEngineName] NVARCHAR(50) NOT NULL,
    [SalesOrderSlavePromotionEngine_ValidFlag]      BIT NOT NULL,
    ...
)
```

### TypeDef 值對應

| TypeDef 值 | 中文 |
|---|---|
| `CartReachPriceExtraPurchase` | 滿額加價購 |
| `CartReachPieceExtraPurchase` | 滿件加價購 |

### 目前 SP 使用方式

SP 目前只從 `SalesOrderSlavePromotionEngine` 取出 PromotionId 和 PromotionName，**TypeDef 未被輸出**：

```sql
-- 第 1273~1290 行：只取 Id 和 Name，未取 TypeDef
ISNULL(STUFF((SELECT ';' + CAST(SalesOrderSlavePromotionEngine_PromotionEngineId AS NVARCHAR)
    FROM dbo.SalesOrderSlavePromotionEngine WITH (NOLOCK)
    WHERE SalesOrderSlavePromotionEngine_SalesOrderSlaveId = SalesOrderData_SalesOrderSlaveId) ...
), '') AS SalesOrderData_PromotionIds,

ISNULL(STUFF((SELECT ';' + SalesOrderSlavePromotionEngine_PromotionEngineName
    FROM dbo.SalesOrderSlavePromotionEngine WITH (NOLOCK)
    WHERE SalesOrderSlavePromotionEngine_SalesOrderSlaveId = SalesOrderData_SalesOrderSlaveId) ...
), '') AS SalesOrderData_PromotionNames
```

---

## SP 現有輸出與缺口總結

| 欄位 | 輸出值 | 能否區分滿額/滿件 |
|---|---|---|
| `SalesOrderData_SalesOrderSlaveIsExtra` | BIT 0/1 | ❌ 只知道「是加價購」 |
| `SalesOrderData_SalesOrderSlaveItemType` | 含 "C" | ❌ 滿額/滿件共用 "C" |
| `SalesOrderData_PromotionIds` | ID 清單 | ❌ 有 ID 但無 TypeDef |
| `SalesOrderData_PromotionNames` | 名稱清單 | ❌ 名稱不可靠 |
| `SalesOrderSlavePromotionEngine_TypeDef` | 未輸出 | ✅ **有答案但未帶出** |

---

## 若要改 SP 的做法（供參考，目前選擇不改）

在子 SP 的最終 SELECT 中新增：

```sql
ISNULL((
    SELECT TOP 1 SalesOrderSlavePromotionEngine_TypeDef
    FROM dbo.SalesOrderSlavePromotionEngine WITH (NOLOCK)
    WHERE SalesOrderSlavePromotionEngine_SalesOrderSlaveId = SalesOrderData_SalesOrderSlaveId
      AND SalesOrderSlavePromotionEngine_TypeDef IN (
          'CartReachPriceExtraPurchase', 'CartReachPieceExtraPurchase'
      )
      AND SalesOrderSlavePromotionEngine_ValidFlag = 1
), '') AS SalesOrderData_CartExtraPurchaseTypeDef
```

需同步修改：
- `csp_BatchExportSalesOrderDataV2Sub.sql`
- `csp_BatchExportSalesOrderDataV2SubAr.sql`

# BatchUploadAverageTime 平均時間統計表

**DB：** `WebStoreDB`

## 用途

記錄各批次類型的**平均每筆執行秒數**，用於 SMS 前台顯示「預估完成時間」。

批次建立時，系統從此表查出對應 TypeDef 的 `AverageSecondsPerRecord`，再乘以總筆數，計算出 `BatchUpload_EstimateFinishMinutes` 寫回主表。

---

## 欄位說明

| 欄位 | 型別 | Nullable | 說明 |
|------|------|----------|------|
| `BatchUploadAverageTime_Id` | long | ❌ | PK |
| `BatchUploadAverageTime_TypeDef` | string(50) | ❌ | 批次類型（對應 BatchUploadTypeDefEnum 字串名稱） |
| `BatchUploadAverageTime_AverageSecondsPerRecord` | int | ✅ | 平均每筆執行秒數 |
| `BatchUploadAverageTime_CreatedDateTime` | DateTime | ❌ | 建立時間 |
| `BatchUploadAverageTime_CreatedUser` | string(50) | ✅ | 建立者 |
| `BatchUploadAverageTime_UpdatedTimes` | byte | ❌ | 更新次數 |
| `BatchUploadAverageTime_UpdatedDateTime` | DateTime | ❌ | 最後更新時間 |
| `BatchUploadAverageTime_UpdatedUser` | string(50) | ✅ | 最後更新者 |
| `BatchUploadAverageTime_ValidFlag` | bool | ❌ | 資料有效旗標 |
| `BatchUploadAverageTime_Rowversion` | byte[] | ❌ | 樂觀鎖版本號 |

---

## 常用查詢

```sql
USE WebStoreDB

-- 查詢所有批次類型的平均時間設定
SELECT
    BatchUploadAverageTime_TypeDef,
    BatchUploadAverageTime_AverageSecondsPerRecord
FROM BatchUploadAverageTime (NOLOCK)
WHERE BatchUploadAverageTime_ValidFlag = 1
ORDER BY BatchUploadAverageTime_TypeDef

-- 查詢特定批次類型的設定
SELECT *
FROM BatchUploadAverageTime (NOLOCK)
WHERE BatchUploadAverageTime_ValidFlag = 1
  AND BatchUploadAverageTime_TypeDef = '{TypeDef}'

-- 與實際執行時間對比（驗證預估是否準確）
SELECT
    b.BatchUpload_TypeDef,
    AVG(DATEDIFF(SECOND, b.BatchUpload_StartDateTime, b.BatchUpload_FinishDateTime)) AS ActualAvgSeconds,
    t.BatchUploadAverageTime_AverageSecondsPerRecord AS ConfiguredAvgSeconds
FROM BatchUpload b (NOLOCK)
LEFT JOIN BatchUploadAverageTime t (NOLOCK)
    ON b.BatchUpload_TypeDef = t.BatchUploadAverageTime_TypeDef
    AND t.BatchUploadAverageTime_ValidFlag = 1
WHERE b.BatchUpload_ValidFlag = 1
  AND b.BatchUpload_StatusDef IN ('Finish', 'FinishWithError')
  AND b.BatchUpload_StartDateTime IS NOT NULL
  AND b.BatchUpload_FinishDateTime IS NOT NULL
GROUP BY b.BatchUpload_TypeDef, t.BatchUploadAverageTime_AverageSecondsPerRecord
ORDER BY b.BatchUpload_TypeDef
```

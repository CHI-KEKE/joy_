# 各環境 Filer 路徑

## BatchUpload 暫存路徑

| 環境 | 路徑 |
|------|------|
| TW QA | `\\TYO-QA-Filer1\Files\Tmp\BatchUpload` |
| HK QA | `\\SG-HK-QA1-SCM2\Storage\Tmp\BatchUpload` / `E:\Storage\Tmp\BatchUpload\{ShopId}` |
| HK Prod | `\\SG-HK-Filer1\Storage\Tmp\BatchUpload` |

<br>

---

## 範例範本檔路徑（Docs 目錄）

| 類型 | URL | 實際路徑 |
|------|-----|---------|
| ModifyRewardPromotionSalePage | `/Docs/ModifyRewardPromotionSalePage/Batch update products in promotions template.xlsx` | `E:\Storage\Docs\ModifyRewardPromotionSalePage\` |
| BatchModifyPromotionOuterId | `/Docs/BatchModifyPromotionOuterId/Batch update products in promotions template (Product part number).xlsx` | `E:\Storage\Docs\BatchModifyPromotionOuterId\` |

**URL 注意事項**：前端路徑需加上前導 `/` 才能抓到正確路徑。

<br>

---

## BatchCrmOrderImport SFTP 路徑

| 環境 | 路徑 |
|------|------|
| SFTP 暫存（Tmp） | `sftp://{server}/workspace/OMO/BatchCrmOrderImport/Tmp/` |
| SFTP 正式處理 | `sftp://{server}/workspace/OMO/BatchCrmOrderImport/{ShopId}/` |

<br>

---

## 執行記錄路徑

Config Key：`BatchUpload.Storage.Floder`（預設：`C:\Temp\UploadData`）

```bash
C:\Temp\UploadData\ExecutedTasks\{yyyyMMdd}\{taskId}\{supplierId}\{BatchUpload_Code}
```

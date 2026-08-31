# BatchModifyPromotionOuterId — 批次異動活動料號

## 基本資訊

| 項目 | 值 |
|------|-----|
| 批次類型（TypeDef） | `BatchModifyPromotionOuterId` |
| Service | `BatchModifyPromotionOuterIdService` |
| NMQ JobId（子 Task） | 486 |
| 範例檔位置 | `E:\Storage\Docs\BatchModifyPromotionOuterId` |

<br>

---

## 批次類型識別

```csharp
case BatchUploadTypeDefEnum.BatchModifyPromotionOuterId
```

Service 繼承自 `AbstractCreateBatchTaskBaseService`

```csharp
batchService.ProcessCreateBatchTask(entity);
```

<br>

---

## 範例檔

```bash
https://sms.qa1.hk.91dev.tw/CommerceCloud/Docs/BatchModifyPromotionOuterId/Batch update products in promotions template (Product part number).xlsx
```

實際路徑：

```bash
E:\Storage\Docs\BatchModifyPromotionOuterId\Batch update products in promotions template (Product part number).xlsx
```

<br>

---

## 主 NMQ Task Data

```json
{
    "BatchUploadId": 11519,
    "SupplierId": 2,
    "UploadType": "BatchModifyPromotionOuterId",
    "UploadUser": "ellygong@nine-yi.com",
    "FilePassword": "",
    "ExportDataCondition": null,
    "Country": "HK"
}
```

<br>

---

## Excel 欄位 Mapping

（與 `ModifyRewardPromotionSalePage` 共用 `BatchModifyPromotionSalePageMappingProfile`）

| 語系 | ScheduleTime | PromotionId | SalePageIds | ModifyStatus |
|------|-------------|-------------|-------------|--------------|
| TW | 期望生效時間 | 活動序號 | 商品頁序號 | 異動狀態 |
| Others | Execution Time | Promotion ID | Product Page ID | Update |

<br>

---

## DoValidate（二次驗證）

使用 `BatchModifyPromotionOuterIdDataEntityValidator`（FluentValidation）

- 驗重複料號
- 驗證活動序號
- 驗證料號字元
- 驗證異動狀態

<br>

---

## SCMAPIV2

```bash
POST /v2/Promotion/ModifyPromotionProductSkuOuterIds
```

位置：`NineYi.SCM.Frontend.NMQV2.BatchUpload.BatchModifyPromotionSalePageTaskProcess`

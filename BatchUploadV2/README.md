# BatchUpload 批次上傳系統

## 目錄結構

| 路徑 | 說明 |
|------|------|
| [核心架構](./核心架構.md) | 整體流程、CreateBatchTask 入口、NMQ 主流程 |
| [schema/](./schema/) | DB 資料模型、狀態機、系統機制 |
| [types/](./types/) | 各批次類型說明 |
| [cases/](./cases/) | 實際案例情境（前因後果） |
| [guides/](./guides/) | 操作步驟 SOP |
| [reference/](./reference/) | 技術查詢參考（SQL、路徑、套件） |

## Schema

| 檔案 | 說明 |
|------|------|
| [table](./schema/table.md) | BatchUpload / BatchUploadData / BatchUploadMessage Table 欄位 |
| [狀態](./schema/狀態.md) | 狀態機與狀態流轉說明 |
| [enum](./schema/enum.md) | Enum / TypeDef 欄位對照與各批次類型對應值 |
| [商店比數上限](./schema/商店比數上限.md) | Quota 機制、SuccessCount、上限筆數、重複執行防護 |

<br>

## 各批次類型

| 檔案 | 批次類型 | 說明 |
|------|----------|------|
| [ModifyRewardPromotionSalePage](./types/ModifyRewardPromotionSalePage.md) | `ModifyRewardPromotionSalePage` | 批次更新活動商品（完整流程） |
| [BatchModifyPromotionOuterId](./types/BatchModifyPromotionOuterId.md) | `BatchModifyPromotionOuterId` | 批次異動活動料號 |
| [ExportRewardPromotionSalePage](./types/ExportRewardPromotionSalePage.md) | `ExportRewardPromotionSalePage` | 匯出活動商品 |
| [ExportRewardPromotionOuterId](./types/ExportRewardPromotionOuterId.md) | `ExportRewardPromotionOuterId` | 匯出活動料號 |
| [BatchCrmOrderImport](./types/BatchCrmOrderImport.md) | `BatchCrmOrderImport` | 其他交易資料匯入（SFTP + ETL） |

<br>

## 案例情境

| 檔案 | 說明 |
|------|------|
| [quota-exceeded](./cases/quota-exceeded.md) | 廠商批次作業超過 Job 上限，卡在待執行 |
| [deadlock-api-error](./cases/deadlock-api-error.md) | SQL Deadlock 導致大量批次中途中止 |
| [api-null-response](./cases/api-null-response.md) | API 回傳 null 導致錯誤明細顯示異常 |
| [zh-hk-format-exception-timeout](./cases/zh-hk-format-exception-timeout.md)（[精美版](./cases/zh-hk-format-exception-timeout.html)） | zh-HK 語系錯誤訊息缺參數導致 FormatException，批次卡在 InProcess 逾時告警 |

<br>

## 操作指南

| 檔案 | 說明 |
|------|------|
| [add-new-type](./guides/add-new-type.md) | 新增一個批次類型的完整 SOP |

<br>

## 技術參考

| 檔案 | 說明 |
|------|------|
| [api](./reference/api.md) | 下載錯誤明細 API（GetMessageList） |
| [query](./reference/query.md) | NMQ Jobs 清單 + 常用 SQL 查詢 |
| [filer-paths](./reference/filer-paths.md) | 各環境 Filer / 暫存路徑 |
| [load-excel](./reference/load-excel.md) | LinqToExcel 套件讀取 Excel 技術細節 |

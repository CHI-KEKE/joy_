# ExportRewardPromotionOuterId — 匯出活動料號

## 基本資訊

| 項目 | 值 |
|------|-----|
| 批次類型（TypeDef） | `ExportRewardPromotionOuterId` |
| `BatchUploadType` Enum 值 | 174 |
| NMQ JobId | 487 |
| NMQ Job Name | `ExportRewardPromotionOuterIdTask` |
| Service | `ExportRewardPromotionOuterIdService.cs` |

<br>

---

## PromotionAPI

- 使用 endpoint：`productsku-outerid-list`

<br>

---

## 子 Task Data 範例

```json
{
    "BatchUploadId": 11878,
    "ProcessType": "ByProcessCount",
    "BatchUploadDataId": 0,
    "ProcessCount": 10,
    "UploadUser": "allenlin@nine-yi.com"
}
```

NMQ Task 建立：

```bash
Create Task, Data: {...}, Job: ExportRewardPromotionOuterIdTask
```

<br>

---

## 子 Process Loop 結束 Log

最後一輪（撈不到 Ready 資料，結束 loop）：

```bash
匯出給點活動適用商品(料號) xlsx 檔案成功
刪除檔案：\\SG-HK-QA1-SCM2\Storage\Tmp\ExportRewardPromotionOuterId\BA2505201400001\BA2505201400001_6780__20250520140940.xlsx
匯出給點活動適用商品(料號)總筆數：4
匯出給點活動適用商品(料號)檔案名稱：BA2505201400001_6780__20250520140940.zip
匯出給點活動適用商品(料號)下載路徑：\\SG-HK-QA1-SCM2\Storage\Tmp\ExportRewardPromotionOuterId\BA2505201400001\BA2505201400001_6780__20250520140940.zip
更新作業執行狀態, status:Finish
新增錯誤清單明細, 筆數:0
尚有資料未處理, 發動 Task 繼續執行批次處理
taskData :{"BatchUploadId":11878,"ProcessType":"ByProcessCount","BatchUploadDataId":0,"ProcessCount":10,"UploadUser":"allenlin@nine-yi.com"}
Create Task, Data: {...}, Job: ExportRewardPromotionOuterIdTask
End BatchUploadTaskProcess
::Finished
```

最後一輪的計數結果：

```bash
上傳失敗筆數:0
上傳成功筆數:1
```

> 主 NMQ 發現沒有 Ready 的 BatchUploadData 可撈時，不再觸發子 Task，流程結束。

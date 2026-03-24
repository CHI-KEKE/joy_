
## 排程

SQL Job : NineYiDW_TransDataFromNMQV2DB

## 這支 SP 的「目標」是什麼

在指定區間 [startDate, endDate)

1. 從 NMQV2DB 撈出簡訊任務資料（#tmpShortMessageData）
2. 從 ERPDB 撈出簡訊歷程（#tmpSmsMessageHistory，只取 Finish + Valid）
3. 依開關 ShortMessage.AreaType.UseSalesMarket.Enable
4. 開：優先用「銷售市場 CountryCode vs 簡訊 CountryProfile_Code」判定 Domestic/Oversea
   關：用「CountryProfileId vs DefaultCountryProfileId」判定 Domestic/Oversea
5. 把結果寫進 NineYiDW.dbo.ShortMessageDM
6. 同時先刪掉該日期區間內舊資料再重灌（ETL 重跑模式）


## 參數與日期區間：你預期的「一整天」是否成立？

如果 @endDate is null → @endDate = dateadd(day, 1, @startDate)

所以預設會跑：[@startDate 00:00:00, @startDate+1 00:00:00)

✅ 符合「跑某一天」的常見預期

#### 風險點

@endDate 參數型別是 date，你傳入如果是某天（例如 2025-12-30），它會被視為 2025-12-30 00:00:00

你在 ERP 查詢那邊參數宣告是 @endDate datetime，但你傳的是 date，SQL Server 會自動轉型成 datetime 的 00:00:00

✅ 邏輯仍然正確（半開區間 < @endDate），但要確定你預期的 endDate 是「隔天」而不是「同天」

## 先刪再插：會不會刪錯 / 重複 / 漏資料？

```sql
DELETE ShortMessageDM
WHERE TaskCreatedDateTime >= @startDate
  AND TaskCreatedDateTime <  @endDate
```

✅ 如果 ShortMessageDM_TaskCreatedDateTime 是你 ETL 的時間維度，且你每次跑都用相同日期條件，這是標準重跑模式


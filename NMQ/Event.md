
## Order

下單：OrderPlaced
付款後：OrderCreated

<br>
<br>

## 綁定問題

發現特定的 Job 應該要綁定特定 Event 並執行：`SaveTradesOrderExtendInfoToS3Job` 如預期沒有執行
QA 環境還沒有把此 Job 綁定到 `OrderPlaced` 上，而是還留在 `OrderCreated`

要用信用卡結帳，不能用 2C2P 否則不會成功執行轉單等動作也就是沒有 `OrderCreated` 發生

<br>
<br>

## Event --> Job Task Data

- `sourceId` 就是 `EventId`
- `Id` 就是 `TaskId`

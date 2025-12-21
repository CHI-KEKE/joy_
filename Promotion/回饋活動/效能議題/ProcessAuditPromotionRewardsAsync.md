


## ProcessAuditPromotionRewardsAsync Create Task 開始跳 CPU High

https://91app.slack.com/archives/C08BE4B4KQW/p1761895776826399


#### 分析方法

- 比對 log 與 CPU 開始陡升的時間
- 針對程式優化效能

<br>

#### 原因

當資料量很大時，CPU 會飆高其中原因包含

- 每次迴圈都同步呼叫兩次 request.ToJson() 和 INMQv3Client.Task.CreateAsync → 導致所有 JSON 序列化與 HTTP 請求都在單一執行緒中執行

- 雖然建立了 tasks 清單，但沒有任何併發控制，造成 → CPU 過度密集 + Thread 被阻塞。


#### 優化

1️⃣ 避免重複序列化，在每次迴圈內，先把 request.ToJson() 的結果存起來

```CSHARP
var json = request.ToJson();
```

因為是「逐筆同步 await」，整個流程會不停切換 context（await → resume → await → resume），若多筆請求同時執行，可以減少「閒等時間」，I/O-bound 的 CreateAsync 現在能同時進行，CPU 不會在等待網路回應時閒置或反覆切換執行緒


作法是可以把每個 CreateAsync 呼叫加入 `List<Task>` 後，改用 `Task.WhenAll(tasks)` 進行併發執行，並搭配 `SemaphoreSlim` 控制同時併發數（例如 8～16 個），在提升效率的同時，也避免對外部服務造成壓力。
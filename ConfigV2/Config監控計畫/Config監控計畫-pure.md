# Config 監控計畫：跨市場設定值一致性檢查

## 01 情境背景與目的

公司內部共用的設定檔專案 `nineyi.configuration` 裡，每個服務底下都有分市場（TW、HK、MY 等）各自獨立的設定檔。實務上常發生的情況是：某個工程師在 TW 的設定檔加了一個新的 Key（例如某個功能開關），但忘了同步加到 HK 或 MY 的對應檔案，導致其他市場上線後功能行為不一致，甚至直接壞掉。這份文件記錄目前討論出來的監控計畫做法，目的是希望有一個能自動、即時發現「某個市場漏加設定值」的機制。

目標倉庫：`nineyi.configuration`
監控市場：TW / HK / MY

**問題本質**：同一個服務下，不同市場的設定檔（例如 `AppSettings.Prod.TW.config` 與 `AppSettings.Prod.HK.config`）理論上結構應該一致，實際上會因為人為疏忽而出現 Key 缺漏或不一致。

**部署前提**：監控機制打算做成一支獨立的 Worker，跑在遠端機器上，定期或即時運作，不依賴任何人手動觸發檢查。

**關鍵限制**：這個 Worker 不會有本機的 `nineyi.configuration` clone，也不會知道什麼時候會有人 push 新的設定值，因此設計上必須直接透過 Bitbucket API 取得倉庫內容，而不是仰賴本機 git 操作。

名詞對照：文件中提到的「市場」對應到設定檔命名裡的市場代碼，例如 `AppSettings.Prod.TW.config`（台灣）、`AppSettings.Prod.HK.config`（香港）、`AppSettings.Prod.MY.config`（馬來西亞），部分服務也可能有 `OR`、`PX` 等其他市場代碼。

## 02 現況能力與限制

目前 `nineyi.ai.code.review.v2` 專案裡的 `BitbucketClient` 已經有兩支可以重用的 API 呼叫邏輯，分別是列出目錄內容的 `GetRepoSrcTreeAsync`，以及讀取單一檔案內容的 `GetFileRawContentAsync`。這兩支 API 原本是給外部的 Linter 工具透過 `BitbucketController` 呼叫使用的，並不是這次要設計的監控 Worker 的一部分，但它們的呼叫方式與踩過的坑可以直接參考。

| 限制項目 | 具體內容 | 對監控計畫的影響 |
|---|---|---|
| 目錄深度 | 目前程式碼呼叫時固定帶 `max_depth=1`，只能列出單層目錄，不支援遞迴列出所有子目錄 | 要掃描全部服務資料夾，需要對每個服務目錄各打一次 API，無法一次列出整個倉庫的所有檔案 |
| 分頁 | 目前的 `GetRawJsonAsync` 沒有處理分頁，只會取回第一頁結果 | 若某個服務底下的檔案數超過單頁上限，會有資料遺漏風險，正式設計時需要補上分頁處理 |
| 速率限制 | 已經實際觀察到 `nineyi.configuration` 這個共用倉庫在短時間內被大量呼叫時，會收到 Bitbucket 回傳的 `429 Too Many Requests` | 若監控 Worker 用高頻率輪詢的方式對整個倉庫掃描，會提高 429 發生的機率，需要在架構設計時特別避開 |
| URL 尾端斜線 | Bitbucket 的 `/src/{revision}` 端點在路徑為空時，如果沒有補上結尾斜線會被誤判成檔案下載而非目錄列表，回傳 404 | 提醒之後若重用類似邏輯組 URL，一律要留意結尾斜線的處理，避免重蹈覆轍 |

結論：如果監控 Worker 採用「定期打 API 全量掃描整個倉庫」的方式，會同時遇到「呼叫次數多、容易 429」與「需要一個個資料夾手動列舉」兩個問題，設計上需要盡量降低不必要的呼叫次數。

## 03 輪詢與事件觸發比較

監控 Worker 要「怎麼知道該去檢查」有兩種基本做法，差異在於是「Worker 自己定時去問」，還是「有變動時對方主動通知 Worker」。

| | 定期輪詢 API | Bitbucket Webhook 事件觸發 |
|---|---|---|
| 觸發時機 | 固定排程，例如每小時跑一次 | 有人 push 設定值變更的當下就立即觸發 |
| API 呼叫量 | 每次排程都要打，不管有沒有變動 | 只有真的有 push 才會呼叫，次數大幅減少 |
| 速率限制風險 | 較高，固定頻率打同一批倉庫容易累積成高頻呼叫 | 較低，只在真正有異動時才呼叫 |
| 即時性 | 有延遲，取決於排程間隔 | 幾乎即時 |
| 實作複雜度 | 較簡單，只需要排程加上 API 呼叫 | 需要有一個對外可達的服務端點，讓 Bitbucket 能夠打進來 |

建議：採用「Webhook 事件觸發為主、定期輪詢為輔」的組合。這個做法跟現有 `nineyi.ai.code.review.v2` 專案本身處理 `pullrequest:created` 之類 Webhook 事件的邏輯性質相同，只是這次改為訂閱 `repo:push` 事件。

## 04 推薦架構：Webhook 觸發加精準比對

核心概念是讓 Bitbucket 在有人 push 到 `nineyi.configuration` 倉庫時，主動通知監控 Worker，Worker 再針對「這次真正被改動的檔案」去做比對，而不是每次都掃描整個倉庫。

流程：有人 push 設定值 → Bitbucket 觸發 Webhook → Worker 取得異動檔案清單 → 抓取對照市場內容比對 → 發現差異就發通知

**步驟一：在 `nineyi.configuration` 倉庫上註冊 Bitbucket Webhook**
指向監控 Worker 對外的服務端點，訂閱 `repo:push` 事件。這跟現有 AI Code Review 服務接收 `pullrequest:created / updated` 事件是同一套 Bitbucket Webhook 機制，只是改成監聽 push 而非 PR。

**步驟二：用 push 事件帶來的 commit hash 查出這次實際改了哪些檔案**
呼叫 Bitbucket 的 diffstat 端點取得本次 push 前後的差異清單：
```
GET /2.0/repositories/{workspace}/{repo}/diffstat/{new}..{old}
```
回傳結果會列出這次 push 真正異動的檔案路徑，例如 `ApplicationConfig/NineYi.Scm.NMQV2/AppSettings.Prod.TW.config`。

**步驟三：過濾出真正相關的異動**
只挑出符合 `AppSettings.Prod.{市場}.config` 這個命名規則的檔案，忽略其他無關異動。

**步驟四：只針對這個服務資料夾，補抓其他市場的對照檔案內容**
不需要掃描全部服務，只需要針對「這次被改動的服務」，額外呼叫幾次讀檔 API 取得其他市場現有內容，例如異動的是 TW，就再抓一次 HK 與 MY：
```
GET /2.0/repositories/nineyi/nineyi.configuration/src/{new_commit}/ApplicationConfig/NineYi.Scm.NMQV2/AppSettings.Prod.HK.config
GET /2.0/repositories/nineyi/nineyi.configuration/src/{new_commit}/ApplicationConfig/NineYi.Scm.NMQV2/AppSettings.Prod.MY.config
```

**步驟五：解析 XML，比對各市場的 Key 集合差異**
設定檔都是標準 .NET 格式（`<appSettings><add key="..." value="..."/></appSettings>`），把每個市場的 Key 取出來做集合比對，若某個市場缺少其他市場都有的 Key，就視為一筆待通知的差異。

這樣設計的好處：每次 push 只需要個位數的 API 呼叫（一次 diffstat 加上兩三次讀檔），完全不會累積成高頻呼叫、避開 429 風險；也完全不需要事先知道誰、什麼時候會 push，因為 Webhook 本來就是為了解決「不知道對方何時會有動作」而存在的機制；而且是準即時反應，設定值改完幾秒內就能發現遺漏。

## 05 保底機制：定期全量掃描

Webhook 有一個已知的風險，就是「Bitbucket 端送出通知失敗」或者「Worker 剛好在重啟、沒有成功接收」時，這次異動就會被漏掉，完全沒有補償機制。這跟先前調查 GitLab MR #240 卡住事件時得到的教訓是同一個道理：只靠一次性的觸發、沒有任何補償或重試設計，一旦中途出狀況就會整個流程默默失敗且沒有人發現。

因此建議：Webhook 作為主要觸發機制，額外保留一個排程機制作為安全網，避免真的漏接時完全沒有補救。

**主要機制**：Webhook 即時觸發，針對單次 push 的異動檔案做精準比對，反應快、呼叫量低。

**保底機制**：每天或每週跑一次全量掃描，用列目錄 API 列出所有服務資料夾，抓齊 TW/HK/MY 全部內容做一次完整比對，確保就算漏接 Webhook，最多隔一天也能被抓到。

保底機制因為呼叫頻率低（一天一次而非每次 push），即使需要對每個服務資料夾各打一輪 API，總呼叫量仍在可接受範圍內，不需要跟 Webhook 觸發時一樣講求精簡。

## 06 異動作者標註設計

除了發現「哪個市場漏加了設定值」之外，還希望能標出「是誰加的」，方便通知的人知道要找誰確認、原始目的是什麼。這個資訊其實不需要額外查詢，Webhook 觸發時本身就會帶著這項資料。

**方式一：直接從 push Webhook payload 取得（建議做法）**
Bitbucket 的 `repo:push` Webhook payload 裡的 `push.changes[].commits[]`，每個 commit 都包含作者資訊：
```json
{
  "hash": "abc1234...",
  "author": {
    "raw": "Allen Lin <allen.lin@91app.com>",
    "user": { "display_name": "Allen Lin", "nickname": "allen.lin" }
  },
  "message": "新增 TW 設定值 XXX",
  "date": "2026-08-18T05:30:00+00:00"
}
```
也就是說，觸發 Webhook 的當下就已經知道是誰 push 的，完全不需要額外呼叫任何 API。

**方式二：查詢特定檔案的 commit 歷史（更精準但較少用到）**
如果一次 push 包含多個 commit、多個檔案異動，想確認具體是哪一次 commit 加了某個特定 Key，可以查詢該檔案的 commit 歷史來輔助判斷，但日常使用建議以方式一為主，已經足夠涵蓋多數情境。

通知範例格式：
```
⚠️ 設定值不一致提醒
專案：NineYi.Scm.NMQV2
異動者：Allen Lin (allen.lin@91app.com)
異動時間：2026-08-18 05:30
新增於：AppSettings.Prod.TW.config
缺少於：AppSettings.Prod.HK.config, AppSettings.Prod.MY.config
Key：SomeFeatureFlag = true
```

保底機制（每日全量掃描）因為不是由某一次 push 觸發，沒有對應的單一 commit 作者可標註，這種情況下可以改標「最後一次修改該檔案的作者」，作為次要資訊參考。

## 07 基礎設施與待釐清事項

這個計畫目前是規劃階段，實際落地前還有幾件事需要確認：

- 監控 Worker 需要一個對外可達的 HTTPS 端點，讓 Bitbucket 能夠把 Webhook 事件送進來，需要走公司對外的 Ingress 或負載平衡器設定，跟現有 AI Code Review 服務的 Webhook 入口是類似的需求。
- 監控 Worker 需要 Bitbucket 的存取憑證，例如 App Password 或類似機制，可以參考 `BitbucketClient` 現有使用的 Basic Auth 驗證方式。
- 需要決定監控 Worker 要獨立建置成一個新的服務／腳本，還是併入現有 `nineyi.ai.code.review.v2` 專案內共用同一套 `BitbucketClient` 邏輯。
- 需要決定發現差異之後要用什麼管道通知，例如 Slack、Teams、Email 或建立追蹤單。
- 需要決定保底掃描的排程機制要用什麼工具，例如 Kubernetes CronJob、Jenkins 排程任務，或其他排程服務。
- 目前重用的 `GetRepoSrcTreeAsync`／`GetFileRawContentAsync` 邏輯沒有處理分頁，若正式導入保底全量掃描，需要補上分頁處理避免資料遺漏。

## 08 後續行動

這份文件目前是釐清與規劃階段的紀錄，尚未進入實作。下一步建議依序確認以下事項，再決定是否正式排入開發工作：

確認 Webhook 註冊方式 → 確認技術棧與部署位置 → 確認通知管道 → 排入開發時程

本次釐清結論：確認採用「Bitbucket Webhook 事件觸發為主、定期全量掃描為輔」的架構最能滿足「遠端 Worker、無法本機 clone、不知道誰何時會 push」這個限制條件，且能同時解決「標出異動作者」的附加需求，不需要額外的 API 呼叫即可從 Webhook payload 取得作者資訊。

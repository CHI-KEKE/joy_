

### 案例 2：SyncFromServiceBaseUrl 配置問題

**問題描述**：有些 controller 會掛掉讀到 v2 config 但打不通，因為 domain 被搬走了

https://91app.slack.com/archives/G04TVB3KW/p1746421134276869

<br>

**相關連結**：
- QA：https://gitlab.91app.com/translation/NineYi.Translation/-/merge_requests/270/diffs
- PP：https://gitlab.91app.com/translation/NineYi.Translation/-/merge_requests/269

<br>

**Exception 查詢**：[監控面板連結](https://monitoring-dashboard.91app.io/explore?schemaVersion=1&panes=%7B%22ih6%22:%7B%22datasource%22:%22ZIOlfD44k%22,%22queries%22:%5B%7B%22datasource%22:%7B%22type%22:%22loki%22,%22uid%22:%22ZIOlfD44k%22%7D,%22editorMode%22:%22code%22,%22expr%22:%22%7Bservice%3D%5C%22pp-translation%5C%22%7D%20%7C%3D%60Exception%60%22,%22maxLines%22:100,%22queryType%22:%22range%22,%22refId%22:%22A%22%7D%5D,%22range%22:%7B%22from%22:%22now-3h%22,%22to%22:%22now%22%7D%7D%7D&orgId=2)
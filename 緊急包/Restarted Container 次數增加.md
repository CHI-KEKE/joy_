https://91app.slack.com/archives/C3DB30C3T/p1770693407190259



11:10 有 restart 一次 container，後面就沒有再重啟
會再確認一下 alert 規則


## 看 CPU / Memory 現狀

alram 之家

https://monitoring-dashboard.91app.io/d/den3uypx2c64gb/logistics-center-alert?orgId=2&from=now-1h&to=now&timezone=Asia%2FTaipei&var-MarketENV=TW-Prod&var-Loki=ZIOlfD44k&var-Cluster=hxdP8t7Vz&var-Namespace=prod-logistics-center&var-Sandbox_Namespace=sandbox-api-gateway&var-CacheClusterID=backend-redis-1-001&var-CloudWatch=kYZD-B7Vk&var-LOG_CONTAIN_STRING=&var-topk_1_node=ip-10-2-231-23.ap-northeast-1.compute.internal&var-Quey_Taints=sg&var-Service_Catalog=appgen



##　看影響的 NMQ Job 狀況

ce9677f5-566b-4959-85d5-860d4e1f9781
4c2f5da7-2deb-490c-b4d3-c65374a2a62b
6415c8af-8a02-498a-b069-3a271a617a8e
61ce0828-9bca-48de-b502-2b088a5c10ea
f8f1ea27-26dc-43ea-b8f4-5e88c2d1a93a
UpdateOrderStatusAfterDeliveryStartedJob
皆顯示 log => Redo => Finish


According to the response from APIGetTaskInfoAsync, the task state is not 'ready' or 'pause'


## alert rule 調整

Time Range 先改為 10 min


## k8s log


Last state: Terminated with 137: OOMKilled, started: 2026-02-03 09:46:17, finished: 2026-02-10 11:07:19




## 如果看到 Dashboard 的 Router 是帶 NMQ 開頭的就只要部署 NMQV3 即可

順序 : Config -> App

## 確認 CI Master 建構狀態

確認該 NMQ CI Master 已經 Build 完成，並成功上傳至 Artifact

CI Master 網址：http://ci-master.91dev.tw:8080/

<br>

## 關閉 NMQV3 Dashboard Router

NMQV3 Dashboard 網址：https://nmqv3-dashboard.qa1.hk.91dev.tw/router-management/

<br>

## 執行部署程序

執行 CI MY 部署程式至 NMQV3

部署網址：https://ci.my.91app.io/view/Deploy%20Global%20QA/job/NineYi.Global.NMQV3%20-%20QA/

<br>

## 系統重啟

關機後重啟系統以確保新版本正常載入

<br>

## 版號確認位置

**Worker Configs**

`D:\nmq\artifacts\worker-configs` 看 config

**Worker Modules Jobs**

`D:\nmq\artifacts\worker-modules\jobs\Prod\NineYi\SCM.NMQV2`

**Library**

`D:\Prod\NineYi\NMQV2\Library\NineYi.Scm.Frontend`



## deploy 機器本身狀態


有一個排程跑 ps
有一個資料夾會被清空重新放新的


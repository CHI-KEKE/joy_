### 2.3 錯誤情境

#### 情境一：訂單狀態時序問題

**錯誤訊息：** [Error] TS250813R000004 訂單狀態Finish 不符合預期退點狀態

<br>

**原因：** 雖然是 OrderReturned event 觸發，但 ReturnGoodsOrderSlave 還沒走到 Finish

<br>

#### 情境二：線上通路判斷問題導致回饋異常

**正流程：** 無符合的線上SalesChannel，跳過回饋 => 沒有正常長 couponRecord

<br>

**造成結果：** 逆流程：訂單已付款，但查無給券紀錄 => 查 SalesOrderValid 後發現沒正常長資料噴錯

<br>
# PayMe 文件

## 目錄
1. [關鍵字](#1-關鍵字)
2. [Pay](#2-pay)
3. [異常紀錄](#3-異常紀錄)
4. [Error Code](#4-error-code)

<br>

---

![alt text](./Img/image30.png)


## 1. 關鍵字

EWallet_PayMe

<br>

---

## 2. Pay

/api/v1.0/pay/EWallet_PayMe/TG250807H00015

<br>

---

## 3. 異常紀錄

### 3.1 Base-64 格式錯誤

<br>

**問題描述**

<br>

查看 log 發現錯誤訊息：

<br>

```
System.FormatException: The input is not a valid Base-64 string as it contains a non-base 64 character, more than two padding characters, or an illegal character among the padding characters.
   at NineYi.PaymentMiddleware.Plugins.PayMe.HttpClients.PayMeHttpClient.GetSignatureHeader(RequestCredentialInfo credentials, String httpMethod, String target
```

<br>

**問題分析**

<br>

- 查看 shop 12 從 8/4 中午過後開始結帳皆會錯誤
- 查看第三方金物流異動歷程 8/4 12:30 左右有更新過 key
- PayMe shop 17 付款正常成功
- shop 12 使用其他付款方式結帳正常

<br>

**處理建議**

<br>

應該需要通報 HK 協助確認 Key 是不是有問題

<br>

**相關討論**

<br>

https://91app.slack.com/archives/C7T5CTALV/p1754498676586939

<br>

### 3.2 [HK] 2024/08/26 有 1 筆訂單因 PayMe API 出現異常出現錯誤狀態，請盡速確認訂單和消費者付款狀態

**Shop 27 (和井Union Well) PayMe 設定異常**

<br>

**問題詳情**：

<br>

- **TG**：TG240826P00076
- **第三方付款時間**：2024/08/26 13:51:41
- **狀態**：已通報

<br>

**錯誤訊息**：

<br>

```
3000 Service Request Authentication Failed,EA001,Authorisation failure. Please check the client_id and client_secret and try again.
```

<br>

**通報內容**：

<br>

@channel Shop 27 (和井Union Well) PayMe 設定異常, 請盡速聯繫商店處理!!!

<br>

PayMe 付款時間: 2024/08/26 13:51:41
TG code: TG240826P00076

<br>

---

## 4. Error Code

**錯誤代碼對照表**：https://docs.google.com/spreadsheets/d/1xqzL6AJoJW2z_ieif_nI-1NrerG2cvMauuKSauLknXU/edit#gid=0

<br>

---
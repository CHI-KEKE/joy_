
## 8. Custom mode 與 Standard Mode 差異

### 8.1 開店流程新增子帳號、第三方金物流資料設定填寫帳號資料

**建立 Stripe 帳號流程**：

<br>

打 Stripe API 建立 ACC ID → 打 API 取得資料填寫連結 → 填寫完畢帳號啟用

<br>

**第三方金物流資料設定**：

<br>

第三方金物流資料設定_金流資料設定，新增判斷「商店預設值」後台第三方帳號類型

<br>

若為 Standard 則維持現有架構

<br>

若為 Custom 則顯示新架構

<br>

**帳號狀態判定**：

<br>

狀態為填寫完成 = 該子帳號的回傳值 `"charges_enabled": true`

<br>

**KYC 表單填寫**：

<br>

點擊「Stripe 子帳號頁面」轉導到「Stripe KYC form」

<br>

由客戶填寫 KYC 表單（公司、代表人資訊、聯絡方式）

<br>

### 8.2 金流手續費費率新增國別和卡別維度

**費率計算維度擴展**：

<br>

新增國別維度計算

<br>

新增卡別維度計算

<br>

支援更細緻的手續費費率設定

<br>

### 8.3 結帳判斷商店 Stripe 帳號類型

**結帳流程判斷**：

<br>

判斷商店 Stripe 帳號類型

<br>

使用 Custom 新架構

<br>

根據信用卡國別和卡別計算金流手續費用

<br>

### 8.4 對帳報表和入賬明細表

**報表功能**：

<br>

對帳報表支援 Custom mode

<br>

入賬明細表支援新架構

<br>

### 8.5 Statement Descriptor 設定

**描述符規則**：

<br>

`statement descriptor` 的值來自於商店合約申請單的「公司英文名稱」

<br>

`statement descriptor` 的值限制是 22 個字

<br>

若超過 22 個字，只會顯示前 22 個字元

<br>

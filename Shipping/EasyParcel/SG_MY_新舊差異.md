
目前 SubmitOrder 已知的 新舊 API 差異 (跟三方金物流存的資料有關係, 還有實際測試送貨會有關係)
商店連絡電話會檢查前綴 +65
新加坡多的必填節點
Pick Unit
提貨點資訊現在是存 ShopDefault
PickContact
PickAddress
PickZipCode
Pick Unit   => 沒有這個, 我會從 PickAddress 找,  沒就帶 "N/A" 送去 easyParcel
Send Unit
送貨點資訊會拿 SalesOrderReceiver 的資料表的資料, 目前沒特別存 Send Unit 的資料,我會從  send address 找, 沒就 "N/A"  送去 easyParcel

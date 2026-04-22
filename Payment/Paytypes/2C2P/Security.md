# 2C2P — 安全設定

## 金鑰配置說明

![金鑰配置示意圖](./Img/image29.png)
![金鑰配置示意圖2](./Img/image28.png)

| 欄位 | 用途 |
|------|------|
| receiverPubKey（左上） | 逆流程加密用，2C2P 以對應私鑰解密 |
| JWT Secret Key（左中） | 正流程加密用 |
| PubKey / Private（右下角） | 逆流程簽章 |

<br>

## 金鑰命名規則

僅識別使用 `KeyName = $"S{SupplierId}S{ShopId}"`，例如：`S12S34`

<br>

## 金鑰產生方式

### 方法一：使用官方工具

使用 2C2P 官方提供的 Key Generation 工具：[Certificate Generation Guide](https://developer.2c2p.com/docs/certificate-generation-guide)

### 方法二：使用 OpenSSL

下載 OpenSSL 後，在 PowerShell 逐行執行以下語法：

- `.key` 檔為商戶私鑰
- `.crt` 檔為商戶公鑰

```powershell
# 請一行一行執行並輸入每個 Command 所需資訊
# 僅識別使用 KeyName = $"S{SupplierId}S{ShopId}"
$KeyName="S12S34"

# 產生 key (這就是商戶私鑰)
&"C:\Program Files\OpenSSL-Win64\bin\openssl.exe" genrsa -out "$KeyName.key" 2048

# 產生 csr (後續相關參數可以直接 Enter 跳過)
# 相關參數說明可以參考 https://blog.miniasp.com/post/2022/06/14/How-to-request-new-tls-certificate-using-OpenSSL
&"C:\Program Files\OpenSSL-Win64\bin\openssl.exe" req -new -key "$KeyName.key" -out "$KeyName.csr"

# 產生 crt (這就是商戶公鑰)
&"C:\Program Files\OpenSSL-Win64\bin\openssl.exe" x509 -req -in "$KeyName.csr" -signkey "$KeyName.key" -out "$KeyName.crt" -days 365

# 產生 cer
&"C:\Program Files\OpenSSL-Win64\bin\openssl.exe" x509 -inform PEM -in "$KeyName.crt" -outform DER -out "$KeyName.cer"

# 產生 pem
&"C:\Program Files\OpenSSL-Win64\bin\openssl.exe" x509 -pubkey -in "$KeyName.cer" -noout > "$KeyName.pem"
```

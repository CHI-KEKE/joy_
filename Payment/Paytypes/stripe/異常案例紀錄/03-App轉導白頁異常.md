# 異常案例 03 — App 轉導後白頁異常事件

### 1.3 App 轉導後白頁異常事件

#### 基本資訊
- **Slack 連結**：https://91app.slack.com/?redir=%2Farchives%2FCMY85JQLC%2Fp1742887474296019%3Fname%3DCMY85JQLC%26perma%3D1742887474296019
- **發生時間**：2025/03/25 上午 9:00-11:00
- **影響範圍**：Android（Mobile 及 APP）

#### 問題概述
多間 HK 商店反映 Android 平台使用信用卡結帳時，用戶按送出後出現白頁，無法完成交易。

#### 異常證據

##### Stripe 官方公告
![alt text](./Img/image-25.png)

##### 每小時 Timeout 監控數據
![alt text](./Img/image-26.png)

#### 影響範圍

##### 受影響商店
| 商店名稱 | 商店 ID | 備註 |
|----------|---------|------|
| Eu Yan Sang | 12 | Android 平台 |
| 繽粉 | 67 | Android 平台 |
| CUAPP | 28 | 第三方 App 使用 MWeb 付款 |

##### 異常訂單範例
| 訂單號 | 銀行 | 卡別 |
|--------|------|------|
| TG250325Q00033 | HSBC | VISA |
| TG250325E00015 | CHINA CITIC BANK INTERNATIONAL | MASTER |
| TG250325K00083 | STANDARD CHARTERED BANK (HONG) | MASTER |
| TG250325L00120 | HANG SENG BANK LIMITED | MASTER |
| TG250325M00006 | HANG SENG BANK LIMITED | MASTER |

##### 典型案例資訊
- **商店**：80 Melvita HK
- **訂單號**：TG250325R00077
- **平台**：iOS App
- **付款方式**：信用卡 - VISA
- **下單時間**：2025/03/25 15:43:04

#### 監控查詢語法
用於分析每小時成功率的 SQL：

```sql
USE WebStoreDB;

WITH HourlyStats AS (
    SELECT 
        DATEADD(SECOND, 58, 
            DATEADD(MINUTE, 1, 
                DATEADD(HOUR, 
                    DATEDIFF(HOUR, 0, TradesOrderThirdPartyPayment_CreatedDateTime), 0
                )
            )
        ) AS HOURLY,
        SUM(CASE 
            WHEN TradesOrderThirdPartyPayment_StatusDef NOT IN ('WaitingToPay', 'Hidden') 
            THEN 1 ELSE 0 
        END) AS Total_Count,
        SUM(CASE 
            WHEN TradesOrderThirdPartyPayment_StatusDef IN ('Success', 'RePaySuccess', 'AuthSuccess', 'CancelAfterSuccess') 
            THEN 1 ELSE 0 
        END) AS Success_Count,
        SUM(CASE 
            WHEN TradesOrderThirdPartyPayment_StatusDef = 'Fail' 
            THEN 1 ELSE 0 
        END) AS Fail_Count,
        SUM(CASE 
            WHEN TradesOrderThirdPartyPayment_StatusDef = 'Timeout' 
            THEN 1 ELSE 0 
        END) AS Timeout_Count,
        SUM(CASE 
            WHEN TradesOrderThirdPartyPayment_StatusDef = 'CancelRequest' 
            THEN 1 ELSE 0 
        END) AS CancelRequest_Count
    FROM TradesOrderThirdPartyPayment (NOLOCK)
    WHERE TradesOrderThirdPartyPayment_ValidFlag = 1
      AND TradesOrderThirdPartyPayment_TypeDef = 'CreditCardOnce_Stripe'
      AND TradesOrderThirdPartyPayment_DateTime BETWEEN '2024-03-23' AND '2024-03-24'
    GROUP BY DATEADD(SECOND, 58, 
        DATEADD(MINUTE, 1, 
            DATEADD(HOUR, 
                DATEDIFF(HOUR, 0, TradesOrderThirdPartyPayment_CreatedDateTime), 0
            )
        )
    )
)
SELECT 
    HOURLY,
    Total_Count,
    Success_Count,
    Fail_Count,
    Timeout_Count,
    CancelRequest_Count,
    CAST(Success_Count * 100 / Total_Count AS VARCHAR(20)) + '%' AS Success_Rate
FROM HourlyStats
ORDER BY HOURLY;
```

#### 後續處理
- 確認 Stripe 服務狀態
- 監控系統恢復情況
- 分析白頁原因並提供解決方案

<br>


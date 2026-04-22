# Razer — Fiuu 整合

Fiuu 為 Razer 的新品牌名稱，部分功能透過 Feature Toggle 控制開關。

參考討論：https://91app.slack.com/archives/G019P1APXMY/p1758703072649869?thread_ts=1758698124.491279&cid=G019P1APXMY

官方 API 文件（本機）：`C:\Users\Allen Lin\Downloads\[official API] Fiuu API Spec for Merchant (v13.90) (5).pdf`

<br>

## Sandbox / Production API 端點差異

| 環境 | Domain |
|------|--------|
| Sandbox | `sandbox-api.fiuu.com` |
| Production | `api.fiuu.com` |

**注意：** 下列 Sandbox 端點需使用 `sandbox-api.fiuu.com`（非 `sandbox-payment.fiuu.com`）：
- `https://sandbox-payment.fiuu.com/RMS/q_by_tid.php`
- `https://sandbox-payment.fiuu.com/RMS/API/refundAPI/index.php`
- `https://sandbox-payment.fiuu.com/RMS/API/refundAPI/q_by_txn.php`

<br>

## 測試卡

https://wiki.91app.com/pages/viewpage.action?pageId=56342554

<br>

## Feature Toggle（IsFiuuEnable）

### 開關設定

```
ShopStaticSetting
- ShopStaticSetting_GroupName = 'RazerPay'
- ShopStaticSetting_Key       = 'IsFiuuEnable'
- ShopStaticSetting_ShopId IN (0, 200071, 200136)
```

### SQL 語法

```sql
-- 查詢
SELECT *
FROM dbo.ShopStaticSetting WITH (NOLOCK)
WHERE ShopStaticSetting_ValidFlag = 1
  AND ShopStaticSetting_GroupName = 'RazerPay'
  AND ShopStaticSetting_Key = 'IsFiuuEnable'
  AND ShopStaticSetting_ShopId IN (0, 200071, 200136)
ORDER BY ShopStaticSetting_ShopId;

-- 新增設定
INSERT dbo.ShopStaticSetting
    (
        ShopStaticSetting_ShopId,
        ShopStaticSetting_GroupName,
        ShopStaticSetting_Key,
        ShopStaticSetting_Value,
        ShopStaticSetting_Description,
        ShopStaticSetting_CreatedUser,
        ShopStaticSetting_UpdatedUser
    )
VALUES
    ( 0,      'RazerPay', 'IsFiuuEnable', 'false', 'Toggle Fiuu payment integration', 'VSTS544811', 'VSTS544811' ),
    ( 200071, 'RazerPay', 'IsFiuuEnable', 'true',  'Toggle Fiuu payment integration', 'VSTS544811', 'VSTS544811' ),  -- 馬幣站
    ( 200136, 'RazerPay', 'IsFiuuEnable', 'true',  'Toggle Fiuu payment integration', 'VSTS544811', 'VSTS544811' );  -- 星幣站
```

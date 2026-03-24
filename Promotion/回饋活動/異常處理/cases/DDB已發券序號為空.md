## 🚨 異常訊息

```log
@oversea_brd [MY][Prod]
優惠券紀錄稽核異常
ServiceName: AuditPromotionRewardCouponService
ShopId: 200009
異常項目:
DDBKey: 8444_MG250722L00007
ECouponId: 222755
MemberId: 549049
錯誤: DDB 已發券序號為空
```

## 🔍 根因分析

| 問題面向 | 詳細說明 |
|----------|----------|
| **預期行為** | 應發送 3 張優惠券並記錄 SlaveIdList |
| **實際狀況** | `"GivenCouponSlaveIdList": []` 為空陣列 |
| **系統判定** | 稽核系統認為發券失敗 |
| **實際情況** | 優惠券可能已正常發放，但記錄缺失 |

## 📋 問題點整理
1. **API 日誌缺失**: SCMAPIV2 無法從 Grafana 查詢 API 日誌
2. **程式版本問題**: `GivenCouponSlaveIdList` 記錄功能為新版程式才支援
3. **稽核誤判**: 舊版程式發放的券無此記錄，導致稽核誤判為異常


## 🔧 手動驗證方法

**查詢實際發券狀況**:
```sql
USE WebStoreDB

SELECT ECoupon_Code, ECoupon_Modes, ECouponSlave_MemberId, *
FROM ECoupon(NOLOCK)
INNER JOIN ECouponSlave(NOLOCK) ON ECoupon_Id = ECouponSlave_ECouponId
WHERE ECoupon_ValidFlag = 1
  AND ECoupon_ShopId = 200009
  AND ECoupon_Id = 222755
```

## 💡 解決
- **短期**: 手動查詢資料庫確認實際發券狀況
- **長期**: 調整稽核邏輯，兼容新舊版程式的記錄格式
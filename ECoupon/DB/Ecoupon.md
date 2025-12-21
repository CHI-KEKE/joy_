

## ECouponCustom + ECoupon

```sql
-- 自訂優惠券完整查詢
USE WebStoreDB;

SELECT 
    ECouponCustom_Name AS 券券名稱,
    ECoupon_ShopId AS 店鋪ID,
    ECoupon_Id AS 券券ID,
    ECoupon_DiscountPercent AS 折扣百分比,
    ECoupon_DiscountPrice AS 折扣金額,
    *
FROM ECouponCustom(NOLOCK)
INNER JOIN ECouponCustomMapping(NOLOCK)
    ON ECouponCustom_Id = ECouponCustomMapping_ECouponCustomId
INNER JOIN ECoupon(NOLOCK)
    ON ECoupon_Id = ECouponCustomMapping_ECouponId
WHERE ECouponCustom_ValidFlag = 1
    AND ECoupon_ShopId = 10230 
    AND ECoupon_ValidFlag = 1;
```

<br>

## ECoupon + ECouponSlave

```sql
-- 會員優惠券查詢
USE WebStoreDB;

SELECT 
    ECoupon_Id AS 券券ID,
    ECouponSlave_Id AS 券券明細ID,
    ECouponSlave_MemberId AS 會員ID,
    ECouponSlave_ValidFlag AS 有效狀態,
    ECouponSlave_UpdatedDateTime AS 更新時間,
    ECouponSlave_UpdatedUser AS 更新者,
    *
FROM ECoupon(NOLOCK)
INNER JOIN ECouponSlave(NOLOCK)
    ON ECouponSlave_ECouponId = ECoupon_Id
WHERE ECoupon_Id = 222734
    AND ECouponSlave_MemberId = 2778955;
```

<br>

## Ids

```sql
-- 店鋪優惠券批次查詢
DECLARE @TargetIds TABLE (Id BIGINT);
INSERT INTO @TargetIds VALUES (41571), (10230), (12345); -- 示例店鋪ID

SELECT 
    ECouponSlave_Id AS 券券明細ID,
    ECouponSlave_ShopId AS 店鋪ID,
    ECouponSlave_ValidFlag AS 有效狀態,
    ECouponSlave_UpdatedDateTime AS 更新時間,
    ECouponSlave_UpdatedUser AS 更新者,
    *
FROM dbo.ECouponSlave(NOLOCK)
WHERE ECouponSlave_ShopId IN (SELECT Id FROM @TargetIds)
ORDER BY ECouponSlave_UpdatedDateTime DESC;
```
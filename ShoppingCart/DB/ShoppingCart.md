
select *
from ShoppingCart(nolock)
where ShoppingCart_ValidFlag = 1



select *
from ShoppingCart(nolock)
where ShoppingCart_ValidFlag = 1
and ShoppingCart_MemberId = 747292




INSERT INTO dbo.ShoppingCart
(
    ShoppingCart_MemberOrUnloginId,
    ShoppingCart_MemberId,
    ShoppingCart_UnloginId,
    ShoppingCart_TypeDef,
    ShoppingCart_DateTime,
    ShoppingCart_SalePageGroupSeq,
    ShoppingCart_ShopId,
    ShoppingCart_SalePageId,
    ShoppingCart_SaleProductId,
    ShoppingCart_SaleProductSKUId,
    ShoppingCart_IsMajor,
    ShoppingCart_IsExtra,
    ShoppingCart_IsOption,
    ShoppingCart_IsGift,
    ShoppingCart_Qty,
    ShoppingCart_CreatedDateTime,
    ShoppingCart_CreatedUser,
    ShoppingCart_UpdatedTimes,
    ShoppingCart_UpdatedDateTime,
    ShoppingCart_UpdatedUser,
    ShoppingCart_ValidFlag,
    ShoppingCart_OptionalTypeId,
    ShoppingCart_OptionalTypeDef
)
VALUES
(
    747292,
    747292,
    '7c74d9dd-389f-4f53-8dec-b5198c6e0d12',
    '',
    '2026-04-16 18:38:27.897',
    0,
    10230,
    1557572,
    0,
    2169000,
    1, --major
    0, -- extra
    0, -- option
    0, -- gift
    1, -- qty
    '2026-04-16 18:38:27.897',
    747292,
    0,
    '2026-04-16 18:38:27.897',
    747292, -- updateuser
    1,
    231422,
    'CartReachPieceExtraPurchase'
);

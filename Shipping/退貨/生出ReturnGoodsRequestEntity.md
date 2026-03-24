

產生 ReturnGoodsRequestEntity

//// 退貨申請單單號
ReturnGoodsRequest_Code = code,

//// 退貨原因
//// 備註: 雖然退貨原因是必填寫欄位 但是滿額贈品不會有取消原因 所以需要補預設值
ReturnGoodsRequest_CauseDef = selectedTs.CauseDef ?? "4",

//// 退貨描述
ReturnGoodsRequest_CauseDesc = confirmEntity.CauseDescription,

ReturnGoodsRequest_IsClosed = false,
ReturnGoodsRequest_MemberId = memberId,

//// 退貨單狀態
ReturnGoodsRequest_StatusDef = ReturnGoodsRequestStatusEnum.WaitingToTrans.ToString(),
ReturnGoodsRequest_StatusUpdatedDateTime = DateTime.Now,
ReturnGoodsRequest_StatusUpdatedUser = memberId.ToString(),

//// 付款方式
ReturnGoodsRequest_PayTypeDef = payProfileTypeDef.ToString(),
ReturnGoodsRequest_PayProfileTypeDef = payProfileTypeDef.ToString(),

//// 主單子單資訊
ReturnGoodsRequest_TradesOrderId = tsFromDb.TradesOrderId,
ReturnGoodsRequest_Qty = Convert.ToInt16(tsFromDb.Qty),
ReturnGoodsRequest_TradesOrderSlaveId = tsFromDb.Id,
ReturnGoodsRequest_TradesOrderSlaveCode = tsFromDb.Code,

//// 退款資訊
ReturnGoodsRequest_BankName = confirmEntity.RefundInfo.BankName,
RerurnGoodsRequest_BankSubName = confirmEntity.RefundInfo.BankSubName,
RerurnGoodsRequest_BankAccountNo = confirmEntity.RefundInfo.BankAccountNo,
RerurnGoodsRequest_BankAccountOwner = confirmEntity.RefundInfo.BankAccountOwner,
ReturnGoodsRequest_IdentityId = confirmEntity.RefundInfo.IdentityId,
ReturnGoodsRequest_SaleProductProductTypeDef = tsFromDb.SaleProductProductTypeDef,
ReturnGoodsRequest_SaleProductVirtualProductProviderTypeDef = tsFromDb.SaleProductVirtualProductProviderTypeDef,
ReturnGoodsRequest_VirtualProducInfo = tsFromDb.ShippingProfileTypeDef == ShippingProfileTypeDefEnum.DigitalDelivery.ToString() && tsFromDb.SaleProductVirtualProductProviderTypeDef == "Edenred" ?
    tsFromDb.VoucherRefundAmountInfo.VoucherList.Select(x => new VoucherRefundEntity() { Id = x.VoucherId, RefundQty = x.RefundQty }).ToJSON() : string.Empty,
ReturnGoodsRequest_NonRefundAmount = tsFromDb.ShippingProfileTypeDef == ShippingProfileTypeDefEnum.DigitalDelivery.ToString() && tsFromDb.SaleProductVirtualProductProviderTypeDef == "Edenred" ?
    tsFromDb.VoucherRefundAmountInfo.VoucherList.Sum(x => x.UnRefundableAmt) : 0,

//// 取貨資訊
ReturnGoodsRequest_ZipCode = confirmEntity.MemberLocationInfo.ZipCode ?? string.Empty,
ReturnGoodsRequest_State = confirmEntity.MemberLocationInfo.State ?? string.Empty,
ReturnGoodsRequest_City = confirmEntity.MemberLocationInfo.City ?? string.Empty,
ReturnGoodsRequest_District = confirmEntity.MemberLocationInfo.District ?? string.Empty,
ReturnGoodsRequest_Address = confirmEntity.MemberLocationInfo.Address == null ? string.Empty : confirmEntity.MemberLocationInfo.Address,
ReturnGoodsRequest_CellPhone = confirmEntity.MemberLocationInfo.CellPhone,
ReturnGoodsRequest_Contact = confirmEntity.MemberLocationInfo.FullName,
ReturnGoodsRequest_County = confirmEntity.MemberLocationInfo.CountryEnglishName,
ReturnGoodsRequest_CountryCode = confirmEntity.MemberLocationInfo.CountryCode,
ReturnGoodsRequest_CountryProfileId = confirmEntity.MemberLocationInfo.CountryProfileId,

//// 退貨方式
ReturnGoodsRequest_TypeDef = ((byte)confirmEntity.ReturnShippingTypeDef).ToString(),

//// 若訂單子單上有加註 不代開發票 應將代為處理發票或折讓相關事宜 取消勾選
RerurnGoodsRequest_IsAgreedToDelegate = tsFromDb.IsIssuedInvoice ? confirmEntity.IsAgreedToDelegate : false


webstoredb


//// 1. 建立退貨申請單
//// 2. 更新OrderSlaveFlow 狀態與相關退貨資訊
//// 退貨相關資訊
flow.OrderSlaveFlow_ReturnGoodsRequestId = request.ReturnGoodsRequest_Id;
flow.OrderSlaveFlow_ReturnGoodsRequestIsClosed = false;
flow.OrderSlaveFlow_ReturnGoodsRequestStatusCause = request.ReturnGoodsRequest_StatusCause;

//// 退貨單狀態切換為待轉單
flow.OrderSlaveFlow_ReturnGoodsRequestStatusDef = ReturnGoodsRequestStatusEnum.WaitingToTrans.ToString();
flow.OrderSlaveFlow_ReturnGoodsRequestStatusUpdatedDateTime = request.ReturnGoodsRequest_StatusUpdatedDateTime;

//// OrderSlaveFlow 狀態切換為退貨處理中
flow.OrderSlaveFlow_StatusForSCMDef = MemberTradesOrderOrderSlaveFlowStatusForScmEnum.Hide.ToString();
flow.OrderSlaveFlow_StatusDef = MemberTradesOrderOrderSlaveFlowStatusEnum.ReturnGoodsRequesting.ToString();
flow.OrderSlaveFlow_StatusForUserDef = MemberTradesOrderOrderSlaveFlowStatusForUserEnum.ReturnGoodsRequesting.ToString();

flow.OrderSlaveFlow_CanCancel = false;
flow.OrderSlaveFlow_CanReturn = false;
flow.OrderSlaveFlow_CanChange = false;


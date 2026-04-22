# ApplePay / GooglePay — 付款 / 查詢 / 退款 Response

## 9. 付款（Pay）

Middleware Pay Response

```json
{
  "request_id": "791025f2-bc7e-4bf8-a69b-c2269318b065",
  "return_code": "0000",
  "return_message": "Success",
  "transaction_id": "pi_3PpiJTHfnYtXGyLl1wSvBM8U",
  "tg_code": "TGCodeForTest",
  "payment_action": null,
  "extend_info": {
    "payment_intent_id": "pi_3PpiJTHfnYtXGyLl1wSvBM8U",
    "charge_id": "ch_3PpiJTHfnYtXGyLl1z1oy8qa",
    "payment_method": "pm_1PpiIdHfnYtXGyLlQ8cs4g5x",
    "client_secret": "pi_3PpiJTHfnYtXGyLl1wSvBM8U_secret_kALJFnCt1XEnt6FYjIfCixES6"
  }
}
```

> **注意**：`extend_info.client_secret` 為行動錢包付款必要回傳欄位，前端需使用此值完成 Apple/Google Pay session。

---

## 10. 查詢（Query）

Middleware Query Response

```json
{
  "request_id": "91cd098e-0c05-4ee1-9462-12a28be57dab",
  "return_code": "0000",
  "return_message": "succeeded",
  "transaction_id": "pi_3PpiJTHfnYtXGyLl1wSvBM8U",
  "extend_info": {
    "payment_intent_id": "pi_3PpiJTHfnYtXGyLl1wSvBM8U",
    "charge_id": "ch_3PpiJTHfnYtXGyLl1z1oy8qa"
  }
}
```

---

## 11. 退款（Refund）

Middleware Refund Response

```json
{
  "request_id": "9e84c95b-2ec0-46ae-8282-5e8a01897ddf",
  "transaction_id": "re_3PpiJTHfnYtXGyLl1RwXH0r8",
  "return_code": "0000",
  "return_message": "success",
  "extend_info": null
}
```

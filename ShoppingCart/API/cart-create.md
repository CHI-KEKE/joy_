

## path

http://cart-api-internal.qa.91dev.tw/api/carts/create


## request body

```json
{
  "HasCrmShopContract": true,
  "CrmShopMemberCardList": [
    {
      "CrmShopMemberCardId": 32,
      "CrmShopMemberCardLevel": 10,
      "CrmShopMemberCardName": "等級 0"
    },
    {
      "CrmShopMemberCardId": 1240,
      "CrmShopMemberCardLevel": 20,
      "CrmShopMemberCardName": "等級 1"
    }
  ],
  "CrmMemberTier": {
    "CrmShopMemberCardId": 32,
    "CrmShopMemberCardName": "等級 0"
  },
  "IsMemberFirstPurchase": false,
  "BlacklistMemberAllowedPayProfileList": [],
  "UserClientTrack": {
    "TrackSourceType": "Web",
    "TrackChannelType": "N1Shopping",
    "TrackDeviceType": "Mobile",
    "HasTrackData": true
  },
  "CouponSetting": {
    "MultipleRedeem": {
      "Discount": {
        "IsMultiple": true,
        "Qty": 10
      },
      "Gift": {
        "IsMultiple": true,
        "Qty": 1
      },
      "Shipping": {
        "IsMultiple": false,
        "Qty": 1
      }
    }
  },
  "CouponInfoList": [],
  "LoyaltyPoint": {
    "MemberTotalBalancePoint": 0.0,
    "IsEnableOption": true,
    "IsUsing": true,
    "IsServiceSuccessful": true,
    "MaxPoints": 0,
    "MaxDiscountPrice": 0,
    "IsShopSelfOwned": false,
    "IsShopSelfOwnedCalculate": false,
    "IsPointsPayEnabled": true,
    "DisplayUsingPointsOption": true,
    "RedeemPointsDollarsPair": {
      "Points": 0,
      "Dollars": 0
    },
    "CheckoutPoints": 0,
    "CheckoutDiscountPrice": 0,
    "MaxDiscountPercentage": 0,
    "LackOfPrice": 0,
    "IsBalanceSufficient": true,
    "IsItemSalePriceSufficient": true,
    "ReachPrice": 0,
    "IsReachPrice": true
  },
  "PromoCodeDispatch": {
    "DispatchStatus": "None"
  },
  "UserAgent": "Mozilla/5.0 (iPhone; CPU iPhone OS 17_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.4 Mobile/15E148 Safari/604.1",
  "HttpReferer": "https://10230.shop.qa.91dev.tw/V2/ShoppingCart/Index?shopId=10230",
  "MemberId": 851237,
  "Auth": "SqbTo0ZpYFaN9ncw9AevEO0zljW4BF9xocAObGdJ9WPs9q75p\\u002BFANdiegcojD2P3DxB0ad2EzNs/J5L9Calha6AyTsRQsw9YmXucGfo1ktTmTMrOczEhUBdBImRCorxSK57le7lwf9nL7Bn\\u002BFTHbJC\\u002Bjq8Nreh9w2EEdnwBGh4GdOaXNg61wugjPPj1b1lpG9eK4NUWqcP5kW5uqMr3aszDHdSohlws8QmNu/QR1slvqEOyx8LgW0vFa\\u002BhAY\\u002BUygR1eg\\u002BIizeqPwNYwNkR26VNOzUNM395B8eImyjpcrMvq4\\u002Bovu6dGyEBh9Us\\u002BDIHwHcvAeagbB0XrKLt3ezRubXRpJ5iHIJuL6LihzCdFYvllAMWzV81GCUAj77J\\u002B6NrjxxXpvyWqktXe8fK7wPtQkWb1pFswCbb7tCRxZc2Kkezf1Bk3U0Fa/YMuGprM6CgIXevaqCzi6flMy92Xv7zjvvbFxyPwIthe5raKKPqrSuiM\\u002BhqEaMdOpluTqQ2v8\\u002BDagjSghSc1C8u7629Dcmk0VDbB3LkCBExaailWK6WWskRkXwTz\\u002BFs3DRDPxTEqMF/op4KNsdLRx3fVV3eQvyE7Ac0k1sxTemgylqcgA3BJr6l8tjgqzDm4JwMFq6SUtXY430whp45t4MKitHPL7osJXZLh8wwJIjEmJACUFpFLH2O3M0MBolCDYBmEioPugUwzF1M2K7cDvDG84Tv6dX\\u002Bmz74tOOd8ytHOwE2HkzuBMD4rYnLQvBm69hVevCdgtpw0XbjytO9I4djSdPjZOMQ450A2oGr9t0jADq6fPC7kAQKAO0Ywvv4O\\u002BUR5K4kvdAD1yDS5BcSKeIIBwIgztW00Z0qQNGfeBadK74wCYVMTf6dqBfkBbyJCPo0LZbxKY3CI\\u002BtD9pHLJJe0MPswqrB61X4/SphROPVliAxwI/1fk3mTIW6XFBFArS49V0iH33ydx5",
  "AuthSameSite": "SqbTo0ZpYFaN9ncw9AevEO0zljW4BF9xocAObGdJ9WPs9q75p\\u002BFANdiegcojD2P3DxB0ad2EzNs/J5L9Calha6AyTsRQsw9YmXucGfo1ktTmTMrOczEhUBdBImRCorxSK57le7lwf9nL7Bn\\u002BFTHbJC\\u002Bjq8Nreh9w2EEdnwBGh4GdOaXNg61wugjPPj1b1lpG9eK4NUWqcP5kW5uqMr3aszDHdSohlws8QmNu/QR1slvqEOyx8LgW0vFa\\u002BhAY\\u002BUygR1eg\\u002BIizeqPwNYwNkR26VNOzUNM395B8eImyjpcrMvq4\\u002Bovu6dGyEBh9Us\\u002BDIHwHcvAeagbB0XrKLt3ezRubXRpJ5iHIJuL6LihzCdFYvllAMWzV81GCUAj77J\\u002B6NrjxxXpvyWqktXe8fK7wPtQkWb1pFswCbb7tCRxZc2Kkezf1Bk3U0Fa/YMuGprM6CgIXevaqCzi6flMy92Xv7zjvvbFxyPwIthe5raKKPqrSuiM\\u002BhqEaMdOpluTqQ2v8\\u002BDagjSghSc1C8u7629Dcmk0VDbB3LkCBExaailWK6WWskRkXwTz\\u002BFs3DRDPxTEqMF/op4KNsdLRx3fVV3eQvyE7Ac0k1sxTemgylqcgA3BJr6l8tjgqzDm4JwMFq6SUtXY430whp45t4MKitHPL7osJXZLh8wwJIjEmJACUFpFLH2O3M0MBolCDYBmEioPugUwzF1M2K7cDvDG84Tv6dX\\u002Bmz74tOOd8ytHOwE2HkzuBMD4rYnLQvBm69hVevCdgtpw0XbjytO9I4djSdPjZOMQ450A2oGr9t0jADq6fPC7kAQKAO0Ywvv4O\\u002BUR5K4kvdAD1yDS5BcSKeIIBwIgztW00Z0qQNGfeBadK74wCYVMTf6dqBfkBbyJCPo0LZbxKY3CI\\u002BtD9pHLJJe0MPswqrB61X4/SphROPVliAxwI/1fk3mTIW6XFBFArS49V0iH33ydx5",
  "UAuth": "ZZlDgoO3AEl1c5FakMd0bTbqiMY3R5PuEb8631s1vnl5YWqEEmCezjW9hgPj1rtuDOYhkkLFGk5tWilaus7DzzbB41Ja\\u002BcYN0ytttpEouuM=",
  "UAuthSameSite": "ZZlDgoO3AEl1c5FakMd0bTbqiMY3R5PuEb8631s1vnl5YWqEEmCezjW9hgPj1rtuDOYhkkLFGk5tWilaus7DzzbB41Ja\\u002BcYN0ytttpEouuM=",
  "Currency": "TWD",
  "UnloginId": "7c1417cb-e2d5-405d-8d00-cb428ee41e30",
  "FMICFromType": ""
}

```

## Controllers

"ControllerShortName": "Carts",
"ActionShortName": "Create",
"RequestPath": "/api/carts/create"
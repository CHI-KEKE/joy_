

https://sms.qa1.my.91dev.tw/Api/ShopMemberMaintain/UpdateShopMemberInfo/83/98000050


{
    "ShopId": 83,
    "MemberId": 98000050,
    "Email": "G@GMIL.COM",
    "FirstName": "",
    "LastName": "",
    "OuterId": "",
    "Birthday": null,
    "CountryCode": "886",
    "IdentityCardId": "",
    "LocalPhone": "",
    "GenderTypeDef": null,
    "MarialStatusTypeDef": null,
    "ProfessionTypeDef": null,
    "AnnualIncomeTypeDef": null,
    "DependentsTypeDef": null,
    "EducationTypeDef": null,
    "NoticeLanguageTypeDef": "zh-TW",
    "Custom1": "",
    "Custom2": "",
    "Custom3": "",
    "LocationCountry": "Singapore 新加坡",
    "LocationCity": "",
    "LocationState": "",
    "LocationDistrict": "",
    "LocationAddress": "fegrgfds",
    "LocationZipCode": "56089"
}



## Validator


       /// <summary>
        /// 檢查是否符合電話格式
        /// </summary>
        /// <param name="value">被驗證字串</param>
        /// <returns>驗證結果</returns>
        private static bool IsPhone(string value)
        {
            string regularExpression = @"^[0-9\+\-\#\*]+$";
            if (HasFilled(value) == false)
            {
                return false;
            }

            return Regex.IsMatch(value, regularExpression);
        }

        /// <summary>
        /// 檢查是否符合地址格式
        /// </summary>
        /// <param name="value">被驗證字串</param>
        /// <returns>驗證結果</returns>
        private static bool IsAddress(string value)
        {
            //// 地址不允許輸入 "|"
            string regularExpression = @"^[^|]+$";
            if (HasFilled(value) == false)
            {
                return false;
            }

            return Regex.IsMatch(value, regularExpression);
        }


不會擋地址


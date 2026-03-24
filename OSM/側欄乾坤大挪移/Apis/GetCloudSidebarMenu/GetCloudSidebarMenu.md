
https://sms.qa1.hk.91dev.tw/Api/Home/GetCloudSidebarMenu?shopId=2


```csharp
this._authService.GetCloudSidebarMenu(user.SupplierId, user.RolesId, user.UsersId, user.SystemName, cloudType, cleanCache: cleanCache, shopId: shopId)
                                        .CloudMenu;
```


```json
{
    "Status": "Success",
    "Data": [
        {
            "CloudType": "Commerce",
            "Modules": [
                {
                    "Id": 175,
                    "Name": "關鍵指標數據",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 384,
                            "Name": "關鍵指標數據",
                            "Url": "https://sms.qa1.hk.91dev.tw/PerformanceReport/KeyPerformanceIndicators",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 183,
                    "Name": "App x LINE 報表",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 881,
                            "Name": "App x LINE 報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/AppLineMemberReport",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 140,
                    "Name": "開店進度",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 568,
                            "Name": "開店進度",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopOpen/Progress",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 141,
                    "Name": "商店與佈景設定",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 852,
                            "Name": "自訂會員專區",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/CustomVipMemberSetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 402,
                            "Name": "商店佈景設計",
                            "Url": "https://collage.qa1.hk.91dev.tw/",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 836,
                            "Name": "智慧推薦設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/IntelligentRecommendation",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 403,
                            "Name": "主題推薦設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/InfoModule/List",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 514,
                            "Name": "主題推薦顯示設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/InfoModule/Setting",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 403
                        },
                        {
                            "Id": 515,
                            "Name": "新增圖文模組",
                            "Url": "https://sms.qa1.hk.91dev.tw/InfoModule/ArticleEdit",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 403
                        },
                        {
                            "Id": 516,
                            "Name": "新增相簿模組",
                            "Url": "https://sms.qa1.hk.91dev.tw/InfoModule/AlbumEdit",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 403
                        },
                        {
                            "Id": 517,
                            "Name": "新增影音模組",
                            "Url": "https://sms.qa1.hk.91dev.tw/InfoModule/VideoEdit",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 403
                        },
                        {
                            "Id": 405,
                            "Name": "活動版位設定 (廣告設定)",
                            "Url": "https://scm.qa1.hk.91dev.tw/LayoutTemplateData/LayoutTemplateDataMasterListManager",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 704,
                            "Name": "金流行銷廣告設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/advertisetag/list",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 705,
                            "Name": "新增金流行銷廣告設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/AdvertiseTag/Form",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 704
                        },
                        {
                            "Id": 404,
                            "Name": "\n活動頁設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Activity/List",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 518,
                            "Name": "新增活動頁",
                            "Url": "https://sms.qa1.hk.91dev.tw/Activity/Edit",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 404
                        },
                        {
                            "Id": 519,
                            "Name": "從範本新增活動頁",
                            "Url": "https://sms.qa1.hk.91dev.tw/Activity/TemplateList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 404
                        },
                        {
                            "Id": 570,
                            "Name": "商店設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Shop/Setting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 764,
                            "Name": "商品卡片顯示設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ShopDefault/ProductCardSetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 758,
                            "Name": "前台資安顯示設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/ToggleAntiFraudAnnouncement",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 614,
                            "Name": "語言顯示設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/LanguageSetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 617,
                            "Name": "幣別顯示設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/CurrencySetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 142,
                    "Name": "Web 與 App 設定",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 571,
                            "Name": "官網設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/OfficialShop/Setting",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 569,
                            "Name": "App 製作",
                            "Url": "https://sms.qa1.hk.91dev.tw/Sidebar/SsoToAppGen",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 865,
                            "Name": "官網導下載設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/AppDownloadSetting",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 592,
                            "Name": "官網導下載短訊開關",
                            "Url": "https://sms.qa1.hk.91dev.tw/OfficialShop/AppLinkSMSSetting",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 794,
                            "Name": "網址重新導向",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/WebsiteRedirection",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 143,
                    "Name": "商品管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 823,
                            "Name": "新增商品頁規格表",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/SalePageSpecChartCreate",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 2013
                        },
                        {
                            "Id": 872,
                            "Name": "商品特色標語顯示設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/SalePageMetaFieldsSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 387,
                            "Name": "商品頁管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/SalePage/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 498,
                            "Name": "商品頁付款 / 配送方式維護",
                            "Url": "https://sms.qa1.hk.91dev.tw/SalePage/SalePageDeliverTypeAndPayTypeList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 387
                        },
                        {
                            "Id": 388,
                            "Name": "新增商品頁",
                            "Url": "https://sms.qa1.hk.91dev.tw/SalePage/Edit",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 497,
                            "Name": "批次新增修改商品",
                            "Url": "https://sms.qa1.hk.91dev.tw/Sidebar/BatchModifySalePage",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 529,
                            "Name": "批次修改商品選項與庫存",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchUploadUpdateSaleProductSKU",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 497
                        },
                        {
                            "Id": 528,
                            "Name": "批次修改商品頁",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchUploadUpdateSalePage",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 497
                        },
                        {
                            "Id": 530,
                            "Name": "批次刪除商品頁",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchRemoveSalePage",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 497
                        },
                        {
                            "Id": 638,
                            "Name": "批次匯入商品語系資料",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchUpdateSalePageML",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 497
                        },
                        {
                            "Id": 637,
                            "Name": "批次匯入商品選項語系資料",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchUpdateSKUMultilingual",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 497
                        },
                        {
                            "Id": 526,
                            "Name": "批次新增商品頁 (系統格式)",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchUploadSalePage",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 497
                        },
                        {
                            "Id": 640,
                            "Name": "批次預約商品資訊調整",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchReserveSaleProductSKUAdjustment",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 497
                        },
                        {
                            "Id": 641,
                            "Name": "商品預約調整進度查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/SaleProduct/SaleProductSKUAdjustmentPlanList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 497
                        },
                        {
                            "Id": 391,
                            "Name": "庫存管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/Stock/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 392,
                            "Name": "贈品管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/SaleProduct/SaleProductGiftList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 505,
                            "Name": "新增贈品",
                            "Url": "https://sms.qa1.hk.91dev.tw/SaleProduct/EditGift",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 392
                        },
                        {
                            "Id": 740,
                            "Name": "商品管理設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/SystemSettings/ProductManagementSettings",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 659,
                            "Name": "商品不可退貨功能啟用",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/SaleProductReturnableSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 740
                        },
                        {
                            "Id": 504,
                            "Name": "庫存預設值與通知設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Stock/SafetyStockDefault",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 740
                        },
                        {
                            "Id": 586,
                            "Name": "新增商品頁預設值設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/SalePageDefault",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 740
                        },
                        {
                            "Id": 775,
                            "Name": "商品頁群組管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/SalePageGroup/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 776,
                            "Name": "商品頁群組新增",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/SalePageGroup/Create",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 822,
                            "Name": "商品頁規格表管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/SalePageSpecChartList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 824,
                            "Name": "批次管理商品頁規格表適用商品",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/BatchUploadSalePageSpecChart",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 2013
                        },
                        {
                            "Id": 777,
                            "Name": "商品頁群組編輯",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/SalePageGroup/Edit",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 841,
                            "Name": "商品特色標語管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ShopMetafield",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 780,
                            "Name": "批次管理商品頁群組",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/BatchUploadSalePageGroup",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 122,
                    "Name": "商品分類與標籤",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 748,
                            "Name": "新增商品品牌",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/BrandCreate",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 122
                        },
                        {
                            "Id": 752,
                            "Name": "分類推薦品牌管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/category_recommendation_brand_management",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 739
                        },
                        {
                            "Id": 772,
                            "Name": "新增商品角標",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ProductBadgeCreate",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 771
                        },
                        {
                            "Id": 773,
                            "Name": "編輯商品角標",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ProductBadgeEdit",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 771
                        },
                        {
                            "Id": 737,
                            "Name": "批次管理品牌",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/BatchUploadBrand",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 122
                        },
                        {
                            "Id": 739,
                            "Name": "商店分類管理(新)",
                            "Url": "https://sms.qa1.hk.91dev.tw/MultiShopCategory/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 501,
                            "Name": "商店分類管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopCategory/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 389,
                            "Name": "商品分類設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopCategorySalePage/ShopCategorySetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 500,
                            "Name": "批次修改商品分類",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopCategorySalePage/BatchUpdateShopCategorySetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 389
                        },
                        {
                            "Id": 499,
                            "Name": "上傳修改分類檔案",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchUpdateShopCategorySalePage",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 389
                        },
                        {
                            "Id": 390,
                            "Name": "分類頁店長推薦排序",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopCategorySalePage/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 503,
                            "Name": "批次設定分類頁店長推薦排序",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopCategorySalePage/Create",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 390
                        },
                        {
                            "Id": 736,
                            "Name": "商品品牌管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/BrandList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 122
                        },
                        {
                            "Id": 755,
                            "Name": "品牌頁店長推薦排序",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/BatchUploadBrandCuratorSort",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 736
                        },
                        {
                            "Id": 687,
                            "Name": "商品標籤管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/TagSystem/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 691,
                            "Name": "批次修改商品標籤語系",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/Batch/ImportSalePageTagsTranslation",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 687
                        },
                        {
                            "Id": 692,
                            "Name": "批次新增商品標籤",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/Batch/BatchCreateSalePageTags",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 687
                        },
                        {
                            "Id": 688,
                            "Name": "批次套用商品標籤",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/Batch/BatchApplySalePageTags",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 687
                        },
                        {
                            "Id": 572,
                            "Name": "熱銷排行榜設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/HotSaleRankingSetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 771,
                            "Name": "商品角標設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ProductBadgeList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 144,
                    "Name": "購買流程設定",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 749,
                            "Name": "購物車功能設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/ShoppingFlowSettings",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 144
                        },
                        {
                            "Id": 718,
                            "Name": "自訂購買須知",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/PurchaseNotesSetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 145,
                    "Name": "金物流管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 741,
                            "Name": "金物流服務申請",
                            "Url": "https://sms.qa1.hk.91dev.tw/SystemSettings/PaymentFlowAndLogisticsServicesApplication",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 646,
                            "Name": "第三方金物流資料設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ThirdPartyServices/Settings",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 741
                        },
                        {
                            "Id": 742,
                            "Name": "金物流功能設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/SystemSettings/PaymentFlowAndLogisticsFunctionSettings",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 573,
                            "Name": "付款 / 配送設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Shop/PaymentShipping",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 742
                        },
                        {
                            "Id": 653,
                            "Name": "新增國內配送方式",
                            "Url": "https://sms.qa1.hk.91dev.tw/Shop/DomesticShipping",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 742
                        },
                        {
                            "Id": 549,
                            "Name": "新增國家地區配送",
                            "Url": "https://sms.qa1.hk.91dev.tw/Shop/OverseasShipping",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 742
                        },
                        {
                            "Id": 574,
                            "Name": "國家/地區配送基本設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Shop/OverseasSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 742
                        },
                        {
                            "Id": 579,
                            "Name": "門市自取設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationPickup/Setting",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 742
                        },
                        {
                            "Id": 835,
                            "Name": "購物金設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ShopStoreCreditProfile",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 742
                        }
                    ]
                },
                {
                    "Id": 146,
                    "Name": "宅配訂單管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 862,
                            "Name": "宅配 - 調貨單出貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/HomeDeliveryDispatchOrderShipping",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 438
                        },
                        {
                            "Id": 863,
                            "Name": "宅配 - 調貨單出貨 - 列印出貨明細",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/HomeDeliveryDispatchOrderShippingReport",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 862
                        },
                        {
                            "Id": 437,
                            "Name": "宅配 - 1. 訂單確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/HomeDelivery/ConfirmList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 480,
                            "Name": "批次訂單確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchHomeDeliveryConfirm",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 437
                        },
                        {
                            "Id": 438,
                            "Name": "宅配 - 2. 訂單出貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/HomeDelivery/ShippingList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 481,
                            "Name": "宅配批次出貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchHomeDelivery",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 438
                        },
                        {
                            "Id": 482,
                            "Name": "宅配批次列印出貨明細",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchPrintHomeShippingOrder",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 438
                        }
                    ]
                },
                {
                    "Id": 172,
                    "Name": "第三方物流商訂單管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 611,
                            "Name": "批次訂單配號",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchDeliveryAllocateCode",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 608
                        },
                        {
                            "Id": 608,
                            "Name": "訂單配號",
                            "Url": "https://sms.qa1.hk.91dev.tw/Delivery/AllocateCodeList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 609,
                            "Name": "列印託運單/出貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/Delivery/ShipConfirmList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 610,
                            "Name": "異常處理",
                            "Url": "https://sms.qa1.hk.91dev.tw/Delivery/ShippingAbnormalList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 612,
                            "Name": "批次出貨確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchDeliveryShipConfirm",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 152,
                    "Name": "門市自取訂單管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 460,
                            "Name": "門市自取 - 1. 訂單配號",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationPickup/AllocateCodeList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 492,
                            "Name": "門市自取批次配號",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchLocationPickupAllocateCode",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 460
                        },
                        {
                            "Id": 461,
                            "Name": "門市自取 - 2. 標籤列印/出貨確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationPickup/ShippingDataList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 493,
                            "Name": "門市自取批次出貨確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchLocationPickupShipConfirm",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 461
                        },
                        {
                            "Id": 462,
                            "Name": "門市自取 - 3. 貨到門市確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationPickup/ShippingArrivedList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 494,
                            "Name": "門市自取批次貨到門市確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchLocationPickupArrivedConfirm",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 462
                        },
                        {
                            "Id": 463,
                            "Name": "門市自取 - 4. 消費者取貨確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationPickup/ShippingPickupList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 495,
                            "Name": "門市自取批次消費者取貨確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchLocationPickupPickupConfirm",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 463
                        },
                        {
                            "Id": 464,
                            "Name": "門市自取 - 5. 消費者未取貨確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationPickup/ShippingPickupCancelList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 153,
                    "Name": "國家/地區宅配訂單管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 465,
                            "Name": "國家/地區宅配 - 1. 訂單確認/出貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/SalesOrder/Oversea/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 496,
                            "Name": "國家/地區訂單批次出貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchOverseaDelivery",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 465
                        },
                        {
                            "Id": 645,
                            "Name": "國家/地區配送批次訂單確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchOverseaDeliveryConfirm",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 465
                        }
                    ]
                },
                {
                    "Id": 174,
                    "Name": "數位發送訂單管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 747,
                            "Name": "數位發送訂單確認與出貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/DigitalDeliveryList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 750,
                            "Name": "數位發送批次訂單確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/DigitalDeliveryConfirmList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 747
                        },
                        {
                            "Id": 751,
                            "Name": "數位發送批次出貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/DigitalDeliveryShippingList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 747
                        }
                    ]
                },
                {
                    "Id": 155,
                    "Name": "取消訂單管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 467,
                            "Name": "新增取消單",
                            "Url": "https://sms.qa1.hk.91dev.tw/CancelOrder/ApplyCancelRequest",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 468,
                            "Name": "取消單查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/CancelOrder/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 156,
                    "Name": "退貨訂單管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 469,
                            "Name": "退貨處理",
                            "Url": "https://sms.qa1.hk.91dev.tw/ReturnGoodsOrder/ReturnGoodsOrderProcess",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 470,
                            "Name": "退貨單查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/ReturnGoodsOrder/ReturnGoodsOrderList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 471,
                            "Name": "批次退貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchReturnGoodsOrder",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 472,
                            "Name": "補收單查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/RechargeReceipt/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 157,
                    "Name": "換貨訂單管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 473,
                            "Name": "換貨處理",
                            "Url": "https://sms.qa1.hk.91dev.tw/ChangeGoodsOrder/Process",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 474,
                            "Name": "換貨單查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/ChangeGoodsOrder/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 158,
                    "Name": "訂單設定與管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 807,
                            "Name": "Invoice 說明設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/Shop/GlobalInvoiceSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 158
                        },
                        {
                            "Id": 833,
                            "Name": "訂購備註",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ShopTradesOrderMemoSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 821,
                            "Name": "所有訂單確認",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/SalesOrderResultConfirmList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 743,
                            "Name": "訂單預設值設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/SystemSettings/OrderDefaultSettings",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 650,
                            "Name": "訂單查詢顯示設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/OrderDisplaySetting",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 743
                        },
                        {
                            "Id": 583,
                            "Name": "出貨說明設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Shop/ShippingNote",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 743
                        },
                        {
                            "Id": 744,
                            "Name": "自訂取消/退/換貨設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/SystemSettings/CustomizeCancellationAndReturnAndExchangeSettings",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 580,
                            "Name": "部份取消功能開關",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/TogglePartialCancel",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 744
                        },
                        {
                            "Id": 593,
                            "Name": "自訂取消原因",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopCustomDefinition/CancelReason",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 744
                        },
                        {
                            "Id": 702,
                            "Name": "退貨開關",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/ToggleReturnGoods",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 744
                        },
                        {
                            "Id": 582,
                            "Name": "部份退貨功能開關",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/TogglePartialReturn",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 744
                        },
                        {
                            "Id": 595,
                            "Name": "自訂退貨原因",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopCustomDefinition/ReturnReason",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 744
                        },
                        {
                            "Id": 578,
                            "Name": "自訂退貨方式",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/CvsReturnSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 744
                        },
                        {
                            "Id": 581,
                            "Name": "換貨功能開關",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/ToggleChangeGoods",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 744
                        },
                        {
                            "Id": 594,
                            "Name": "自訂換貨原因",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopCustomDefinition/ChangeReason",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 744
                        }
                    ]
                },
                {
                    "Id": 159,
                    "Name": "促購活動",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 769,
                            "Name": "檢視折扣活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/PromotionEngine/View",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 406
                        },
                        {
                            "Id": 520,
                            "Name": "新增折扣活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/Promotion/NewForm",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 406
                        },
                        {
                            "Id": 765,
                            "Name": "新增折扣活動（指定商品活動、現折活動、贈品活動、買X享Y免費）",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/PromotionEngine/Create",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 406
                        },
                        {
                            "Id": 406,
                            "Name": "折扣活動設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Promotion/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 767,
                            "Name": "編輯折扣活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/PromotionEngine/Edit",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 406
                        },
                        {
                            "Id": 768,
                            "Name": "複製折扣活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/PromotionEngine/Clone",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 406
                        },
                        {
                            "Id": 694,
                            "Name": "批次更新折扣活動商品",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/Batch/BatchModifyPromotionSalePage",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 406
                        },
                        {
                            "Id": 693,
                            "Name": "信用卡 BIN 碼管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/Promotion/BinCodeList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 406
                        },
                        {
                            "Id": 635,
                            "Name": "折扣活動商品查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/Promotion/ActivitySalePageList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 859,
                            "Name": "回饋活動設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/PromotionRewardList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 408,
                            "Name": "購物車活動設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/PurchaseExtra/Index",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 643,
                            "Name": "國家/地區運費活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/shopshippingtypepromotion/list",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 644,
                            "Name": "新增國家/地區運費活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/shopshippingtypepromotion/Form",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 643
                        }
                    ]
                },
                {
                    "Id": 178,
                    "Name": "限購設定",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 407,
                            "Name": "商品活動設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Promotion/ActivityList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 828,
                            "Name": "購買資格設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/PurchaseQualificationList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 160,
                    "Name": "分析洞察",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 711,
                            "Name": "商品報表",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/analytics/productAnalytics",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 551,
                            "Name": "即時商品銷售排行榜",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/SaleRankingRealTimeList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 844,
                            "Name": "組合商品銷售統計表",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/SalepageBundleSalesStatisticsList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 690,
                            "Name": "折扣活動成效報表",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/promotion/promotionEffect",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 422,
                            "Name": "商店配送方式統計表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/OrderShippingTypeStatisitc",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 734,
                            "Name": "商店出貨分析報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Sidebar/StoreShipmentAnalysisReport",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 421,
                            "Name": "商店出貨狀態分析",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/ShopShippingList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 734
                        },
                        {
                            "Id": 563,
                            "Name": "門市自取統計報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/LocationPickupList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 734
                        },
                        {
                            "Id": 717,
                            "Name": "商店銷售統計表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/SalesStatisticsDataByDateList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 420,
                            "Name": "訂單相關報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Sidebar/OrderReport",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 871,
                            "Name": "回饋活動報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/PromotionRewardStatistics",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 557,
                            "Name": "即時訂單統計表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/SupplierRealTimeTrackOrderList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 420
                        },
                        {
                            "Id": 558,
                            "Name": "取消訂單統計表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/CancelOrderStatisticsList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 420
                        },
                        {
                            "Id": 556,
                            "Name": "訂單來源分析",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/OrderSourceList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 420
                        }
                    ]
                },
                {
                    "Id": 161,
                    "Name": "線上客服管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 855,
                            "Name": "即時客服設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ChatBotSetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 431,
                            "Name": "客服管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/Question/QuestionList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 633,
                            "Name": "代客修改配送資訊",
                            "Url": "https://sms.qa1.hk.91dev.tw/SalesOrderReceiver/ModifySalesOrderReceiverPorcess",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 607,
                            "Name": "代客申請退貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/ReturnGoodsOrder/ReturnGoodsRequestProcess",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 840,
                            "Name": "代客申請換貨",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ChangeGoodsRequestProcess",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 432,
                            "Name": "消費者訂單查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/Overview/OrderSlaveFlowDataList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 433,
                            "Name": "運費退款單設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/RefundRequest/SalesOrderFeeRefundList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 544,
                            "Name": "新增運費退款單",
                            "Url": "https://sms.qa1.hk.91dev.tw/RefundRequest/ReturnSalesOrderFee",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 433
                        },
                        {
                            "Id": 434,
                            "Name": "開放退貨申請",
                            "Url": "https://sms.qa1.hk.91dev.tw/ReturnGoodsOrder/ApplyReturn",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 435,
                            "Name": "交易黑名單管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/Blacklist/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 545,
                            "Name": "新增黑名單",
                            "Url": "https://sms.qa1.hk.91dev.tw/Blacklist/Edit",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 435
                        },
                        {
                            "Id": 546,
                            "Name": "批次匯入黑名單",
                            "Url": "https://sms.qa1.hk.91dev.tw/Blacklist/BatchCreateBlackList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 435
                        },
                        {
                            "Id": 547,
                            "Name": "黑名單條件設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Blacklist/Setting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 435
                        }
                    ]
                },
                {
                    "Id": 180,
                    "Name": "客服幫手",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 851,
                            "Name": "即時對話",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ChatRoomList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 878,
                            "Name": "對話查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ChatSearch",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 877,
                            "Name": "對話標籤管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ChatTagGroupSetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 856,
                            "Name": "客服人員管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/AgentManagement",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 857,
                            "Name": "自動回覆管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/AutoReplyManagement",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 858,
                            "Name": "自動回覆設定歷程",
                            "Url": "https://sms.qa1.hk.91dev.tw/replysetting_history_module",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 867,
                            "Name": "AI 客服設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/AIAgentSetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 868,
                            "Name": "AI 客服設定歷程",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/AIAgentSettingHistory",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 869,
                            "Name": "客服幫手設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ChatSetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 870,
                            "Name": "客服幫手設定歷程",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ChatSettingHistory",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 873,
                            "Name": "客服回應表現",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ChatReport",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 162,
                    "Name": "優惠券",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 728,
                            "Name": "優惠券設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ECoupon/List",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 729,
                            "Name": "新增優惠券",
                            "Url": "https://sms.qa1.hk.91dev.tw/ECoupon/ECouponForm",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 728
                        },
                        {
                            "Id": 533,
                            "Name": "折價券折抵上限預設值設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ECoupon/DefaultMaxDiscountLimit",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 728
                        },
                        {
                            "Id": 532,
                            "Name": "設定不適用任何折價券商品",
                            "Url": "https://sms.qa1.hk.91dev.tw/SalePage/ExcludeEcouponList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 728
                        },
                        {
                            "Id": 654,
                            "Name": "批次更新折價券適用商品",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchUpdateECouponPromotionTag",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 728
                        },
                        {
                            "Id": 619,
                            "Name": "贈品券退還設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/ToggleReturnCoupon",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 728
                        },
                        {
                            "Id": 732,
                            "Name": "門市核銷優惠券設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ECoupon/ECouponAuditSettings",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 728
                        },
                        {
                            "Id": 676,
                            "Name": "門市券設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ECoupon/StoreECouponList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 677,
                            "Name": "新增門市券",
                            "Url": "https://sms.qa1.hk.91dev.tw/ECoupon/StoreECouponEdit",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 676
                        },
                        {
                            "Id": 424,
                            "Name": "優惠券報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Sidebar/ECouponReport",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 559,
                            "Name": "優惠券即時報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/ECouponImmediateList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 424
                        },
                        {
                            "Id": 560,
                            "Name": "優惠券績效報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/ECouponPerformanceList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 424
                        },
                        {
                            "Id": 647,
                            "Name": "贈品券指定兌換門市報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/GiftECouponLocationReport",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 424
                        },
                        {
                            "Id": 561,
                            "Name": "門市券領取 / 使用統計報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/ShopCouponList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 424
                        },
                        {
                            "Id": 778,
                            "Name": "優惠券相關功能設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/Coupon/Settings",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 728
                        }
                    ]
                },
                {
                    "Id": 163,
                    "Name": "會員積分",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 826,
                            "Name": "點數折抵排除商品設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/RedeemPointExcludeCondition",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 827,
                            "Name": "點數折抵排除商品歷程記錄",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/RedeemPointExcludeConditionHistory",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 834,
                            "Name": "批次更新活動商品",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/BatchUploadSalePage",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 409,
                            "Name": "會員積分設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/LoyaltyPoint/ProfileSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 674,
                            "Name": "積分活動設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Promotion/RewardPointlist",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 506,
                            "Name": "批次異動紅利點數",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchModifyLoyaltyPoint",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 733,
                            "Name": "會員積分報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Sidebar/LoyaltyPointReport",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 745,
                            "Name": "積分活動績效報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/PromotionRewardPointStatistics",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 733
                        },
                        {
                            "Id": 423,
                            "Name": "會員積分統計報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/LoyaltyPointList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 733
                        }
                    ]
                },
                {
                    "Id": 164,
                    "Name": "會員邀請活動",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 695,
                            "Name": "會員邀請活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/Promotion/PromoCodeMGOList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 696,
                            "Name": "新增會員邀請活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/Promotion/PromoCodeMGOForm",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 695
                        },
                        {
                            "Id": 701,
                            "Name": "會員邀請活動設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/PromoCodeMGOSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 695
                        }
                    ]
                },
                {
                    "Id": 165,
                    "Name": "優惠碼活動",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 678,
                            "Name": "優惠碼活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/Promotion/PromoCodeKOLList",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 679,
                            "Name": "新增優惠碼活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/Promotion/PromoCodeKOLForm",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 678
                        },
                        {
                            "Id": 686,
                            "Name": "優惠碼付款報表設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/PromoCodePaymentExportSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 166,
                    "Name": "商品評價管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 706,
                            "Name": "商品評價管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/SalePageComment/Index",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 709,
                            "Name": "商品評價設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/ToggleCommentSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 181,
                    "Name": "直播活動",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 875,
                            "Name": "直播活動管理",
                            "Url": "https://91app-live.qa1.hk.91dev.tw/live-events",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 181
                        },
                        {
                            "Id": 876,
                            "Name": "直播平台設定",
                            "Url": "https://91app-live.qa1.hk.91dev.tw/stream-settings",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 181
                        }
                    ]
                },
                {
                    "Id": 177,
                    "Name": "團購活動",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 774,
                            "Name": "團購活動",
                            "Url": "https://sms.qa1.hk.91dev.tw/CommerceCloud/ReferrerPromotion",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 167,
                    "Name": "定期購",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 731,
                            "Name": "定期購管理",
                            "Url": "https://regularpurchase-manager.qa1.hk.91dev.tw/admin/controllers/regular/product.php",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 466,
                            "Name": "定期購訂單",
                            "Url": "https://sms.qa1.hk.91dev.tw/RegularOrder/List",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 731
                        }
                    ]
                }
            ]
        },
        {
            "CloudType": "Marketing",
            "Modules": [
                {
                    "Id": 110,
                    "Name": "用戶分群",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 710,
                            "Name": "購買意圖智慧預測",
                            "Url": "https://sms.qa1.hk.91dev.tw/Audience/Query#/dciu",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 784,
                            "Name": "CDMP 名單上傳",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/mkt-contract/cdmp/cdmp-audience-upload",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 770,
                            "Name": "名單圈選",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/audience/query",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 399,
                            "Name": "名單管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/Audience/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 121,
                    "Name": "客戶關係管理(CRM)",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 398,
                            "Name": "會員機制",
                            "Url": "https://sms.qa1.hk.91dev.tw/CrmMemberTier/Settings",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 396,
                            "Name": "會員禮",
                            "Url": "https://sms.qa1.hk.91dev.tw/VipMemberPresent/List",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 508,
                            "Name": "新增生日禮",
                            "Url": "https://sms.qa1.hk.91dev.tw/VipMemberPresent/Create",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 396
                        },
                        {
                            "Id": 509,
                            "Name": "新增升等禮",
                            "Url": "https://sms.qa1.hk.91dev.tw/VipMemberPresent/CreateLevelUpgradedPresent",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 396
                        },
                        {
                            "Id": 511,
                            "Name": "新增續等禮",
                            "Url": "https://sms.qa1.hk.91dev.tw/VipMemberPresent/CreateLevelRenewPresent",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 396
                        },
                        {
                            "Id": 510,
                            "Name": "新增開卡禮",
                            "Url": "https://sms.qa1.hk.91dev.tw/VipMemberPresent/CreateOpenCardPresent",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 396
                        },
                        {
                            "Id": 397,
                            "Name": "會員首下載禮設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/AppFirstDownload/List",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 512,
                            "Name": "新增首下載禮",
                            "Url": "https://sms.qa1.hk.91dev.tw/AppFirstDownload/Create",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 397
                        },
                        {
                            "Id": 738,
                            "Name": "會員禮管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/Marketing/VipMemberPresent",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 111,
                    "Name": "行銷操作",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 513,
                            "Name": "新增訊息推播",
                            "Url": "https://sms.qa1.hk.91dev.tw/NotificationProfile/Edit",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 401
                        },
                        {
                            "Id": 874,
                            "Name": "智慧通知",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/action/smartchannel",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 642,
                            "Name": "名單操作管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/Audience/Coupon",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 400,
                            "Name": "名單通知管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/Notification/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 401,
                            "Name": "App 訊息推播設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/NotificationProfile/List",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 723,
                            "Name": "個人化行銷",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/action/personalizedMarketing",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 112,
                    "Name": "分析洞察",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 825,
                            "Name": "行銷通知成效報表",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/insight/notificationPerformance",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 853,
                            "Name": "智慧推薦報表",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/insight/intelligentRecommendation",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 671,
                            "Name": "導流歸因報表",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/campaign/linkEffect",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 673,
                            "Name": "名單成效報表",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/audience/audienceProfileReport",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 425,
                            "Name": "APP開啟統計數",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/AppOpenList",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 113,
                    "Name": "流量成長",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 785,
                            "Name": "Facebook 相關功能設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Facebook/BusinessExtensionManagement",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 120
                        },
                        {
                            "Id": 669,
                            "Name": "導流連結管理",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/campaign/linkManagement",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 670,
                            "Name": "導流連結樣板管理",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/campaign/templateManagement",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 652,
                            "Name": "智慧網址 Deeplink",
                            "Url": "https://sms.qa1.hk.91dev.tw/Deeplink/DeeplinkGenerator",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 587,
                            "Name": "自動導流網址設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/UrlShortener/AutoRedirectUrlGenerator",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 589,
                            "Name": "廣告追蹤碼設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/TrackingCode/List",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 588,
                            "Name": "產品目錄",
                            "Url": "https://sms.qa1.hk.91dev.tw/ProductFeed/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 591,
                            "Name": "Facebook Messenger設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Facebook/FBMessengerManagement",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 700,
                            "Name": "Behavior API",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/customerData/behaviorApi",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 845,
                            "Name": "電郵使用量及方案",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/growth/email-quota-usage-history",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 179,
                    "Name": "夥伴市集",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 847,
                            "Name": "夥伴市集",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/growth/partnerMarket",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                }
            ]
        },
        {
            "CloudType": "OMNI",
            "Modules": [
                {
                    "Id": 123,
                    "Name": "批次作業進度查詢",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 386,
                            "Name": "批次作業進度查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 698,
                            "Name": "批次預約作業進度查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/ScheduledBatchUploads",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 699,
                            "Name": "會員聯繫自訂名單分配試算",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/PreviewCallingListAllocation",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 386
                        }
                    ]
                },
                {
                    "Id": 124,
                    "Name": "全通路會員管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 599,
                            "Name": "會員查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/CrmMember/List",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 393,
                            "Name": "線上會員管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMember/List",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 713,
                            "Name": "編輯會員資料",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMaintain/EditOSM",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 393
                        },
                        {
                            "Id": 715,
                            "Name": "編輯會員資料",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMaintain/EditCRM",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 599
                        },
                        {
                            "Id": 781,
                            "Name": "客群管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMNI/MemberCollectionList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 720,
                            "Name": "刪除會員資料 (單筆)",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMaintain/DeleteSingleOSM",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 393
                        },
                        {
                            "Id": 722,
                            "Name": "刪除會員資料 (單筆)",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMaintain/DeleteSingleCRM",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 599
                        },
                        {
                            "Id": 507,
                            "Name": "會員資料匯出功能設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Supplier/MemberInfoExportSetting",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 393
                        },
                        {
                            "Id": 630,
                            "Name": "手機號碼修改及帳號合併",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMerge/List",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 393
                        },
                        {
                            "Id": 631,
                            "Name": "手機號碼修改及帳號合併歷程",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMerge/History",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 393
                        },
                        {
                            "Id": 846,
                            "Name": "退貨降等會員名單",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMNI/ExportDowngradeMember",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 393
                        },
                        {
                            "Id": 719,
                            "Name": "刪除會員資料 (批次) ",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMaintain/DeleteBatchOSM",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 393
                        },
                        {
                            "Id": 721,
                            "Name": "刪除會員資料 (批次) ",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMaintain/DeleteBatchCRM",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 599
                        },
                        {
                            "Id": 648,
                            "Name": "會員資料匯入",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchCrmMemberImport/Index",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 600,
                            "Name": "會員匯出進度查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/CrmMember/ExportList",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 603,
                            "Name": "批次異動會員等級",
                            "Url": "https://sms.qa1.hk.91dev.tw/CrmTargetMemberGroup/List",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 601,
                            "Name": "會員行銷活動追蹤",
                            "Url": "https://sms.qa1.hk.91dev.tw/CrmCampaign/List",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 602,
                            "Name": "會員行銷活動報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/CrmReport/CampaignStatistics",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 604,
                            "Name": "刪除會員資料 (單筆)",
                            "Url": "https://sms.qa1.hk.91dev.tw/CrmNotificationProfile/List",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 599
                        },
                        {
                            "Id": 394,
                            "Name": "會員填寫資料設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/VipShopMember/Settings",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 395,
                            "Name": "國外會員設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopDefault/OverseaMemberSetting",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 842,
                            "Name": "會員登入註冊設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMNI/SocialLoginList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 714,
                            "Name": "編輯會員通知設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMaintain/EditNotificationOSM",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 393
                        },
                        {
                            "Id": 716,
                            "Name": "編輯會員通知設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMaintain/EditNotificationCRM",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 599
                        },
                        {
                            "Id": 632,
                            "Name": "手機號碼修改及帳號合併紀錄",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopMemberMerge/HistoryDetail",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 393
                        }
                    ]
                },
                {
                    "Id": 125,
                    "Name": "全通路訂單管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 436,
                            "Name": "所有線上訂單查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/OverView/SalesOrderResultList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 703,
                            "Name": "批次匯出資料",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchSalesOrderExport",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 436
                        },
                        {
                            "Id": 639,
                            "Name": "匯出訂單收據功能",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/BatchInvoiceExport",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 436
                        },
                        {
                            "Id": 649,
                            "Name": "其他交易資料匯入",
                            "Url": "https://sms.qa1.hk.91dev.tw/ImportCrmOrder/Index",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 126,
                    "Name": "公司資訊與方案",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 565,
                            "Name": "服務方案 (合約專區)",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopContract/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 566,
                            "Name": "基本資料 / 聯絡資料",
                            "Url": "https://sms.qa1.hk.91dev.tw/Supplier/InfoSetting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 127,
                    "Name": "帳號權限設定",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 479,
                            "Name": "使用者權限",
                            "Url": "https://sms.qa1.hk.91dev.tw/UserPermission/UserRoleMappingList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 548,
                            "Name": "新增群組",
                            "Url": "https://sms.qa1.hk.91dev.tw/UserPermission/RolePermission",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 479
                        },
                        {
                            "Id": 477,
                            "Name": "變更密碼",
                            "Url": "https://sms.qa1.hk.91dev.tw/Account/ChangePassword",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 478,
                            "Name": "兩步驟驗證",
                            "Url": "https://sms.qa1.hk.91dev.tw/TwoFactorAuthentication/Bind",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 861,
                            "Name": "存取驗證",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMNI/SecurityVerificationSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 479
                        }
                    ]
                },
                {
                    "Id": 128,
                    "Name": "帳務管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 426,
                            "Name": "短訊對帳報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/ShortMessageList",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 697,
                            "Name": "指定付款滿額現折對帳報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/DesignatePaymentPromotionAccounting",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 613,
                            "Name": "退款單查詢",
                            "Url": "https://sms.qa1.hk.91dev.tw/RefundRequest/List",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 684,
                            "Name": "繳費單管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/orders/payments",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 712,
                            "Name": "第三方帳務核對",
                            "Url": "https://sms.qa1.hk.91dev.tw/ExpenseOrder/ExpenseOrderReportV2",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 685,
                            "Name": "批次繳費單核銷",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/Batch/BatchWriteOffOrderPayment",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 684
                        },
                        {
                            "Id": 783,
                            "Name": "行銷通知對帳報表",
                            "Url": "https://analytics.qa1.hk.91dev.tw/#/insight/notificationBilling",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 176,
                    "Name": "多語系內容管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 753,
                            "Name": "內容管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 754,
                            "Name": "折扣活動翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/PromotionEngine",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 759,
                            "Name": "積分活動翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/PromotionEngineRewardPoint",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 760,
                            "Name": "會員邀請活動翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/PromoCodeMGO",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 761,
                            "Name": "金流行銷廣告翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/AdvertiseTag",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 762,
                            "Name": "購物車活動翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/PurchaseExtra",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 779,
                            "Name": "商品頁群組翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/SalePageGroup",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 786,
                            "Name": "優惠券相關功能設定-自訂券名稱設定翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ECouponSettingECouponCustom",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 787,
                            "Name": "優惠券相關功能設定-篩選條件自訂翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ECouponSettingECouponCatalog",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 788,
                            "Name": "門市設定翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/Location",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 789,
                            "Name": "贈品管理翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/SaleProductGift",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 790,
                            "Name": "會員等級翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/CrmShopMemberCard",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 791,
                            "Name": "會員制度說明翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/CrmShopMemberCardSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 795,
                            "Name": "新版會員禮翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ShopMemberPresent",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 796,
                            "Name": "官網設定翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/OfficialShop",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 797,
                            "Name": "訂單備註翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/SalePageMemo",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 802,
                            "Name": "優惠券設定翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ECoupon",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 803,
                            "Name": "自訂取消原因翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/Cancel",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 804,
                            "Name": "自訂退貨原因翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ReturnGoods",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 805,
                            "Name": "自訂換貨原因翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ChangeGoods",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 806,
                            "Name": "門市自取設定翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/LocationPickup",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 808,
                            "Name": "紅利點數設定翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/LoyaltyPointProfile",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 810,
                            "Name": "商店簡介翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ShopSummary",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 811,
                            "Name": "購物說明翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ShoppingNotice",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 812,
                            "Name": "客服服務翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/CustomerService",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 813,
                            "Name": "商店公告翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ShopAnnouncement",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 814,
                            "Name": "商店名稱翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ShopName",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 815,
                            "Name": "會員卡名稱翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/VipShopMemberCard",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 816,
                            "Name": "會員權益聲明翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/Benefits",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 817,
                            "Name": "自訂欄位翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/VipShopMemberInfoRule",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 818,
                            "Name": "2C2P",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/TwoCTwoP",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 819,
                            "Name": "其他轉帳方式翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/CustomOfflinePayment",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 820,
                            "Name": "商店帳號登入名稱翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ThirdpartyAuthButtonContent",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 829,
                            "Name": "主題推薦設定圖文模組翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/InfoModuleArticle",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 830,
                            "Name": "主題推薦設定相簿模組翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/InfoModuleAlbum",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 831,
                            "Name": "主題推薦設定影音模組翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/InfoModuleVideo",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 832,
                            "Name": "商品頁規格表翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/SalePageSpecChart",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 753
                        },
                        {
                            "Id": 837,
                            "Name": "QFPay",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/QFPay",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 838,
                            "Name": "購物金說明翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/StoreCredit",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 839,
                            "Name": "自訂訂單備註",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/ShopTradesOrderMemoCustomSetting",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 843,
                            "Name": "特色標語翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/SalepageMetafields",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 849,
                            "Name": "門市資訊頁翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/SEOContentShopStoreList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 850,
                            "Name": "自訂包裝選項翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/CustomPackaging",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 860,
                            "Name": "回饋活動翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/PromotionReward",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 864,
                            "Name": "推薦人翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/Referrer",
                            "IsDisplay": false,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 866,
                            "Name": "官網導下載設定翻譯",
                            "Url": "https://sms.qa1.hk.91dev.tw/Translate/ContentManagement/AppDownloadSetting",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 176
                        }
                    ]
                }
            ]
        },
        {
            "CloudType": "OMO",
            "Modules": [
                {
                    "Id": 114,
                    "Name": "門市/店員設定與管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 521,
                            "Name": "門市設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Location/List",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 412
                        },
                        {
                            "Id": 522,
                            "Name": "店員設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Location/EmployeeList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 412
                        },
                        {
                            "Id": 534,
                            "Name": "新增門市",
                            "Url": "https://sms.qa1.hk.91dev.tw/Location/Create",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 521
                        },
                        {
                            "Id": 535,
                            "Name": "業績移轉歷程",
                            "Url": "https://sms.qa1.hk.91dev.tw/Location/SalesTransferLogList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 412
                        },
                        {
                            "Id": 543,
                            "Name": "新增 QR Code 立牌",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopStandList/Create",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 416
                        },
                        {
                            "Id": 848,
                            "Name": "門市資訊頁設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMO/SEOContentShopStoreList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 412,
                            "Name": "門市與店員設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Sidebar/LocationEmployee",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 416,
                            "Name": "QR Code 立牌產生器",
                            "Url": "https://sms.qa1.hk.91dev.tw/ShopStandList/List",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 792,
                            "Name": "新增門市群組",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMO/StoreGroupCreate",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 793,
                            "Name": "門市群組設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMO/StoreGroupList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 801,
                            "Name": "\n批次新增/刪除店員",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMO/BatchLocationEmployeeSetting",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 118,
                    "Name": "店員幫手",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 413,
                            "Name": "店員幫手設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/Sidebar/LocationWizard",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 523,
                            "Name": "登入帳號管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationWizard/List",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 413
                        },
                        {
                            "Id": 536,
                            "Name": "店員幫手使用門市設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationWizard/Enable",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 624,
                            "Name": "店員幫手報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Sidebar/LocationWizardReport",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 625,
                            "Name": "店員幫手紀錄報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/LocationWizardRecord",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 624
                        },
                        {
                            "Id": 622,
                            "Name": "門市帳號設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationWizard/LocationLoginManagement",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 413
                        },
                        {
                            "Id": 629,
                            "Name": "匯出門市訂單輸入明細",
                            "Url": "https://sms.qa1.hk.91dev.tw/BatchUpload/LocationOrderInputDetailExport",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 537,
                            "Name": "線上活動推廣",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationReferralLink/List",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 413
                        },
                        {
                            "Id": 626,
                            "Name": "門市服務績效報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/LocationWizardPerformance",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 624
                        },
                        {
                            "Id": 621,
                            "Name": "店員密碼設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationWizard/EmployeeLoginManagement",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 413
                        },
                        {
                            "Id": 627,
                            "Name": "集點活動報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/LocationWizardRewardPoint",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 624
                        },
                        {
                            "Id": 524,
                            "Name": "會員註記設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationWizard/Label",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 413
                        },
                        {
                            "Id": 525,
                            "Name": "品牌識別",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationWizard/BrandIdentity",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 413
                        },
                        {
                            "Id": 628,
                            "Name": "會員聯繫報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/LocationWizardTelemarketing",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 624
                        },
                        {
                            "Id": 540,
                            "Name": "店員幫手集點活動設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationRewardPoint/List",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 413
                        },
                        {
                            "Id": 623,
                            "Name": "店員幫手登入異動歷程",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationWizard/DeviceLoginHistory",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 413
                        },
                        {
                            "Id": 538,
                            "Name": "店員幫手功能開關",
                            "Url": "https://sms.qa1.hk.91dev.tw/LocationWizard/Setting",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 413
                        }
                    ]
                },
                {
                    "Id": 115,
                    "Name": "推薦人",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 725,
                            "Name": "推薦人修改記錄",
                            "Url": "https://sms.qa1.hk.91dev.tw/AppRefereeProfile/History",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 541,
                            "Name": "推薦人設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/AppRefereeProfile/Update",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 724,
                            "Name": "推薦人設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/AppRefereeProfile/Setting",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 414,
                            "Name": "推薦人管理",
                            "Url": "https://sms.qa1.hk.91dev.tw/AppRefereeProfile/List",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 116,
                    "Name": "分析洞察",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 763,
                            "Name": "OMO 成效報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMO/OMOReport",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 564,
                            "Name": "推薦人推廣報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/AppRefereePromotionList",
                            "IsDisplay": false,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 726,
                            "Name": "推薦人推廣報表",
                            "Url": "https://sms.qa1.hk.91dev.tw/Report/ReferrerPromotionListReport",
                            "IsDisplay": true,
                            "ShopId": [
                                2
                            ],
                            "ParentFunctionId": 0
                        }
                    ]
                },
                {
                    "Id": 182,
                    "Name": "門市庫存預留管理",
                    "Icon": null,
                    "Functions": [
                        {
                            "Id": 879,
                            "Name": "基本設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMO/StoreReservationBasicSettings",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        },
                        {
                            "Id": 880,
                            "Name": "門市預留與庫存設定",
                            "Url": "https://sms.qa1.hk.91dev.tw/OMO/StoreReservationSettings",
                            "IsDisplay": true,
                            "ShopId": [],
                            "ParentFunctionId": 0
                        }
                    ]
                }
            ]
        }
    ],
    "ErrorMessage": null,
    "TimeStamp": "2026-02-12T17:29:15.5466389+08:00"
}
```
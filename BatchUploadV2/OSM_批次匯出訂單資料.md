

 ## OSM_批次匯出訂單資料時，指定付款方式會匯出失敗


 csp_BatchExportSalesOrderData
 csp_BatchExportSalesOrderDataV2



SELECT DISTINCT 
    SalesOrderData_SalesOrderSlaveId
FROM #tmpSalesOrderDataResponse 
LEFT JOIN dbo.SalesOrderSlavePayProfileType WITH (NOLOCK) 
    ON SalesOrderSlavePayProfileType_SalesOrderSlaveId = SalesOrderData_SalesOrderSlaveId
    AND SalesOrderSlavePayProfileType_ValidFlag = 1
LEFT JOIN dbo.PayProfile WITH (NOLOCK) 
    ON PayProfile_TypeDef = SalesOrderSlavePayProfileType_PayProfileTypeDef 
    AND PayProfile_ValidFlag = 1
WHERE PayProfile_StatisticsTypeDef = @PayProfileStatisticsTypeDef;



SalesOrderSlavePayProfileType_PayProfileTypeDef

沒再用這張表啦!


HK PROD 也這樣呵呵


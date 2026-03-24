
點數回收的 job 執行時，在進行逆流程主邏輯前，會先篩選 detail 資料抓出 "要退的對應子單"，但因為這個篩選是 by TS Code，因此以線下訂單而言，TS_正 + TS_逆都被帶出來往後走，到真正的逆流程退點時，被檢查邏輯所阻擋，造成 "不該退逆向單" 的狀況

<br>

## 問題分析

- **篩選邏輯問題**: 使用 TS Code 進行篩選時，會同時抓到正向單和逆向單
- **線下訂單特性**: TS_正 + TS_逆 都會被篩選條件帶出
- **檢查邏輯阻擋**: 在逆流程退點時被後續檢查邏輯阻擋
- **錯誤結果**: 造成不該退逆向單的異常狀況

<br>

### 修正方案


修正為要對應到 `OriginalCrmSalesOrderSlaveId` 才是真正要退的子單

**相關 MR**: https://gitlab.91app.com/commerce-cloud/nine1.promotion/nine1.promotion.worker/-/merge_requests/817






# sync-translate-housekeeper — 邏輯說明

> PHP 檔案：`admin/daemon/sync_translate_housekeeper.php`
> 對應排程：每天台灣時間 22:00（`0 14 * * *`）
> pg_id：2

---

## 一句話摘要

每晚從 91APP API 抓取所有語系的翻譯 JSON，同步寫入 MySQL（`i18n_translate_record`）及 Redis，確保定期購服務的多語系顯示資料保持最新。

---

## 執行流程

```
main()
├── 取得需同步的語系清單（translate_main.get_translate_main_data_by_browse()）
├── 取得需同步的模組清單（Gb_Config::）
└── foreach lang × module
    ├── Api91::fetchTranslateJSON(project, module, lang) → 取得翻譯 JSON
    ├── 若有資料（count > 0）
    │   ├── i18n_translate_record.load_by_lang_and_module()
    │   │   ├── 存在 → UPDATE（itr_data, itr_update_count++）
    │   │   └── 不存在 → INSERT
    │   └── Redis.set(prefix_lang_module, json)
    └── 任何 exception → writeLog error，繼續下一筆
```

---

## Redis Key 格式

```
{REDIS_SERVER_PREFIX}_{lang}_{module}
例如：rps_zh_TW_regular-purchase
```

> `REDIS_SERVER_PREFIX` 由 `Gb_Config` 設定。

---

## 涉及資料表

| 資料表 | 操作 | 說明 |
|---|---|---|
| `translate_main` | SELECT | 取得啟用的語系清單 |
| `i18n_translate_record` | SELECT + INSERT/UPDATE | 翻譯資料主表 |

---

## 外部依賴

| 依賴 | 說明 |
|---|---|
| MySQL | 讀語系清單、寫翻譯資料 |
| **Redis** | 寫入翻譯快取（唯一使用 Redis 的 job） |
| **91APP API** | `fetchTranslateJSON` 取得翻譯 JSON |

---

## 與定期購的關係

**幾乎無關。** 這支是整個 PHP 系統的共用基礎設施，  
讓頁面文字、錯誤訊息可以多語系顯示，不影響定期購核心流程。  
翻寫優先級：**低**。

---

## 翻寫 .NET Core 注意事項

1. **是否需要移植？**：此 job 是為 PHP 的多語系機制設計的，.NET Core 若使用不同的 i18n 方案（如 resource file、`IStringLocalizer`），此 job **可能直接不需要**。需與產品/架構師確認。
2. **Redis 設定**：`REDIS_SERVER_WRITE`（寫入用）獨立設定，新版需確認 Redis 連線字串來源。
3. **TRANSLATE_MODULE_LIST 設定**：翻譯模組清單在 Gb_Config 中，新版需對應到 appsettings.json 或 DB 設定。
4. **錯誤處理**：單一語系/模組失敗不中斷整個 job，繼續處理下一筆，新版需保留此容錯設計。
5. **執行頻率**：每天 22:00 一次，新版若翻譯資料變動頻繁可考慮調整頻率。

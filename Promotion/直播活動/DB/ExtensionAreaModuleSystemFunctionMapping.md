```sql

USE AuthExternalDB

-- 875 直播活動管理 live_buy_events
-- 876 直播平台設定 live_buy_setting

SELECT ExtensionAreaModuleSystemFunctionMapping_LanguageKey,*
FROM ExtensionAreaModuleSystemFunctionMapping(NOLOCK)
WHERE ExtensionAreaModuleSystemFunctionMapping_ValidFlag = 1
--AND ExtensionAreaModuleSystemFunctionMapping_SystemFunctionId IN (876,875)
AND ExtensionAreaModuleSystemFunctionMapping_SystemFunctionId IN (876,875)

UPDATE ExtensionAreaModuleSystemFunctionMapping
SET ExtensionAreaModuleSystemFunctionMapping_LanguageKey = N'live_buy_setting'
WHERE ExtensionAreaModuleSystemFunctionMapping_ValidFlag = 1
--AND ExtensionAreaModuleSystemFunctionMapping_SystemFunctionId IN (876,875)
AND ExtensionAreaModuleSystemFunctionMapping_SystemFunctionId = 876

SELECT ExtensionAreaModuleSystemFunctionMapping_LanguageKey,*
FROM ExtensionAreaModuleSystemFunctionMapping(NOLOCK)
WHERE ExtensionAreaModuleSystemFunctionMapping_ValidFlag = 1
--AND ExtensionAreaModuleSystemFunctionMapping_SystemFunctionId IN (876,875)
AND ExtensionAreaModuleSystemFunctionMapping_SystemFunctionId IN (876,875)
```
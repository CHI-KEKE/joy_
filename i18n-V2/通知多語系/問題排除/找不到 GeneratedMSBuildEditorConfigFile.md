## Test 專案找不到 GeneratedMSBuildEditorConfigFile**

1. 開啟登錄編輯程式 : WIN + R => regedit
2. 導航至指定路徑 : HKEY_LOCAL_MACHINE => SYSTEM => CurrentControlSet => Control => FileSystem => LongPathsEnabled
3. 修改數值 : 右鍵 => Value 改成 1
4. 重新啟動電腦

這個問題通常是因為 Windows 系統對長路徑的限制導致，啟用長路徑支援可以解決此問題
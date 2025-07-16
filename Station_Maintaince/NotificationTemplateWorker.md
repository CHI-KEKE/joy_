# 📧 NotificationTemplateWorker 維護文件

> 📚 這是 NotificationTemplateWorker 系統的完整維護指南，包含本機開發問題排除和常見設定問題等重要資訊

<br>

## 📖 目錄

1. [🔧 本機開發問題排除](#-本機開發問題排除)
2. [☁️ AWS 設定問題](#-aws-設定問題)

<br>

---

## 🔧 本機開發問題排除

**問題 1：Test 專案找不到 GeneratedMSBuildEditorConfigFile**

<br>

**解決步驟**：

<br>

1. 開啟登錄編輯程式
   ```
   WIN + R => regedit
   ```

<br>

2. 導航至指定路徑
   ```
   HKEY_LOCAL_MACHINE => SYSTEM => CurrentControlSet => Control => FileSystem => LongPathsEnabled
   ```

<br>

3. 修改數值
   ```
   右鍵 => Value 改成 1
   ```

<br>

4. 重新啟動電腦

<br>

**說明**：這個問題通常是因為 Windows 系統對長路徑的限制導致，啟用長路徑支援可以解決此問題

<br>

---

## ☁️ AWS 設定問題

**問題 2：沒吃到 AWS 設定**

<br>

**解決方法**：

<br>

把正確的 AWS Key / Secret 複製過去

<br>

**操作步驟**：

<br>

1. 確認 AWS 認證資訊設定檔位置
2. 複製正確的 AWS Access Key ID
3. 複製正確的 AWS Secret Access Key
4. 更新本機設定檔或環境變數

<br>

**注意事項**：

<br>

確保 AWS 認證資訊具有適當的權限，並且格式正確無誤

<br>

---
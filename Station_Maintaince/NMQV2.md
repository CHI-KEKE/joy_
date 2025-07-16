# 🔄 NMQV2 文件

<br>

## 📖 目錄

- [🔄 NMQV2 文件](#-nmqv2-文件)
  - [📖 目錄](#-目錄)
  - [🔧 .NET Framework 安裝](#-net-framework-安裝)
  - [💻 IDE 問題排除](#-ide-問題排除)
  - [⚙️ NMQ 組件框架需求](#️-nmq-組件框架需求)

<br>

---

## 🔧 .NET Framework 安裝

**Microsoft .NET Framework 參考組件安裝**：

<br>

安裝網址：https://www.nuget.org/packages/microsoft.netframework.referenceassemblies.net45

<br>

**.NET Framework 4.5.2 Developer Pack 離線安裝器**：

<br>

參考文件：https://www.cnblogs.com/rqcim/p/15882239.html

<br>

這個安裝包提供了完整的開發環境支援，適用於離線環境的安裝需求

<br>

---

## 💻 IDE 問題排除

**Visual Studio 2022 相容性問題**：

<br>

如果在使用 Visual Studio 2022 時遇到 IDE 報錯問題，需要安裝 Visual Studio 2019 才能解決

<br>

**問題詳細說明與解決方案**：

<br>

參考討論：https://www.reddit.com/r/VisualStudio/comments/x9w4ep/vs_2022_keeps_asking_me_to_download_net_framework/?rdt=58431

<br>

這個問題通常出現在 VS 2022 持續要求下載 .NET Framework 的情況下，透過安裝 VS 2019 可以解決相容性問題

<br>

---

## ⚙️ NMQ 組件框架需求

**NMQ Dispatcher 框架需求**：

<br>

需要安裝 .NET Core 2.1

<br>

**其他子 NMQ 組件框架需求**：

<br>

需要安裝 .NET Framework 4.5

<br>

**重要提醒**：

<br>

不同的 NMQ 組件有不同的框架需求，請確保按照組件類型安裝正確的框架版本，避免相容性問題

<br>

---
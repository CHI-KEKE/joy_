
## 為專案申請 EKS + Monitoring + KP ServiceAccount 基本權限資源做法



**開申請單**：https://teamroom.91app.biz/issues/23598


強調須建立 NKP ServiceAccount 基本權限

經確認 Translation 在 NKP IRSA 不與 AKSK 的權限相同，調整如下

僅需要 NY-QA-S3-TranslationService CRUD 權限
不需要 NY-TW-Deny-Not-In-VPC-NAT-TPE-Office-IP

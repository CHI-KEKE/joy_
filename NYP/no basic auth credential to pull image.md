
## 21. 🔑 Image Pull Secrets

### 問題描述

佈署 Translation 新服務時發生 **"no basic auth credential to pull image"** 錯誤，原因是 Kubernetes 無法正確讀取 ImagePullSecrets 來存取私有 Docker Registry。

<br>

### 問題原因

當 Kubernetes 嘗試從私有 Docker Registry (如 `docker-dev.build.91app.io`) 拉取映像時，需要使用 ImagePullSecrets 來進行身份驗證。如果 Secrets 設定不正確或遺失，就會出現權限錯誤。

<br>

### 解決方案

#### 步驟 1：檢查 ImagePullSecrets 設定

確認 Kubernetes Deployment 或 Pod 設定中的 ImagePullSecrets 配置：

![alt text](./image-2.png)

<br>

**重要設定項目**：

| 項目 | 說明 |
|------|------|
| Secret Name | 指向包含 Registry 認證資訊的 Secret |
| Registry URL | 私有 Docker Registry 的位址 |
| 認證資料 | Docker Registry 的使用者名稱和密碼 |

<br>

#### 步驟 2：調整 YAML 配置

透過修改 Kubernetes YAML 檔案來正確設定 ImagePullSecrets：

![alt text](./image-3.png)

<br>

**YAML 範例配置**：

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: translation-service
spec:
  template:
    spec:
      imagePullSecrets:
      - name: docker-registry-secret
      containers:
      - name: translation
        image: docker-dev.build.91app.io/91app/translation:latest
```

<br>

#### 步驟 3：重新部署服務

**自動重新部署**：
- 修改設定檔後，系統會自動觸發重新部署

**手動重新部署**：
- 如果沒有修改設定檔，需要手動執行重新部署

![alt text](./image-4.png)

<br>

### 常見錯誤類型

| 錯誤訊息 | 可能原因 | 解決方案 |
|----------|----------|----------|
| `no basic auth credentials` | ImagePullSecret 不存在或格式錯誤 | 檢查 Secret 設定和引用 |
| `pull access denied` | Registry 權限不足 | 確認 Registry 認證資料正確 |
| `image not found` | 映像路徑或標籤錯誤 | 驗證映像名稱和版本標籤 |
| `timeout` | 網路連線問題 | 檢查 Registry 網路可達性 |

<br>

### 最佳實務

**Secret 管理**：
- 使用 Kubernetes Secret 儲存敏感的認證資料
- 定期輪換 Registry 認證資訊
- 避免在 YAML 中明文儲存密碼

<br>

**部署流程**：
- 在每個 Namespace 中建立對應的 ImagePullSecret
- 確保 ServiceAccount 有正確的 Secret 關聯
- 使用 Helm 或其他工具統一管理 Secret 配置

<br>

### 驗證步驟

**檢查 Secret 是否存在**：
```bash
kubectl get secrets -n <namespace>
kubectl describe secret docker-registry-secret -n <namespace>
```

<br>

**檢查 Pod 狀態**：
```bash
kubectl get pods -n <namespace>
kubectl describe pod <pod-name> -n <namespace>
```

<br>

**查看詳細錯誤**：
```bash
kubectl logs <pod-name> -n <namespace>
kubectl get events -n <namespace> --sort-by=.metadata.creationTimestamp
```

<br>

---

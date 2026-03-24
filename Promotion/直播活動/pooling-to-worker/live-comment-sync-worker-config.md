# Live Comment Sync Worker — 設定說明書

## 用途

`live-comment-sync` 是一個長駐 Worker（k8s Deployment），負責每 10 秒定期呼叫 livebuy API 進行直播留言補撈，補足 Webhook 掉訊息造成的留言缺口。

---

## 相關檔案

| 檔案 | 說明 |
|------|------|
| `charts/live-comment-sync/values-tw-qa.yaml` | TW QA Worker 設定 |
| `charts/live-comment-sync/values-hk-qa.yaml` | HK QA Worker 設定 |
| `charts/live-comment-sync/values-my-qa.yaml` | MY QA Worker 設定 |
| `src/Nine1.Live.Buy.CreateTask/script/call-livebuy-api.ps1` | Worker 執行的 PowerShell 腳本 |

---

## values yaml 完整欄位說明

### 頂層欄位

```yaml
market: TW                        # 市場代碼。TW / HK / MY
env: QA                           # 部署環境。QA / Prod
serviceName: Live-Buy             # 服務名稱，用於組合 config 路徑
fullnameOverride: "live-comment-sync"  # k8s 資源的命名前綴，固定設為 job 名稱
```

> ⚠️ `fullnameOverride` 不可省略。缺少時 Helm 會用 Chart 名稱自動命名，造成資源名稱不符預期。

---

### `worker` 物件

#### 角色與 Config 來源

```yaml
worker:
  role: Worker                    # 固定填 Worker，對應共用 chart 的 Deployment template

  configSource:
    version: "latest"             # S3 config 版本，一般填 latest
    files: []                     # 不需要從 S3 拉取 config 檔案時填空陣列

  secretSource:
    secrets: []                   # 不需要 AWS Secrets Manager 時，填空陣列
                                  # ⚠️ 不可省略此欄位，template 需要此結構存在
```

**需要 S3 config 時的寫法（參考）：**
```yaml
configSource:
  version: "latest"
  files:
    - s3://91app-ap-northeast-1-private-conf/TW-QA/Live-Buy/Worker/{{CONFIG_VERSION}}/settings.json
```

**需要 Secrets Manager 時的寫法（參考）：**
```yaml
secretSource:
  secrets:
    - key: /TW-QA/Live-Buy/Worker/secrets
      fileName: secrets.json
```

---

#### Pod 數量

```yaml
  replicaCount: 1                 # 期望的 Pod 數量
                                  # ⚠️ 欄位名稱是 replicaCount，不是 replicas
                                  # 若 autoscaling.enabled: true，此值會被 HPA 覆蓋
```

---

#### Image

```yaml
  image:
    repository: docker-dev.build.91app.io/91app/nine1-live-buy-create-task
    tag: "latest"
    # pullSecrets 不設定時，template 預設使用 91app-build-site
```

---

#### 環境變數

```yaml
  environmentVariables:
    ASPNETCORE_ENVIRONMENT: "Staging"         # QA 環境填 Staging
    NY_DEPLOY_REGION: ap-northeast-1          # TW: ap-northeast-1 / HK,MY: ap-southeast-1
    NY_DEPLOY_MARKET: tw                      # 小寫市場代碼
    NY_DEPLOY_ENV: QA
    NY_DEPLOY_CONFIG_TYPE: s3
    NY_DEPLOY_CONFIG_URL: https://...         # Voyager config URL（依市場不同）
    NY_DEPLOY_SERVICE_DISCOVERY_TYPE: consul
    NY_DEPLOY_SERVICE_DISCOVERY_URL: http://voyager.qa.91dev.tw:8500/   # 依市場不同
    TZ: "Asia/Taipei"

    # Worker 專用：指定要執行的 PowerShell 腳本
    EXECUTE_FILE_PATH: ./script/call-livebuy-api.ps1

    # Worker 專用：livebuy API 連線資訊
    LIVEBUY_API_URL: http://91app-live-io.qa.91dev.tw   # 依市場不同
    LIVEBUY_API_PATH: /api/live/comments/sync
```

**各市場環境變數差異：**

| 欄位 | TW QA | HK QA | MY QA |
|------|-------|-------|-------|
| `NY_DEPLOY_REGION` | `ap-northeast-1` | `ap-southeast-1` | `ap-southeast-1` |
| `NY_DEPLOY_MARKET` | `tw` | `hk` | `my` |
| `NY_DEPLOY_CONFIG_URL` | `...ap-northeast-1.../ap-northeast-1/qa/` | `...ap-northeast-1.../ap-southeast-1/qa/` | `...ap-northeast-1.../ap-southeast-1/qa/` |
| `NY_DEPLOY_SERVICE_DISCOVERY_URL` | `http://voyager.qa.91dev.tw:8500/` | `http://voyager.qa.91dev.tw:8500/` | `http://voyager.qa.91dev.tw:8500/` |
| `LIVEBUY_API_URL` | `http://91app-live-io.qa.91dev.tw` | `http://91app-live-io.qa1.hk.91dev.tw` | `http://91app-live-io.qa1.my.91dev.tw` |

---

#### 資源配置

```yaml
  resources:                      # Pod 啟動時保證分配的資源 (requests)
    cpu: "200m"
    memory: "250Mi"

  limits:                         # 允許短暫使用的上限資源
    cpu: "400m"                   # 建議為 resources.cpu 的 2 倍
    memory: "250Mi"               # 建議與 resources.memory 相同
                                  # ⚠️ limits 不設定時，template 自動使用 resources 值
```

---

#### Autoscaling（HPA）

```yaml
  autoscaling:
    enabled: false                # Worker 固定 1 個 Pod，不需要自動擴展
    minReplicas: 1                # ⚠️ 即使 enabled: false，欄位也必須填寫
    maxReplicas: 1                #    否則 worker-pdb.yaml template 讀取時會 nil pointer 錯誤
    targetCPUUtilizationPercentage: 80
```

> ⚠️ **常見錯誤：** 只寫 `autoscaling: enabled: false` 而不補完整欄位，部署時會遇到：
> ```
> nil pointer evaluating interface {}.enabled
> ```

---

## 完整 values 範本（TW QA）

```yaml
market: TW
env: QA
serviceName: Live-Buy
fullnameOverride: "live-comment-sync"

worker:
  role: Worker
  configSource:
    version: "latest"
    files: []
  secretSource:
    secrets: []

  replicaCount: 1

  image:
    repository: docker-dev.build.91app.io/91app/nine1-live-buy-create-task
    tag: "latest"

  podAnnotations: {}

  environmentVariables:
    ASPNETCORE_ENVIRONMENT: "Staging"
    NY_DEPLOY_REGION: ap-northeast-1
    NY_DEPLOY_MARKET: tw
    NY_DEPLOY_ENV: QA
    NY_DEPLOY_CONFIG_TYPE: s3
    NY_DEPLOY_CONFIG_URL: https://91app-ap-northeast-1-voyager-configuration.s3-ap-northeast-1.amazonaws.com/ap-northeast-1/qa/
    NY_DEPLOY_SERVICE_DISCOVERY_TYPE: consul
    NY_DEPLOY_SERVICE_DISCOVERY_URL: http://voyager.qa.91dev.tw:8500/
    TZ: "Asia/Taipei"
    EXECUTE_FILE_PATH: ./script/call-livebuy-api.ps1
    LIVEBUY_API_URL: http://91app-live-io.qa.91dev.tw
    LIVEBUY_API_PATH: /api/live/comments/sync

  resources:
    cpu: "200m"
    memory: "250Mi"

  limits:
    cpu: "400m"
    memory: "250Mi"

  autoscaling:
    enabled: false
    minReplicas: 1
    maxReplicas: 1
    targetCPUUtilizationPercentage: 80
```

---

## 常見錯誤與解法

| 錯誤訊息 | 原因 | 解法 |
|---------|------|------|
| `nil pointer evaluating interface {}.enabled` | `autoscaling` 欄位不完整 | 補上 `minReplicas`、`maxReplicas`、`targetCPUUtilizationPercentage` |
| Deployment 名稱不符預期 | 缺少 `fullnameOverride` | 加上 `fullnameOverride: "live-comment-sync"` |
| Pod 數量不對 | 使用 `replicas` 而非 `replicaCount` | 改為 `replicaCount: 1` |

---

## deployments.json 對應設定

Worker 在 `nine1-devops-deployments.QA.json` 的 role 設定如下：

```json
{
  "name": "Worker",
  "APP_ROLE": "worker",
  "RELEASE_NAME": "live-comment-sync",
  "versions": {
    "app": "latest",
    "config": "latest",
    "secret": "v1.0.0"
  }
}
```

> `APP_ROLE: "worker"` 讓共用 Pipeline 選擇 Worker chart template（`nine1-general-worker`）。  
> `APP_ROLE: "scheduler"` 則對應 CronJob chart template。

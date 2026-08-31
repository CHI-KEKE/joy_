
## RELEASE_NAME

### 定義

RELEASE_NAME 是 **Helm release 的唯一識別名稱**，決定 K8s 中所有資源（Deployment、Service、HPA 等）的命名。

### 命名規則（從 FQDN 推導）

```
FQDN            去掉 Nine1.    全小寫 + dot→hyphen
Nine1.Regularpurchase.V2.Worker.Console.NMQv3Worker
  → Regularpurchase.V2.Worker.Console.NMQv3Worker
  → regularpurchase-v2-worker-console-nmqv3worker
```

| 專案 | FQDN | RELEASE_NAME |
|------|------|--------------|
| regularpurchase.v2.worker | `Nine1.Regularpurchase.V2.Worker.Console.NMQv3Worker` | `regularpurchase-v2-worker-console-nmqv3worker` |
| promotion.worker | `Nine1.Promotion.Console.NMQv3Worker` | `promotion-console-nmqv3worker` |
| commerce.nmqv3.worker | `Nine1.Commerce.Nmqv3.Console.NMQv3Worker` | `commerce-nmqv3-console-nmqv3worker` |

### 需對齊的位置

| 位置 | 欄位 | 格式 |
|------|------|------|
| `.gitlab-ci.yml` | `RELEASE_NAME` | `"{release-name}"` |
| `nine1-devops-deployments.QA.json` | `roles[].RELEASE_NAME` | `"{release-name}"` |
| `nine1-devops-deployments.Prod.json` | `roles[].RELEASE_NAME` | `"{release-name}"` |
| `charts/` 目錄名稱 | 資料夾名 | `charts/{release-name}/` |
| `values-tw-*.yaml` | `fullnameOverride` | `{release-name}` |
| `values-tw-*.yaml` | `image.repository` | `docker*.91app.io/91app/nine1-{release-name}` |
| `src/.manifest/sdm.json` | `ServiceId` | `"nine1-{release-name}"` |
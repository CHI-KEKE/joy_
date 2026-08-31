manifest 也在 Redis，一起被刪了

```bash
Fetch 第一步：讀 manifest
    └─ Redis 被清空 → manifest 不見了 → 讀到空的 {}

比對 ETag：
    manifest ETag = null  ≠  S3 ETag "abc123"
    → 全部判定為「需要更新」→ 全部重新下載 ✅
```

這個情況反而是好事，因為 ETag 全部是 null，所以所有 module 都會重新下載。

--------------------------------------------------------------------------------------------------------------------------------

## ✅ Fetch 之後能完全恢復

```bash
 S3（永遠是最終來源）
     → Fetch 重新下載所有翻譯 JSON
     → 寫回 Redis
     → 寫回 manifest
     → InMemory 過期後自動讀到新值
```
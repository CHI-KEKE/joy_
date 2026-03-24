

## 為什麼要實作 WebSocket

1. 即時性要求

需要客服與客戶需要即時雙向對話，HTTP 輪詢會造成延遲和資源浪費，WebSocket 提供持久連線，訊息即時推送，

2. 高平行處理需求

多個客服同時處理多個聊天室，一個聊天室可能有多個連線（客服後台 + 客戶前台），需要訊息廣播功能


3. 降低伺服器負載

避免頻繁的 HTTP 請求建立/斷開連線，減少 Header 重複傳輸的開銷，提升使用者體驗


## 技術選擇：AWS API Gateway WebSocket

這個專案特別的是不使用傳統的 SignalR，而是採用 AWS API Gateway WebSocket

- ✅ 無伺服器架構：不需要管理 WebSocket 伺服器
- ✅ 自動擴展：AWS 自動處理連線擴展
- ✅ 區域化部署：支援多市場（TW/HK/MY）的區域部署
- ✅ 分離前後台連線：Frontend API 和 Backend API 分開管理


## 為什麼 Websocket要拆分 Frontend 和 Backend API？


#### 🔐 情境 A：安全性隔離


假設只有一個 WebSocket API (所有人都連到這裡)

┌─────────────────────────────────────────────────────────────────┐
│         ❌ 單一 API 的潛在安全風險                                 │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  1. 消費者的前端程式碼（可被任何人查看）                            │
│     const ws = new WebSocket(                                     │
│         'wss://chat-api.91app.io/chat/?shopId=1&userId=123&...'  │
│     );                                                            │
│                                                                   │
│     ⚠️ 問題：惡意使用者可以看到完整的 WebSocket URL                │
│                                                                   │
│  2. 惡意使用者的攻擊                                               │
│     // 駭客打開瀏覽器 Console                                      │
│     const ws = new WebSocket(                                     │
│         'wss://chat-api.91app.io/chat/?shopId=1&userId=999&...'  │
│         //                                      ↑ 嘗試偽造客服ID   │
│     );                                                            │
│     ws.send(JSON.stringify({                                      │
│         route: "sendMessage",                                     │
│         chatRoomId: 456,                                          │
│         content: "我是假的客服"  ← 冒充客服                        │
│     }));                                                          │
│                                                                   │
│     雖然有 Token 驗證，但：                                        │
│     ├─ 駭客知道了 API 端點                                         │
│     ├─ 可以進行 DDoS 攻擊                                          │
│     ├─ 可以嘗試暴力破解 Token                                      │
│     └─ 可以探測 API 的漏洞                                         │
│                                                                   │
└───────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│         ✅ 拆分 Frontend / Backend API 的安全優勢                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  1. 消費者的前端程式碼                                             │
│     const ws = new WebSocket(                                     │
│         'wss://cwlbu4wu41.execute-api.ap-northeast-1....'        │
│         //   ↑ Frontend API ID (公開的)                          │
│     );                                                            │
│                                                                   │
│  2. 客服的後台程式碼（不公開）                                      │
│     const ws = new WebSocket(                                     │
│         'wss://va3ctvj2x6.execute-api.ap-northeast-1....'        │
│         //   ↑ Backend API ID (不公開的)                         │
│     );                                                            │
│                                                                   │
│     ✅ 優勢：                                                      │
│     ├─ 消費者完全不知道 Backend API 的 URL                        │
│     ├─ 即使駭客攻擊 Frontend API，Backend API 不受影響             │
│     ├─ 可以針對 Frontend API 設定更嚴格的 Rate Limit               │
│     └─ 可以分別監控兩個 API 的流量和錯誤率                         │
│                                                                   │
└───────────────────────────────────────────────────────────────┘


#### 📊 情境 B：權限控制與資料隔離

┌─────────────────────────────────────────────────────────────────┐
│         如果只用 Role 區分（單一 API）                             │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  // 後端需要在每個請求都檢查 Role                                  │
│  public async Task SendMessageAsync(SendMessageRequest req)      │
│  {                                                                │
│      var connection = await GetConnection(req.ConnectionId);     │
│                                                                   │
│      // ⚠️ 需要在程式碼層級檢查權限                                │
│      if (connection.Role == "Member")                             │
│      {                                                            │
│          // 消費者只能看到自己的聊天室                             │
│          if (req.ChatRoomId != connection.AllowedChatRoom)       │
│          {                                                        │
│              throw new UnauthorizedException();                  │
│          }                                                        │
│      }                                                            │
│      else if (connection.Role == "Agent")                        │
│      {                                                            │
│          // 客服可以看到多個聊天室                                 │
│          if (!connection.AllowedChatRooms.Contains(              │
│              req.ChatRoomId))                                    │
│          {                                                        │
│              throw new UnauthorizedException();                  │
│          }                                                        │
│      }                                                            │
│                                                                   │
│      // ... 業務邏輯                                              │
│  }                                                                │
│                                                                   │
│  ❌ 問題：                                                         │
│  ├─ 每個 API 都需要重複權限檢查                                    │
│  ├─ 容易遺漏權限檢查（安全漏洞）                                   │
│  ├─ 難以審計誰做了什麼操作                                         │
│  └─ 效能開銷（每次都查詢權限）                                     │
│                                                                   │
└───────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│         拆分 Frontend / Backend API 的權限優勢                     │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  Frontend API (消費者專用)                                        │
│  ├─ AWS IAM Policy: 只允許連接到自己的聊天室                       │
│  ├─ Rate Limit: 每分鐘 100 則訊息                                 │
│  ├─ CloudWatch Alarm: 監控異常流量                                │
│  └─ 不需要在程式碼層級檢查權限（基礎設施層級保護）                  │
│                                                                   │
│  Backend API (客服專用)                                           │
│  ├─ 只有內網/VPN 可以存取（更嚴格的網路隔離）                       │
│  ├─ Rate Limit: 每分鐘 1000 則訊息（客服需要更高限制）             │
│  ├─ 獨立的監控和告警                                              │
│  └─ 可以存取所有聊天室（業務需求）                                 │
│                                                                   │
└───────────────────────────────────────────────────────────────┘



## 為什麼需要 ConnectionId？

情境：消費者 Alice (userId=123) 使用多個裝置

┌─────────────────────────────────────────────────────────────────┐
│  時間軸：Alice 的連線行為                                          │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  10:00 - Alice 在電腦上打開聊天室 #456                            │
│          └─ WebSocket 連線建立                                    │
│          └─ AWS 產生: connectionId = "abc123xyz"                 │
│          └─ Redis 儲存:                                           │
│              Connection:abc123xyz {                               │
│                  userId: 123,                                     │
│                  chatRoomId: 456,                                 │
│                  role: "Member",                                  │
│                  device: "Desktop"  ← 這是同一個人的不同裝置      │
│              }                                                    │
│              ChatRoom:456 {                                       │
│                  connectionIdList: ["abc123xyz"]                  │
│              }                                                    │
│                                                                   │
│  10:05 - Alice 在手機上也打開聊天室 #456（電腦的連線還在）         │
│          └─ WebSocket 連線建立                                    │
│          └─ AWS 產生: connectionId = "def456uvw"  ← 不同的ID     │
│          └─ Redis 更新:                                           │
│              Connection:def456uvw {                               │
│                  userId: 123,  ← 同一個使用者                     │
│                  chatRoomId: 456,  ← 同一個聊天室                 │
│                  role: "Member",                                  │
│                  device: "Mobile"  ← 但是不同的裝置               │
│              }                                                    │
│              ChatRoom:456 {                                       │
│                  connectionIdList: ["abc123xyz", "def456uvw"]     │
│                                                  ↑ 新增           │
│              }                                                    │
│                                                                   │
│  10:10 - 客服 Bob (userId=789) 打開聊天室 #456                    │
│          └─ WebSocket 連線建立                                    │
│          └─ AWS 產生: connectionId = "ghi789rst"  ← 又一個ID     │
│          └─ Redis 更新:                                           │
│              Connection:ghi789rst {                               │
│                  userId: 789,  ← 不同使用者                       │
│                  chatRoomId: 456,  ← 同一個聊天室                 │
│                  role: "Agent",  ← 不同角色                       │
│                  device: "Desktop"                                │
│              }                                                    │
│              ChatRoom:456 {                                       │
│                  connectionIdList: [                              │
│                      "abc123xyz",  ← Alice 電腦                   │
│                      "def456uvw",  ← Alice 手機                   │
│                      "ghi789rst"   ← Bob 客服                     │
│                  ]                                                │
│              }                                                    │
│                                                                   │
│  🎯 關鍵問題：如果只用 chatRoomId，如何區分這 3 個連線？          │
│                                                                   │
│  10:15 - Alice 在手機上關閉聊天室                                 │
│          └─ $disconnect 觸發                                      │
│          └─ 需要知道是哪個 connectionId 要斷開                    │
│          └─ 如果只有 chatRoomId，無法識別要刪除哪一個！           │
│                                                                   │
│  ❌ 錯誤做法（只用 chatRoomId）：                                  │
│      DELETE ChatRoom:456  ← 會把所有連線都刪除！                  │
│                            (包括 Alice 的電腦和 Bob)               │
│                                                                   │
│  ✅ 正確做法（使用 connectionId）：                                │
│      DELETE Connection:def456uvw  ← 只刪除手機的連線             │
│      ChatRoom:456.connectionIdList.remove("def456uvw")            │
│      保留: ["abc123xyz", "ghi789rst"]  ← 其他連線不受影響         │
│                                                                   │
└───────────────────────────────────────────────────────────────┘


#### 📊 ConnectionId 的特性

每次 WebSocket 連線都會產生唯一的 ConnectionId

特性 1: 唯一性
├─ AWS API Gateway 保證全域唯一
├─ 格式: 類似 "L0SM9cOFvHcCJhg="（Base64）
└─ 即使同一個使用者、同一個聊天室，每次連線都不同

特性 2: 生命週期
├─ 連線建立時產生
├─ 連線斷開時失效
└─ 無法重複使用（關閉後重開會產生新的）

特性 3: 與實體的關係
├─ 1 個使用者 可以有 N 個 connectionId（多裝置）
├─ 1 個聊天室 可以有 N 個 connectionId（多人）
└─ 1 個 connectionId 只對應 1 個 WebSocket 連線

```csharp
// 斷線時必須使用 connectionId，不能只用 chatRoomId
public async Task DisconnectAsync(string connectionId)
{
    //// 1. 根據 connectionId 找到對應的聊天室
    var connectionCacheEntity = await _webSocketConnectionService
        .GetConnectionCacheEntityAsync(connectionId);
    
    var chatRoomId = connectionCacheEntity.ChatRoomId;
    
    //// 2. 只移除這一個 connectionId，不影響其他連線
    await _webSocketConnectionService
        .RemoveChatRoomConnectionIdsAsync(chatRoomId, new List<string> { connectionId });
    
    //// 3. 刪除這個 connectionId 的快取
    await _webSocketConnectionService
        .RemoveConnectionCacheEntityAsync(connectionId);
}
```

##  路由自己實作的意思

┌─────────────────────────────────────────────────────────────────┐
│          SignalR 的自動路由（框架提供）                            │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  // 開發者只需要寫這些                                             │
│  public class ChatHub : Hub                                       │
│  {                                                                │
│      public async Task JoinChatRoom(long chatRoomId)             │
│      {                                                            │
│          // 框架自動管理 Group                                     │
│          await Groups.AddToGroupAsync(                            │
│              Context.ConnectionId,  ← 框架自動追蹤                │
│              $"ChatRoom_{chatRoomId}"  ← Group 名稱               │
│          );                                                       │
│      }                                                            │
│                                                                   │
│      public async Task SendMessage(long chatRoomId, string msg)  │
│      {                                                            │
│          // 框架自動路由到 Group 內所有連線                        │
│          await Clients.Group($"ChatRoom_{chatRoomId}")           │
│              .SendAsync("ReceiveMessage", msg);                   │
│          // ✅ 不需要查詢誰在這個 Group                            │
│          // ✅ 不需要判斷 API Type                                 │
│          // ✅ 不需要處理發送失敗                                  │
│      }                                                            │
│  }                                                                │
│                                                                   │
│  框架內部自動做了：                                                │
│  ├─ 維護 Group 成員列表（記憶體中）                               │
│  ├─ 管理 ConnectionId 生命週期                                    │
│  ├─ 處理連線斷開時的清理                                          │
│  └─ 訊息路由到所有 Group 成員                                     │
│                                                                   │
└───────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────┐
│     AWS API Gateway + 自己實作路由（此專案）                       │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  步驟 1: 定義 Group 的單位 ← 你說的完全正確                       │
│  ┌─────────────────────────────────────────────────────┐        │
│  │  決策：以 chatRoomId 為 Group 單位                   │        │
│  │  原因：同一個聊天室的人需要看到相同的訊息            │        │
│  └─────────────────────────────────────────────────────┘        │
│                                                                   │
│  步驟 2: 自己實作 Group 管理（使用 Redis）                        │
│  ┌─────────────────────────────────────────────────────┐        │
│  │  // 連線時加入 Group                                 │        │
│  │  public async Task ConnectAsync(ConnectRequest req)  │        │
│  │  {                                                   │        │
│  │      var chatRoomId = req.ChatRoomId;               │        │
│  │      var connectionId = req.ConnectionId;           │        │
│  │                                                      │        │
│  │      // 🎯 自己儲存到 Redis                          │        │
│  │      var chatRoom = await GetChatRoomCache(         │        │
│  │          chatRoomId);                                │        │
│  │      chatRoom.ConnectionIdList.Add(connectionId);   │        │
│  │      await SaveChatRoomCache(chatRoomId, chatRoom); │        │
│  │  }                                                   │        │
│  └─────────────────────────────────────────────────────┘        │
│                                                                   │
│  步驟 3: 自己實作訊息路由（從 Redis 查詢）                        │
│  ┌─────────────────────────────────────────────────────┐        │
│  │  public async Task SendMessageAsync(SendRequest req) │        │
│  │  {                                                   │        │
│  │      var chatRoomId = req.ChatRoomId;               │        │
│  │                                                      │        │
│  │      // 🎯 從 Redis 查詢 Group 成員                  │        │
│  │      var chatRoom = await GetChatRoomCache(         │        │
│  │          chatRoomId);                                │        │
│  │      var connectionIds =                             │        │
│  │          chatRoom.ConnectionIdList;                  │        │
│  │      // ["abc123", "def456", "ghi789"]              │        │
│  │                                                      │        │
│  │      // 🎯 查詢每個連線的詳細資訊                     │        │
│  │      foreach (var connId in connectionIds)          │        │
│  │      {                                               │        │
│  │          var conn = await GetConnectionCache(       │        │
│  │              connId);                                │        │
│  │          // { role: "Agent", userId: 789 }          │        │
│  │      }                                               │        │
│  │  }                                                   │        │
│  └─────────────────────────────────────────────────────┘        │
│                                                                   │
│  步驟 4: 根據不同 API Type 發送                                   │
│  ┌─────────────────────────────────────────────────────┐        │
│  │  // 🎯 判斷使用哪個 API                              │        │
│  │  var requests = connections.Select(conn => {        │        │
│  │      var apiType = conn.Role == "Agent"             │        │
│  │          ? SocketAPIType.Backend                    │        │
│  │          : SocketAPIType.Frontend;                  │        │
│  │                                                      │        │
│  │      return new WebSocketRequest {                  │        │
│  │          ConnectionId = conn.Id,                    │        │
│  │          Message = message,                         │        │
│  │          ApiType = apiType  ← 這裡決定用哪個 API     │        │
│  │      };                                              │        │
│  │  });                                                 │        │
│  │                                                      │        │
│  │  // 🎯 並行發送                                      │        │
│  │  await SendWebSocketMessagesAsync(requests);        │        │
│  └─────────────────────────────────────────────────────┘        │
│                                                                   │
│  步驟 5: 自己處理發送失敗                                         │
│  ┌─────────────────────────────────────────────────────┐        │
│  │  var failedIds = await SendMessages(requests);      │        │
│  │  if (failedIds.Any())                               │        │
│  │  {                                                   │        │
│  │      // 🎯 清理失敗的連線                            │        │
│  │      await RemoveConnectionsFromGroup(              │        │
│  │          chatRoomId, failedIds);                    │        │
│  │  }                                                   │        │
│  └─────────────────────────────────────────────────────┘        │
│                                                                   │
└───────────────────────────────────────────────────────────────┘
 
 ✅ 1. Group 單位的選擇
   └─ 決定：以 chatRoomId 為 Group 單位
   └─ 原因：需要對話隔離，聊天室 A 的人看不到聊天室 B 的訊息

✅ 2. 從資料庫/Redis 查詢 Group 成員
   └─ Redis HGET "ChatRoom:456" → 取得 connectionIdList
   └─ 遍歷每個 connectionId 取得詳細資訊

✅ 3. 成員可能來自不同角色
   ├─ 客服 (Agent) → 來自後台管理系統
   ├─ 消費者 (Member) → 來自官網前台
   └─ AI 客服 (AIAgent) → 來自 AI 系統

✅ 4. 根據不同 API Type 發送
   ├─ Agent → Backend API (不同的 apiId)
   └─ Member → Frontend API (不同的 apiId)


##  客服工作台流程與聊天室隔離機制

┌─────────────────────────────────────────────────────────────────┐
│              客服後台管理系統 (SPA Frontend)                         │
├─────────────────────────────────────────────────────────────────┤
│                                                                   │
│  1️⃣ 客服登入後 - 工作台首頁                                         │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  📊 待處理聊天室列表 (每 5 秒輪詢或接收推播)                │    │
│  │                                                           │    │
│  │  GET /api/chatrooms/unread-agent-chatrooms               │    │
│  │  ↓                                                        │    │
│  │  ✅ 聊天室 #456 (消費者: 小明)  👤 未分配  🔴 3則未讀     │    │
│  │  ✅ 聊天室 #789 (消費者: 小華)  👤 客服A   🟡 1則未讀     │    │
│  │  ✅ 聊天室 #101 (消費者: 小李)  👤 我      ⚪ 已回覆      │    │
│  │                                                           │    │
│  │  這些資料從 DB 查詢（客服不在時儲存的訊息）                 │    │
│  └─────────────────────────────────────────────────────────┘    │
│                           │                                      │
│                           │ 客服點選「聊天室 #456」                │
│                           ▼                                      │
│  2️⃣ 開啟聊天室視窗                                               │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │  Step 1: 載入歷史訊息                                     │    │
│  │  GET /api/chatrooms/456/messages?limit=50                │    │
│  │  ↓                                                        │    │
│  │  從 DB 取得聊天記錄：                                      │    │
│  │  - 消費者(10:00): 請問商品還有貨嗎？                       │    │
│  │  - 消費者(10:05): 急！麻煩盡快回覆                         │    │
│  │  - 消費者(10:10): 在嗎？                                  │    │
│  │                                                           │    │
│  │  Step 2: 建立 WebSocket 連線                             │    │
│  │  new WebSocket('wss://backend-api.../chat/?              │    │
│  │      shopId=1&userId=789&chatRoomId=456&...')           │    │
│  │  ↓                                                        │    │
│  │  POST /api/websocket/connections                         │    │
│  │  - 儲存連線到 Redis                                       │    │
│  │  - Key: "ChatRoom:456:Connections"                       │    │
│  │  - Value: { "conn_agent_789": {...} }                    │    │
│  │                                                           │    │
│  │  Step 3: 開始接收即時訊息                                 │    │
│  │  websocket.onmessage = (event) => {                      │    │
│  │      // 如果消費者此時發新訊息，立即顯示                   │    │
│  │  }                                                        │    │
│  └─────────────────────────────────────────────────────────┘    │
│                           │                                      │
│                           │ 客服輸入回覆                          │
│                           ▼                                      │
│  3️⃣ 客服發送訊息                                                 │
│  websocket.send(JSON.stringify({                                │
│      route: "sendMessage",                                       │
│      chatRoomId: 456,                                            │
│      content: "有貨喔！"                                          │
│  }))                                                             │
│  ↓                                                               │
│  POST /api/websocket/messages                                    │
│  - 儲存訊息到 DB ✓                                               │
│  - 發送給聊天室內所有連線 ✓                                      │
│    ├─ 消費者 (Frontend API) ✓                                   │
│    └─ 客服自己 (Backend API) ✓ (echo 確認)                       │
│                                                                   │
└───────────────────────────────────────────────────────────────┘

所以當消費者在聊天室 #456 發送訊息時：
1. ChatRoomMessagingService.SendMessageAsync(chatRoomId=456)
   │
   ├─ 查詢 Redis: "ChatRoom:456"
   │  └─ 取得 ConnectionIdList: ["conn_member_123", "conn_agent_789"]
   │
   ├─ 並行查詢連線詳細資訊
   │  ├─ "Connection:conn_member_123" → Role: Member → 用 Frontend API
   │  └─ "Connection:conn_agent_789"  → Role: Agent  → 用 Backend API
   │
   └─ 發送訊息
      ├─ PostToConnection(conn_member_123, Frontend API) ✓
      └─ PostToConnection(conn_agent_789, Backend API) ✓


## Redis 連線狀態管理詳解

AWS API Gateway WebSocket 只負責連線本身，但不知道「這個連線屬於哪個聊天室」。所以需要自己用 Redis 管理：


┌─────────────────────────────────────────────────────────────────┐
│              1️⃣ 連線建立 ($connect)                              │
└─────────────────────────────────────────────────────────────────┘

消費者連線請求
wss://frontend-api.../chat/?shopId=1&userId=123&chatRoomId=456&...
    │
    ▼
AWS API Gateway 產生 connectionId: "conn_abc123"
    │
    ▼
POST /api/websocket/connections
    │
    ├─ UserService.ConnectAsync()
    │   │
    │   ├─ Step 1: 驗證使用者
    │   │   └─ SELECT * FROM Users WHERE UserId=123 ✓
    │   │
    │   ├─ Step 2: 建立連線快取
    │   │   └─ Redis HSET "Connection:conn_abc123" 
    │   │       {
    │   │         "ShopId": 1,
    │   │         "UserId": 123,
    │   │         "ChatRoomId": 456,
    │   │         "Role": "Member"
    │   │       }
    │   │       TTL: 20分鐘 ⏰
    │   │
    │   └─ Step 3: 加入聊天室連線清單
    │       └─ Redis HGET "ChatRoom:456"
    │           ├─ 如果不存在，建立新的
    │           ├─ ConnectionIdList.Add("conn_abc123")
    │           └─ HSET "ChatRoom:456"
    │               {
    │                 "ShopId": 1,
    │                 "ConnectionIdList": ["conn_abc123"]
    │               }
    │               TTL: 60分鐘 ⏰
    │
    └─ 返回 204 NoContent ✓

┌─────────────────────────────────────────────────────────────────┐
│              2️⃣ 連線活躍檢查 (Keep-Alive)                        │
└─────────────────────────────────────────────────────────────────┘

消費者/客服發送任何訊息或通知
    │
    ▼
CheckConnectionAsync(chatRoomId=456, connectionId="conn_abc123")
    │
    ├─ Step 1: 檢查聊天室快取是否存在
    │   └─ Redis HGET "ChatRoom:456" ✓
    │
    ├─ Step 2: 檢查連線是否在聊天室內
    │   └─ "conn_abc123" in ConnectionIdList ✓
    │
    ├─ Step 3: 檢查連線快取是否存在
    │   └─ Redis HGET "Connection:conn_abc123" ✓
    │
    └─ Step 4: 延長 TTL (續約)
        ├─ EXPIRE "Connection:conn_abc123" 1200秒 (20分鐘)
        └─ EXPIRE "ChatRoom:456" 3600秒 (60分鐘)

┌─────────────────────────────────────────────────────────────────┐
│              3️⃣ 連線斷開 ($disconnect)                           │
└─────────────────────────────────────────────────────────────────┘

消費者關閉瀏覽器或網路斷線
    │
    ▼
AWS API Gateway 觸發 $disconnect
    │
    ▼
DELETE /api/websocket/connections
    │
    ├─ UserService.DisconnectAsync("conn_abc123")
    │   │
    │   ├─ Step 1: 取得連線資訊
    │   │   └─ Redis HGET "Connection:conn_abc123"
    │   │       → { ChatRoomId: 456 }
    │   │
    │   ├─ Step 2: 從聊天室移除連線
    │   │   └─ RemoveChatRoomConnectionIdsAsync(456, ["conn_abc123"])
    │   │       ├─ HGET "ChatRoom:456"
    │   │       ├─ ConnectionIdList.Remove("conn_abc123")
    │   │       └─ HSET "ChatRoom:456"
    │   │           {
    │   │             "ConnectionIdList": []  ← 變成空陣列
    │   │           }
    │   │
    │   └─ Step 3: 刪除連線快取
    │       └─ Redis DEL "Connection:conn_abc123" ✓
    │
    └─ 返回 204 NoContent ✓

┌─────────────────────────────────────────────────────────────────┐
│              4️⃣ 失敗連線清理                                      │
└─────────────────────────────────────────────────────────────────┘

訊息發送失敗 (例如客戶端已斷線但未觸發 $disconnect)
    │
    ▼
SendWebSocketMessagesAsync() 返回 failedConnectionIds
    │
    ├─ HandleWebSocketResultsAsync(chatRoomId=456, failedIds=["conn_abc123"])
    │   │
    │   ├─ 從聊天室移除失敗連線
    │   │   └─ RemoveChatRoomConnectionIdsAsync(456, ["conn_abc123"])
    │   │
    │   └─ 刪除連線快取
    │       └─ RemoveConnectionCacheEntityAsync("conn_abc123")
    │
    └─ 記錄錯誤日誌 📝

┌─────────────────────────────────────────────────────────────────┐
│              5️⃣ 自動過期 (TTL 機制)                              │
└─────────────────────────────────────────────────────────────────┘

連線 20 分鐘沒有任何活動
    │
    ├─ Redis 自動刪除 "Connection:conn_abc123" (TTL 到期)
    │
    └─ 下次訊息發送會失敗，觸發清理流程


#### 情境 A：正常使用

10:00 - 消費者開啟聊天室
        └─ Redis: Connection:conn_123 (TTL: 20min)
        └─ Redis: ChatRoom:456 → ["conn_123"]

10:05 - 消費者發送訊息
        └─ CheckConnectionAsync() → 延長 TTL

10:10 - 消費者發送訊息
        └─ CheckConnectionAsync() → 延長 TTL

10:15 - 消費者關閉聊天室
        └─ $disconnect 觸發
        └─ 刪除 Connection:conn_123
        └─ ChatRoom:456 → []


#### 情境 B：網路斷線未觸發 $disconnect

10:00 - 消費者開啟聊天室
        └─ Redis: Connection:conn_123 (TTL: 20min)

10:05 - 消費者網路斷線 (未觸發 $disconnect)
        └─ Redis 資料還在，但連線實際上已斷

10:10 - 客服發送訊息
        └─ SendToConnection(conn_123) → 失敗 ❌
        └─ HandleWebSocketResultsAsync()
        └─ 主動清理 Connection:conn_123 ✓

10:25 - Redis TTL 到期
        └─ 自動刪除 Connection:conn_123 (備援機制)


#### 情境 C：客服查看聊天室

10:00 - 客服打開聊天室
        └─ 先用 REST API 載入歷史訊息
            GET /api/chatrooms/456/messages
        
10:00:05 - 建立 WebSocket 連線
        └─ Redis: Connection:conn_789 (TTL: 20min, Role: Agent)
        └─ Redis: ChatRoom:456 → ["conn_123", "conn_789"]
                                   ↑消費者     ↑客服

10:05 - 消費者發送訊息
        └─ 系統查詢 ChatRoom:456
        └─ 發送給 conn_123 (Frontend API) ✓
        └─ 發送給 conn_789 (Backend API) ✓
        └─ 客服立即收到訊息 ✓


## 客服如何知道有留言？


 情境：客服在處理聊天室時

1️⃣ 客服打開後台系統
   └─ 可能有「待處理聊天室列表頁」(REST API 查詢)
   └─ 或者有「全域通知連線」(WebSocket，但程式碼中沒有明確看到)

2️⃣ 客服點選要處理的聊天室
   └─ 建立 WebSocket 連線到該聊天室
   └─ POST /api/websocket/connections (chatRoomId=456, userId=789, role=Agent)

3️⃣ 客服閱讀訊息時
   └─ 前端發送「已讀通知」
   └─ { route: "sendNotification", type: "ReadMessage", readMessageId: 12345 }
   └─ POST /api/websocket/notifications
       └─ SendReadMessageNotificationAsync()
           ├─ 更新資料庫的 ReadMessageId
           └─ 發送通知給「聊天室內所有其他連線」
               └─ 消費者會看到「客服已讀」的提示

4️⃣ 消費者發新訊息時
   └─ 如果客服「已經連線到該聊天室」→ 立即收到訊息
   └─ 如果客服「沒有連線」→ 訊息存入資料庫，等客服下次打開


##  為什麼使用 AWS API Gateway

面向	AWS API Gateway WebSocket	自建 SignalR Server
伺服器管理	✅ 無需管理，完全託管	❌ 需要管理伺服器、負載平衡器
自動擴展	✅ AWS 自動處理，無限擴展	⚠️ 需要手動設定 Auto Scaling
高可用性	✅ AWS 保證 99.95% SLA	⚠️ 需要自己實作 Multi-AZ
區域部署	✅ 輕鬆部署到多個 Region	❌ 需要在各區域建立基礎設施
連線狀態管理	⚠️ 需要自己用 Redis 管理	✅ SignalR 內建連線管理
成本（小流量）	✅ 按使用量計費，沒流量不收費	❌ 需要至少一台伺服器持續運行
成本（大流量）	⚠️ 可能昂貴（每百萬連線分鐘）	✅ 固定成本，流量再大都一樣
延遲	⚠️ 多一層 API Gateway（~50ms）	✅ 直連伺服器，延遲更低
開發複雜度	⚠️ 需要自己實作訊息路由邏輯	✅ SignalR 抽象層簡化開發
監控與日誌	✅ 整合 CloudWatch，完整監控	⚠️ 需要自建監控系統
前後台分離	✅ 輕鬆建立多個 API	⚠️ 需要程式碼層級控制


#### 無伺服器架構的優勢


情境：週年慶大促銷

傳統 SignalR：
- 需要提前規劃容量
- 需要手動擴展伺服器數量
- 擴展需要 5-10 分鐘
- 促銷結束後需要手動縮減（否則浪費成本）

AWS API Gateway：
- 完全自動擴展，無上限
- 擴展時間：毫秒級
- 促銷結束後自動縮減，成本立即下降
- 開發團隊不需要待命處理擴展問題



## 發送訊息流程 (sendMessage)

客戶端/客服端
    │
    ├─ 透過 WebSocket 發送訊息
    │  { "route": "sendMessage", "chatRoomId": 123, "content": "Hello" }
    │
    ▼
AWS API Gateway (sendMessage route)
    │
    ├─ Integration Request Template (轉換參數 + 加入 connectionId)
    │
    ▼
POST /api/websocket/messages
    │
    ├─ WebSocketController.SendMessageAsync()
    │   │
    │   ▼
    │   ChatRoomMessagingService.SendMessageAsync()
    │       │
    │       ├─ 1. 前置準備
    │       │   └─ PrepareMessageContextAsync() (取得使用者、聊天室、連線資訊)
    │       │
    │       ├─ 2. 驗證
    │       │   ├─ ValidateUserAndChatRoom()
    │       │   ├─ ValidateRateLimitAndConnectionIfNeededAsync()
    │       │   └─ ValidateUserPermissionIfNeededAsync()
    │       │
    │       ├─ 3. 產生訊息ID
    │       │   └─ _messageRepository.GetMessageIdAsync()
    │       │
    │       ├─ 4. 處理 Conversation
    │       │   └─ HandleConversationIdAsync()
    │       │
    │       ├─ 5. 並行處理
    │       │   ├─ SendToWebSocketClientsAsync() ──┐
    │       │   │   (發送訊息給所有連線)            │  並行執行
    │       │   └─ SaveMessageIfNotTimeoutAsync() ──┘
    │       │       (儲存訊息到資料庫)
    │       │
    │       ├─ 6. 處理發送結果
    │       │   └─ HandleWebSocketResultsAsync()
    │       │       (清理失敗的連線)
    │       │
    │       └─ 7. 更新聊天室狀態 & 通知 AI
    │           ├─ HandleChatRoomStatusAsync()
    │           └─ HandleMessageToAIAsync()
    │
    ▼
WebSocketAPIHttpClientService.SendWebSocketMessagesAsync()
    │
    ├─ 取得所有聊天室內的連線 (從 Cache)
    ├─ 判斷每個連線的 SocketAPIType (Frontend/Backend)
    │
    ▼
    並行發送到多個連線
    ├─ SendWebSocketMessageAsync(connectionId1, message, Frontend)
    ├─ SendWebSocketMessageAsync(connectionId2, message, Backend)
    └─ SendWebSocketMessageAsync(connectionId3, message, Frontend)
        │
        ▼
    AmazonFactory.CreateOrGetApiGatewayClient(socketApiType)
        │
        ├─ 從 Cache 取得或建立 IAmazonApiGatewayManagementApi
        │   ├─ Frontend API Client → https://{frontendApiId}.execute-api...
        │   └─ Backend API Client → https://{backendApiId}.execute-api...
        │
        ▼
    apiClient.PostToConnectionAsync(request)
        │  {
        │    ConnectionId: "xxx",
        │    Data: { messageId, content, role, timestamp... }
        │  }
        │
        └─> AWS API Gateway → 推送給對應 connectionId 的客戶端


## 斷線流程 ($disconnect)

客戶端/客服端
    │
    ├─ 關閉 WebSocket 連線 (主動或網路斷線)
    │
    ▼
AWS API Gateway ($disconnect route)
    │
    ├─ Integration Request Template
    │  { "connectionId": "$context.connectionId", "userName": "..." }
    │
    ▼
DELETE /api/websocket/connections
    │
    ├─ WebSocketController.DeleteConnection()
    │   │
    │   └─ _userService.DisconnectAsync(connectionId)
    │       │
    │       ├─ 從 Redis Cache 移除連線資訊
    │       └─ 清理相關資源
    │
    └─ 返回 204 NoContent


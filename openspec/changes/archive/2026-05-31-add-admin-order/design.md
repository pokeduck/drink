## Context

訂單（揪團）是平台核心業務但目前後端尚未實作。`specs-legacy/admin-order.md` 已有詳細規格、`memory:project_spec_order.md` 已記錄核心決策，本次 change 將其形式化為 OpenSpec capability `admin-order`，並把後端打通至可被後台呼叫的程度。

**現況**
- 已存在的相關 Entity：`Shop`、`ShopMenuItem`、`Size`、`Sugar`、`Ice`、`Topping`、`ShopMenuItemSize/Sugar/Ice/Topping`、`ShopSugarOverride`、`ShopToppingOverride`、`User`、`AdminUser`
- 通知 infra：`IEmailSender` 介面 + `LogEmailSender` mock 實作（目前只支援 `SendVerificationEmailAsync`）
- AdminMenu 已預留訂單管理項目（Id=7、Id=8 `/order/list`），不需新增 seed
- `ErrorCodes.cs` 已預留模組 05（Order）的 7 個錯誤碼
- DB 使用 `UseSnakeCaseNamingConvention()`，所有 table / column 自動轉 snake_case
- Migrator 透過 `RegisterAllEntities()` 自動掃 Entity（讓 `DbSet` 註冊不必手寫）

**限制**
- 後台只「查看」飲料明細，所有編輯/刪除權落在前台（user-order capability 處理）
- 通知本期僅 email 路徑，Push 留給 Firebase 整合的另一個 change
- 後台 UI 不在本 change 範圍，但 API 設計要能支撐既有 admin-ui.md 規範下的列表頁 + 詳情頁

## Goals / Non-Goals

**Goals:**
- 形式化 admin-order capability，包含 Entity 結構、狀態流轉、價格快照、Excel 匯出、Email 通知
- 提供完整 `/api/admin/orders` API 集合（列表/詳情/改狀態/取消/匯出/通知）
- 通知設計可擴充：本期 Email-only，但介面與分派邏輯預留 Push 通道位置
- 一個 change 內完成 Entity 建立 + Migration + Admin API + `pnpm generate` 可重跑

**Non-Goals:**
- 前台 user-order API（建立揪團、加入飲料、編輯/刪除飲料）— 另開 `add-user-order` change
- Admin Nuxt UI 頁面 — 另開 UI change
- Firebase push notification — 另開 change，預期僅補上 `IPushSender` 介面與 `NotificationDispatcher.SendPushAsync`
- 通知歷史紀錄資料表 — 屬於 notification capability，本 change 不開新 table，只透過 `IEmailSender` 直接送出

## Decisions

### D1 — Excel 套件選用 ClosedXML

| 候選 | 結論 |
|------|------|
| **ClosedXML** | ✅ MIT license、純 OSS、API 直覺、廣泛使用、支援 .NET 8+ |
| EPPlus v4 | ⚠️ v5 起改 PolyForm Noncommercial license；停在 v4 雖可商用但已停止更新 |
| OpenXML SDK 原生 | ❌ 太底層、開發成本高，不適合單純列表匯出 |

**Why ClosedXML**：MIT、API 簡潔、未來補 user-order export 時也能直接複用。

### D2 — 通知分派架構

新增 `OrderNotificationService`（屬於 `Application/Services`），透過 `NotificationDispatcher` 抽象（本期定義為 `IEmailSender` 直呼）對每位收件人依 `NotificationType` 決定通道：

```
NotificationType.None    → skip
NotificationType.Email   → IEmailSender.SendOrderNotificationAsync
NotificationType.WebPush → skip（log "PUSH_NOT_IMPLEMENTED"，等 Firebase change 替換）
NotificationType.Both    → 呼叫 Email；Push 同樣 skip
```

**擴充 `IEmailSender`** 加入：

```csharp
Task SendOrderNotificationAsync(string to, string recipientName, string groupTitle, string shopName, string note);
```

實作仍由 `LogEmailSender` 提供 mock，僅 log。等正式 mail provider（Resend/SMTP）上線時整個 sender 一起換掉。

**Why not** 建一個 `INotificationService` interface 大包？因為 push 與 email 通道差異大（push 需要 device token / FCM SDK），本期先把 email 路徑做穩，等 push 進來時再抽出共同介面才不會過度抽象。

### D3 — 狀態流轉驗證放在 Service 層

合法轉換表（白名單矩陣）寫在 `AdminOrderService` 內常數，違反者回 `InvalidStatusTransition`。

```
Active     → Closed, Delivered, Cancelled
Closed     → Active, Delivered, Cancelled
Delivered  → Active, Closed, Completed
Completed  → (none)        // 終態
Cancelled  → (none)        // 終態
```

`PUT /status` 與 `PUT /cancel` 共用同一個內部 `TransitionAsync(orderId, target)` 方法，差別只在 controller 預先指定 `target = Cancelled` 並額外擋住「非 Active/Closed」狀態（用 `CannotCancelOrder` 而非 `InvalidStatusTransition`，因為這是業務語意）。

### D4 — 列表 API 計算欄位：item_count + total_amount

兩種做法：

| 方案 | 優點 | 缺點 |
|------|------|------|
| **A. EF Core 在 SQL 端 group by** | 性能好、單次 query | LINQ 寫起來囉嗦，但可接受 |
| B. C# 端 Sum / Count | 易寫 | N+1，列表性能差 |

選 **A**：透過 `_db.GroupOrders.Include(g => g.OrderItems).Select(...)` 投影到匿名物件，讓 EF 翻成 SQL `LEFT JOIN ... GROUP BY`。

實際寫法用 `Select` 投影並把 sum 做為 subquery：
```csharp
.Select(g => new {
  g.Id, g.Title, ...,
  ItemCount = g.OrderItems.Count(),
  TotalAmount = g.OrderItems.Sum(i => i.TotalPrice * i.Quantity)
})
```

EF Core 10 對 `Sum(x.A * x.B)` 已可正確翻譯（drink 其他列表已驗證過此模式）。

### D5 — Excel 匯出排序與檔名

**排序**：先 `RecipientName ASC`，再 `CreatedAt ASC`（單一 sheet，扁平列出所有飲料）。

**檔名**：`{title}_{yyyyMMdd}.xlsx`，title 內 OS-unsafe 字元（`\ / : * ? " < > |`）以底線替換。

**欄位**：填單人、收件人、品項、尺寸、甜度、冰塊、加料（多筆以「、」串接）、品項價、甜度加價、加料加價、小計、數量、實付金額、備註、建立時間。

**Why 單 sheet**：後台主要用途是給店家對單，扁平列表最易讀；分組顯示由前端負責。

### D6 — Entity 配置：Decimal 精度與 Index

| 欄位 | 配置 |
|------|------|
| `OrderItem.ItemPrice/SugarPrice/ToppingPrice/TotalPrice` | `decimal(10, 2)` — 與 `ShopMenuItemSize.Price` 等其他價格欄位一致 |
| `OrderItemTopping.Price` | `decimal(10, 2)` |
| `GroupOrder.Status` | int，建立 index 以便篩選快速 |
| `GroupOrder.ShopId` | FK，建立 index |
| `GroupOrder.InitiatorId` | FK，建立 index |
| `GroupOrder.Deadline` | 建立 index（列表常依此排序/篩選） |
| `OrderItem.GroupOrderId` | FK，建立 index |

無 unique constraint — 同一 user 可同時有多個揪團、同一收件人可在同一揪團點多杯。

### D7 — 列表 keyword 搜尋實作

搜尋目標：`Title` 或 `Initiator.Name` 含 keyword（case-insensitive、PostgreSQL `ILIKE`）。

```csharp
query = query.Where(g =>
  EF.Functions.ILike(g.Title, $"%{keyword}%") ||
  EF.Functions.ILike(g.Initiator.Name, $"%{keyword}%"));
```

**Why** 使用 `ILike` 而非 `Contains`：PostgreSQL 下 `Contains` 會翻成 `LIKE`（case-sensitive），不符 admin 列表通用搜尋語意。

### D8 — 通知 endpoint 回傳格式

```json
{
  "data": {
    "total_recipients": 5,
    "email_sent": 3,
    "push_skipped": 1,
    "none_skipped": 1,
    "failed": 0
  },
  "code": 0
}
```

讓後台 UI 能顯示「實際發送 X 封 email，N 位用戶 push 待 Firebase 啟用、N 位關閉通知」這類訊息。

## Risks / Trade-offs

- **[Risk] ClosedXML 在 .NET 10 / Linux container 的相容性** → ClosedXML 從 0.100+ 全面支援 .NET 6/7/8，.NET 10 應無問題；先以最新穩定版引入，CI 跑通即驗證
- **[Risk] EF 翻譯 `Sum(price * quantity)` 在巢狀條件下可能 fallback 到 client-side** → 列表 query 寫 unit-level test 或在 dev 啟用 `LogTo(Console.WriteLine)` 確認 SQL；若 fallback 改用 raw SQL projection
- **[Risk] 通知為 fire-and-forget，email send 失敗不應整支 API 失敗** → 在 `OrderNotificationService` 內 catch per-recipient exception，記 log 並累計到 `failed` 計數回傳；不 throw 給 controller
- **[Trade-off] Push 通道暫不實作會讓 NotificationType=WebPush 的用戶在發通知時不會收到** → 後台 UI 端顯示 `push_skipped` 計數讓管理員知悉；Firebase change 上線後既有資料無需 migration
- **[Trade-off] 不做通知歷史表** → 後台暫時看不到「這筆訂單通知過幾次、發給誰」；歷史紀錄留給後續 notification capability 處理，本期 email log 在 application log 中可查
- **[Risk] 後台沒做 Concurrency Token，多人同時改 Status 可能 race** → drink 全站目前不做樂觀鎖，本 change 也不引入；後台多人同時改狀態的場景極罕見，且狀態本身是 idempotent（最後寫入勝出）

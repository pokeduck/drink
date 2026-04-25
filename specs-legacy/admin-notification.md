# Spec: 後台通知管理 (Admin Notification)

## Objective
- 後台管理員查看系統發出的所有通知紀錄
- 提供兩種檢視維度：全部通知列表、以揪團分組的通知列表
- 支援重發單筆通知、批次重發
- 每筆通知記錄發送歷史，重發時追加歷史紀錄而非覆蓋

---

## Entities

### Notification（通知紀錄）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| GroupOrderId | int | ✅ | FK → GroupOrder |
| UserId | int | ✅ | FK → User（接收者） |
| Type | NotificationType (enum) | ✅ | 發送方式（WebPush / Email / Both），取自接收者當時的設定 |
| IsSuccess | bool | ✅ | 最近一次發送是否成功 |
| Content | string(500) | ✅ | 通知內容（如「下午茶揪起來 的飲料已送達」） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity），觸發者 UserId 或 AdminUserId |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### NotificationHistory（發送歷史）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| NotificationId | int | ✅ | FK → Notification |
| IsSuccess | bool | ✅ | 該次發送是否成功 |
| ErrorMessage | string(500) | ❌ | 失敗原因 |
| SentAt | DateTime | ✅ | 發送時間 |
| TriggeredBy | int | ✅ | 觸發者 ID（UserId 或 AdminUserId） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

---

## Relationships
- `Notification` → `GroupOrder`：多對一
- `Notification` → `User`：多對一（接收者）
- `Notification` → `NotificationHistory`：一對多
- 一次「發送通知」操作，對每位參與者各產生一筆 Notification + 一筆 NotificationHistory
- 重發時，更新 Notification.IsSuccess，並追加一筆 NotificationHistory

---

## Business Rules

### 通知產生
- 前台發起人或後台管理員手動觸發通知時：
  1. 取得揪團所有參與者（依 OrderItem.UserId 去重）
  2. 排除 NotificationType = None 的用戶
  3. 每位用戶產生一筆 Notification（Type = 用戶當時的 NotificationType）
  4. 每筆 Notification 同時產生一筆 NotificationHistory
- Notification.IsSuccess = 該次發送結果
- Notification.Content = 「{揪團 Title} 的飲料已送達」

### 重發
- 重發單筆：重新發送該 Notification，追加一筆 NotificationHistory
- 批次重發：多選後批次重發，每筆各追加一筆 NotificationHistory
- 重發時更新 Notification.IsSuccess 為最新一次結果
- 重發時 Notification.Updater = AdminUserId

### 發送方式
- WebPush：透過 Web Push API 發送
- Email：透過 Email 發送
- Both：同時發送 WebPush + Email，兩者皆成功才算 IsSuccess = true

---

## Code Style

```csharp
public class Notification : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int GroupOrderId { get; set; }
    public GroupOrder GroupOrder { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public NotificationType Type { get; set; }

    public bool IsSuccess { get; set; }

    [StringLength(500)]
    public string Content { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }

    public ICollection<NotificationHistory> Histories { get; set; }
}

public class NotificationHistory : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int NotificationId { get; set; }
    public Notification Notification { get; set; }

    public bool IsSuccess { get; set; }

    [StringLength(500)]
    public string? ErrorMessage { get; set; }

    public DateTime SentAt { get; set; }

    public int TriggeredBy { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}
```

---

## API Endpoints

### 通知列表（全部）
```
GET /api/admin/notifications?page=1&page_size=20&sort_by=created_at&sort_order=desc&keyword=wayne&is_success=false&group_order_id=1
```
- 遵循 SPEC.md 列表通用規範（分頁、排序、搜尋、篩選）
- keyword 搜尋欄位：user name, user email, content
- 篩選條件：is_success, group_order_id
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "group_order_id": 1,
        "group_order_title": "下午茶揪起來",
        "user_id": 2,
        "user_name": "Alice",
        "user_email": "alice@example.com",
        "type": 2,
        "is_success": true,
        "content": "下午茶揪起來 的飲料已送達",
        "send_count": 1,
        "created_at": "2025-03-15T16:00:00Z"
      }
    ],
    "total": 50,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 通知詳情（含發送歷史）
```
GET /api/admin/notifications/{notificationId}
```
- Response:
```json
{
  "data": {
    "id": 1,
    "group_order_id": 1,
    "group_order_title": "下午茶揪起來",
    "user_id": 2,
    "user_name": "Alice",
    "user_email": "alice@example.com",
    "type": 2,
    "is_success": true,
    "content": "下午茶揪起來 的飲料已送達",
    "created_at": "2025-03-15T16:00:00Z",
    "updated_at": "2025-03-15T16:00:00Z",
    "histories": [
      {
        "id": 1,
        "is_success": true,
        "error_message": null,
        "sent_at": "2025-03-15T16:00:00Z",
        "triggered_by": 1,
        "triggered_by_name": "Wayne"
      }
    ]
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 揪團通知列表（以揪團分組）
```
GET /api/admin/notifications/by-group-order?page=1&page_size=20&sort_by=created_at&sort_order=desc&keyword=下午茶
```
- keyword 搜尋欄位：group order title
- Response:
```json
{
  "data": {
    "items": [
      {
        "group_order_id": 1,
        "group_order_title": "下午茶揪起來",
        "shop_name": "50嵐",
        "notification_count": 5,
        "success_count": 4,
        "fail_count": 1,
        "last_sent_at": "2025-03-15T16:00:00Z"
      }
    ],
    "total": 10,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```
- 點擊某筆揪團後，導向通知列表頁並帶入 `group_order_id` 篩選

### 重發單筆通知
```
POST /api/admin/notifications/{notificationId}/resend
```
- 重新發送通知，追加 NotificationHistory
- 更新 Notification.IsSuccess 為最新結果

### 批次重發通知
```
POST /api/admin/notifications/resend
```
- Request Body:
```json
{
  "ids": [1, 2, 3]
}
```
- 批次重發，每筆各追加 NotificationHistory
- 各筆獨立處理，部分失敗不影響其他

---

## AdminMenu 變更

新增通知管理 Menu：
```csharp
// 通知管理
new AdminMenu { Id = 18, ParentId = null, Name = "通知管理", Icon = "Bell",         Endpoint = null,                        Sort = 6 },
new AdminMenu { Id = 19, ParentId = 18,   Name = "通知列表", Icon = "ChatDotRound", Endpoint = "/notification/list",        Sort = 1 },
new AdminMenu { Id = 20, ParentId = 18,   Name = "揪團通知", Icon = "ChatLineRound",Endpoint = "/notification/by-group",    Sort = 2 },
```

MenuConstants 新增：
```csharp
// 通知管理
public const int NotificationList = 19;
public const int NotificationByGroup = 20;
```

---

## Frontend（Admin）

### 頁面：通知列表
- 路徑：`/notification/list`
- el-table 顯示所有通知（揪團標題、接收者名稱、Email、發送方式、是否成功、發送次數、建立時間）
- 是否成功以 el-tag 顏色區分（成功=綠、失敗=紅）
- 操作欄：查看詳情、重發
- 篩選：is_success、group_order_id（可從揪團通知頁帶入）
- keyword 搜尋
- 支援多選（el-table selection），可批次重發
- 點擊查看詳情進入通知詳細頁

### 頁面：通知詳情
- 路徑：`/notification/list/:notificationId`
- 通知基本資訊（揪團、接收者、發送方式、狀態、內容）
- 發送歷史 el-table（發送時間、是否成功、錯誤訊息、觸發者）
- 操作：重發按鈕

### 頁面：揪團通知列表
- 路徑：`/notification/by-group`
- el-table 顯示揪團維度的通知統計（揪團標題、店家、通知數、成功數、失敗數、最後發送時間）
- 點擊某筆揪團 → 跳轉至通知列表頁，帶入 group_order_id 篩選
- keyword 搜尋揪團標題

---

## Success Criteria

### 通知列表
- [ ] `GET /api/admin/notifications` 支援分頁、排序、keyword 搜尋、篩選 is_success / group_order_id
- [ ] 列表顯示 send_count（發送歷史次數）

### 通知詳情
- [ ] `GET /api/admin/notifications/{notificationId}` 回傳通知資訊 + 發送歷史列表

### 揪團通知列表
- [ ] `GET /api/admin/notifications/by-group-order` 以揪團分組，顯示通知數 / 成功數 / 失敗數

### 重發
- [ ] `POST /api/admin/notifications/{notificationId}/resend` 重發單筆，追加 NotificationHistory
- [ ] `POST /api/admin/notifications/resend` 批次重發，各筆獨立處理
- [ ] 重發後更新 Notification.IsSuccess 為最新結果

### Entity
- [ ] 一次發送通知，每位參與者各一筆 Notification + 一筆 NotificationHistory
- [ ] 重發追加 NotificationHistory，不覆蓋原紀錄
- [ ] Notification、NotificationHistory 皆實作 ICreateEntity / IUpdateEntity

### AdminMenu
- [ ] 通知管理加入 AdminMenu Seed Data（Id 18, 19, 20）
- [ ] MenuConstants 新增 NotificationList = 19, NotificationByGroup = 20

---

## Boundaries

✅ Always:
- 每位參與者各一筆 Notification
- 重發追加 NotificationHistory，不覆蓋
- NotificationType = None 的用戶不產生 Notification
- 重發後更新 Notification.IsSuccess 為最新結果

⚠️ Ask First:
- 修改 Notification / NotificationHistory Entity 結構
- 修改通知內容格式
- 新增通知觸發時機

🚫 Never:
- 覆蓋或刪除 NotificationHistory
- 對 NotificationType = None 的用戶產生通知
- 在通知內容中包含敏感資訊

# Spec: 後台系統設定 (Admin System Setting)

## Objective
- 後台管理員管理平台全域設定，集中控制揪團預設行為、推播開關、系統公告與維護模式
- 設定項以 key-value 形式存儲，方便擴充
- 提供清除系統快取功能（非持久化設定，僅為操作按鈕）

---

## Entities

### SystemSetting（系統設定）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| Key | string(100) | ✅ | 設定鍵名（唯一），如 `group_order_default_deadline_minutes` |
| Value | string(500) | ✅ | 設定值（字串儲存，取用時依型別轉換） |
| Description | string(200) | ✅ | 設定說明，供後台顯示 |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

- Key 欄位加 Unique Index

---

## Seed Data

| Key | Value（預設值） | Description |
|-----|----------------|-------------|
| `group_order_default_deadline_minutes` | `120` | 揪團預設截止時間（分鐘） |
| `web_push_enabled` | `true` | Web Push 通知全域開關 |
| `announcement` | `` (空字串) | 系統公告內容（空字串 = 無公告） |
| `maintenance_mode` | `false` | 維護模式開關（開啟後前台顯示維護頁面） |

---

## Business Rules

### 設定讀寫
- 所有設定以 key-value 儲存，Value 統一為 string，由 API / 前端依 Key 的語意做型別轉換
- 更新設定時，僅更新 Value 欄位，Key 與 Description 不可由 API 修改
- 設定不可新增或刪除，僅能透過 Seed Data 管理項目清單

### 揪團預設截止時間
- `group_order_default_deadline_minutes`：前台建立揪團時，預設截止時間 = 當前時間 + 此設定值（分鐘）
- 用戶仍可在建立時自行調整截止時間
- 允許值：正整數，最小 30，最大 1440（24 小時）

### Web Push 開關
- `web_push_enabled`：控制 Web Push 通知全域開關
- `false` 時，所有 Web Push 發送跳過（包含 NotificationType = WebPush 與 Both）
- Email 發送不受此設定影響
- 更新此設定後立即生效，不需重啟

### 系統公告
- `announcement`：前台頂部顯示的公告內容
- 空字串 = 不顯示公告
- 支援純文字，不支援 HTML / Markdown
- 前台透過獨立 API 取得公告內容（不需登入）

### 維護模式
- `maintenance_mode`：開啟後前台顯示靜態維護頁面（單一頁面，無任何功能）
- 前台透過 `GET /api/announcement` 取得 `maintenance_mode` 狀態，為 `true` 時前端路由導向 `/maintenance` 靜態頁面
- 後端 User.API 加入 MaintenanceMiddleware，維護模式下所有 API 回傳 503 Service Unavailable（`GET /api/announcement` 除外）
- 後台（Admin.API）不受影響，管理員可正常操作
- 更新此設定後立即生效

### 清除系統快取
- 非持久化設定，為操作型功能
- 清除伺服器端所有 Memory Cache / Distributed Cache
- 呼叫後回傳成功即完成

---

## Code Style

```csharp
public class SystemSetting : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(100)]
    public string Key { get; set; }

    [StringLength(500)]
    public string Value { get; set; }

    [StringLength(200)]
    public string Description { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}
```

### Seed Data

```csharp
new SystemSetting { Id = 1, Key = "group_order_default_deadline_minutes", Value = "120",   Description = "揪團預設截止時間（分鐘）" },
new SystemSetting { Id = 2, Key = "web_push_enabled",                     Value = "true",  Description = "Web Push 通知全域開關" },
new SystemSetting { Id = 3, Key = "announcement",                         Value = "",      Description = "系統公告內容" },
new SystemSetting { Id = 4, Key = "maintenance_mode",                     Value = "false", Description = "維護模式開關" },
```

### SettingConstants

```csharp
public static class SettingConstants
{
    public const string GroupOrderDefaultDeadlineMinutes = "group_order_default_deadline_minutes";
    public const string WebPushEnabled = "web_push_enabled";
    public const string Announcement = "announcement";
    public const string MaintenanceMode = "maintenance_mode";
}
```

---

## API Endpoints

### 取得所有設定
```
GET /api/admin/system-settings
```
- 回傳所有設定項（不分頁，固定少量項目）
- Response:
```json
{
  "data": [
    {
      "id": 1,
      "key": "group_order_default_deadline_minutes",
      "value": "120",
      "description": "揪團預設截止時間（分鐘）",
      "updated_at": "2025-03-15T16:00:00Z"
    },
    {
      "id": 2,
      "key": "web_push_enabled",
      "value": "true",
      "description": "Web Push 通知全域開關",
      "updated_at": "2025-03-15T16:00:00Z"
    },
    {
      "id": 3,
      "key": "announcement",
      "value": "",
      "description": "系統公告內容",
      "updated_at": "2025-03-15T16:00:00Z"
    },
    {
      "id": 4,
      "key": "maintenance_mode",
      "value": "false",
      "description": "維護模式開關",
      "updated_at": "2025-03-15T16:00:00Z"
    }
  ],
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 更新設定
```
PUT /api/admin/system-settings
```
- Request Body（批次更新）：
```json
{
  "settings": [
    { "key": "group_order_default_deadline_minutes", "value": "60" },
    { "key": "web_push_enabled", "value": "false" }
  ]
}
```
- 驗證規則：
  - `group_order_default_deadline_minutes`：正整數，30 ≤ value ≤ 1440
  - `web_push_enabled`：`true` 或 `false`
  - `announcement`：長度 ≤ 500
  - `maintenance_mode`：`true` 或 `false`
  - 不存在的 key → 回傳 `SETTING_KEY_NOT_FOUND`
- Response:
```json
{
  "data": null,
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 清除系統快取
```
POST /api/admin/system-settings/clear-cache
```
- 清除伺服器端所有快取
- Response:
```json
{
  "data": null,
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 前台取得公告（公開 API）
```
GET /api/announcement
```
- 不需登入
- 維護模式下仍可存取
- Response:
```json
{
  "data": {
    "announcement": "系統將於 3/30 凌晨 2:00 進行維護",
    "maintenance_mode": false
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

---

## AdminMenu 變更

新增系統設定 Menu：
```csharp
// 系統設定
new AdminMenu { Id = 21, ParentId = null, Name = "系統設定", Icon = "Setting", Endpoint = "/system/setting", Sort = 7 },
```

MenuConstants 新增：
```csharp
// 系統設定
public const int SystemSetting = 21;
```

---

## Frontend（Admin）

### 頁面：系統設定
- 路徑：`/system/setting`
- 表單式頁面，非列表頁
- 各設定項依型別顯示不同元件：
  - 揪團預設截止時間：el-input-number（min=30, max=1440, step=30），單位顯示「分鐘」
  - Web Push 開關：el-switch
  - 系統公告：el-input textarea（maxlength=500, show-word-limit）
  - 維護模式：el-switch，開啟時顯示確認 el-dialog（「確定開啟維護模式？前台將暫停服務」）
- 清除系統快取：獨立按鈕（el-button danger），點擊後顯示確認 el-dialog
- 頁面底部「儲存」按鈕，一次送出所有設定變更

---

## Success Criteria

### Entity
- [ ] SystemSetting 實作 ICreateEntity / IUpdateEntity
- [ ] Key 欄位建立 Unique Index
- [ ] Seed Data 包含 4 筆預設設定

### API
- [ ] `GET /api/admin/system-settings` 回傳所有設定項
- [ ] `PUT /api/admin/system-settings` 批次更新設定，依 key 驗證 value 格式
- [ ] `POST /api/admin/system-settings/clear-cache` 清除伺服器端快取
- [ ] `GET /api/announcement` 公開 API，不需登入，維護模式下仍可存取

### Business Logic
- [ ] `web_push_enabled = false` 時，Web Push 發送跳過
- [ ] `maintenance_mode = true` 時，前台顯示靜態維護頁面（`/maintenance`），無任何功能
- [ ] User.API MaintenanceMiddleware 在維護模式下回傳 503（`GET /api/announcement` 除外）
- [ ] `announcement` 為空字串時前台不顯示公告
- [ ] 設定 Key 不可新增 / 刪除，僅能更新 Value

### AdminMenu
- [ ] 系統設定加入 AdminMenu Seed Data（Id 21）
- [ ] MenuConstants 新增 SystemSetting = 21

---

## Boundaries

✅ Always:
- 設定更新後立即生效
- 維護模式不影響後台操作
- User.API MaintenanceMiddleware 攔截所有請求回傳 503（公告 API 除外）
- 公告 API 不需登入
- 設定項目清單僅由 Seed Data 管理

⚠️ Ask First:
- 新增 SystemSetting Seed Data（新設定項）
- 修改 SystemSetting Entity 結構
- 修改維護模式的例外 API 清單

🚫 Never:
- 透過 API 新增或刪除設定 Key
- 在維護模式下放行公告 API 以外的前台請求
- 允許 announcement 包含 HTML / Script（XSS 風險）

# Spec: 後台驗證信管理 (Admin Verification)

## Objective
- 後台管理員查看前台用戶的驗證信發送紀錄（註冊驗證信、忘記密碼驗證信）
- 兩種驗證信分開管理，各自獨立子頁面
- 支援重發單筆、批量重發
- 會員詳情頁最下方顯示該用戶的歷史驗證信紀錄（兩種混合、readonly、分頁）
- 補充前台忘記密碼流程 API

---

## Entities

### VerificationEmail（驗證信紀錄）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| UserId | int | ✅ | FK → User |
| Type | VerificationEmailType (enum) | ✅ | 驗證信類型 |
| Token | string(200) | ✅ | 驗證 Token |
| ExpiresAt | DateTime | ✅ | Token 過期時間 |
| IsUsed | bool | ✅ | 是否已使用（已點擊驗證），預設 false |
| UsedAt | DateTime | ❌ | 使用時間 |
| IsSuccess | bool | ✅ | 發送是否成功 |
| ErrorMessage | string(500) | ❌ | 發送失敗原因 |
| SentAt | DateTime | ✅ | 發送時間 |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity），前台觸發 = 0，後台重發 = AdminUserId |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### VerificationEmailType (Enum)
```csharp
public enum VerificationEmailType
{
    Register = 1,       // 註冊驗證信
    ForgotPassword = 2  // 忘記密碼驗證信
}
```

---

## Relationships
- `VerificationEmail` → `User`：多對一
- 每次發送（含重發）各自獨立一筆 VerificationEmail 紀錄
- 重發時產生新的 Token，舊紀錄不作廢（但舊 Token 過期後自然失效）

---

## Business Rules

### 註冊驗證信
- 前台用戶註冊後，系統自動發送一筆註冊驗證信
- 每次發送產生獨立的 VerificationEmail 紀錄（Type = Register）
- Token 有效期限：24 小時
- 用戶點擊驗證連結後：IsUsed = true, UsedAt = 當前時間，User.EmailVerified = true
- 驗證時以最新一筆未過期、未使用的 Token 為準，舊 Token 仍可使用（只要未過期且未使用）

### 忘記密碼驗證信
- 前台用戶請求忘記密碼，系統發送一筆忘記密碼驗證信
- 每次發送產生獨立的 VerificationEmail 紀錄（Type = ForgotPassword）
- Token 有效期限：1 小時
- 用戶點擊連結後導向重設密碼頁，提交新密碼時：IsUsed = true, UsedAt = 當前時間
- 重設密碼成功後撤銷該用戶所有 UserRefreshToken，強制重新登入

### 後台重發
- 管理員可重發單筆驗證信：產生一筆新的 VerificationEmail（新 Token），發送給該用戶
- 批量重發：多選後批次重發，每筆各產生一筆新紀錄
- 重發時依原紀錄的 Type 決定發送哪種驗證信
- 重發限制：同一用戶、同一類型，10 分鐘內最多重發 1 次（避免濫發）
- 只能重發給 EmailVerified = false 的用戶（註冊驗證信），或不限制（忘記密碼驗證信）

### 發送失敗
- 發送失敗時 IsSuccess = false，ErrorMessage 記錄失敗原因
- 失敗的紀錄可被重發

---

## Code Style

```csharp
public class VerificationEmail : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int UserId { get; set; }
    public User User { get; set; }

    public VerificationEmailType Type { get; set; }

    [StringLength(200)]
    public string Token { get; set; }

    public DateTime ExpiresAt { get; set; }

    public bool IsUsed { get; set; }

    public DateTime? UsedAt { get; set; }

    public bool IsSuccess { get; set; }

    [StringLength(500)]
    public string? ErrorMessage { get; set; }

    public DateTime SentAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public enum VerificationEmailType
{
    Register = 1,
    ForgotPassword = 2
}
```

---

## API Endpoints

### 前台 — 忘記密碼（補充 user-auth）

#### 請求忘記密碼
```
POST /api/user/auth/forgot-password
```
- Request Body:
```json
{
  "email": "wayne@example.com"
}
```
- 不論 Email 是否存在，皆回傳成功（防止帳號探測）
- Email 存在且 Status = Active 時，發送忘記密碼驗證信
- 產生一筆 VerificationEmail（Type = ForgotPassword，Token 有效 1 小時）
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

#### 重設密碼
```
POST /api/user/auth/reset-password
```
- Request Body:
```json
{
  "token": "reset-token-string",
  "new_password": "newSecret123"
}
```
- Token 無效或過期回傳 400（`INVALID_TOKEN`）
- Token 已使用回傳 400（`TOKEN_ALREADY_USED`）
- 成功後：
  - 更新 User.PasswordHash（Argon2id）
  - VerificationEmail.IsUsed = true, UsedAt = 當前時間
  - 撤銷該用戶所有 UserRefreshToken，強制重新登入
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

---

### 後台 — 註冊驗證信列表

#### 列表
```
GET /api/admin/verifications/register?page=1&page_size=20&sort_by=sent_at&sort_order=desc&keyword=wayne&is_success=true&is_used=false
```
- 遵循 SPEC.md 列表通用規範（分頁、排序、搜尋、篩選）
- 固定篩選 Type = Register
- keyword 搜尋欄位：user name, user email
- 篩選條件：is_success, is_used
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "user_id": 2,
        "user_name": "Alice",
        "user_email": "alice@example.com",
        "is_success": true,
        "is_used": false,
        "sent_at": "2025-03-15T16:00:00Z",
        "expires_at": "2025-03-16T16:00:00Z"
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

#### 重發單筆
```
POST /api/admin/verifications/{verificationId}/resend
```
- 產生一筆新的 VerificationEmail（新 Token），發送給該用戶
- 重發限制：同一用戶、同一類型，10 分鐘內最多重發 1 次
- 違反限制回傳 429（`RESEND_TOO_FREQUENT`）
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

#### 批量重發
```
POST /api/admin/verifications/register/resend
```
- Request Body:
```json
{
  "ids": [1, 2, 3]
}
```
- 每筆各產生新的 VerificationEmail，各自獨立處理
- 部分失敗不影響其他（如某用戶 10 分鐘內已重發過則跳過）
- Response:
```json
{
  "data": {
    "success_count": 2,
    "skip_count": 1,
    "skipped_ids": [3]
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

---

### 後台 — 忘記密碼驗證信列表

#### 列表
```
GET /api/admin/verifications/forgot-password?page=1&page_size=20&sort_by=sent_at&sort_order=desc&keyword=wayne&is_success=true&is_used=false
```
- 固定篩選 Type = ForgotPassword
- 其餘同註冊驗證信列表
- Response 格式同註冊驗證信列表

#### 重發單筆
```
POST /api/admin/verifications/{verificationId}/resend
```
- 同註冊驗證信重發（共用同一 API）

#### 批量重發
```
POST /api/admin/verifications/forgot-password/resend
```
- Request Body 與 Response 格式同註冊驗證信批量重發

---

### 後台 — 會員詳情頁驗證信紀錄

#### 取得會員驗證信紀錄
```
GET /api/admin/members/{memberId}/verifications?page=1&page_size=20&sort_by=sent_at&sort_order=desc
```
- 回傳該用戶所有驗證信紀錄（Register + ForgotPassword 混合）
- 分頁（預設 page_size = 20，遵循全站統一預設值）
- readonly，不提供操作
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "type": 1,
        "is_success": true,
        "is_used": true,
        "used_at": "2025-03-15T16:05:00Z",
        "sent_at": "2025-03-15T16:00:00Z",
        "expires_at": "2025-03-16T16:00:00Z",
        "error_message": null
      },
      {
        "id": 5,
        "type": 2,
        "is_success": true,
        "is_used": false,
        "used_at": null,
        "sent_at": "2025-03-20T10:00:00Z",
        "expires_at": "2025-03-20T11:00:00Z",
        "error_message": null
      }
    ],
    "total": 3,
    "page": 1,
    "page_size": 10
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

---

## AdminMenu 變更

原 Id = 6「驗證信列表」拆分為兩個子頁面：
```csharp
// 會員管理（修改 Id 6，新增 Id 22）
new AdminMenu { Id = 6,  ParentId = 4,  Name = "註冊驗證信",     Icon = "Message",     Endpoint = "/member/verification/register",        Sort = 2 },
new AdminMenu { Id = 22, ParentId = 4,  Name = "忘記密碼驗證信", Icon = "EditPen",     Endpoint = "/member/verification/forgot-password",  Sort = 3 },
```

MenuConstants 修改 / 新增：
```csharp
// 會員管理
public const int MemberList = 5;
public const int VerificationRegister = 6;       // 原 VerificationList，改名
public const int VerificationForgotPassword = 22; // 新增
```

---

## Frontend（Admin）

### 頁面：註冊驗證信列表
- 路徑：`/member/verification/register`
- el-table 顯示註冊驗證信紀錄（用戶名稱、Email、發送狀態、是否已使用、發送時間、過期時間）
- 發送狀態以 el-tag 顏色區分（成功=綠、失敗=紅）
- 是否已使用以 el-tag 區分（已使用=藍、未使用=灰）
- 操作欄：重發
- 篩選：is_success, is_used
- keyword 搜尋
- 支援多選（el-table selection），可批量重發

### 頁面：忘記密碼驗證信列表
- 路徑：`/member/verification/forgot-password`
- 格式同註冊驗證信列表
- 固定篩選 Type = ForgotPassword

### 會員詳情頁（修改現有頁面）
- 在會員詳情頁最下方新增「驗證信紀錄」區塊
- el-table 顯示該用戶所有驗證信（類型、發送狀態、是否已使用、使用時間、發送時間、過期時間、錯誤訊息）
- 類型以 el-tag 區分（註冊=藍、忘記密碼=橘）
- readonly，無操作欄
- 分頁，預設 page_size = 20

---

## Success Criteria

### Entity
- [ ] VerificationEmail 實作 ICreateEntity / IUpdateEntity
- [ ] VerificationEmailType enum 包含 Register = 1, ForgotPassword = 2
- [ ] 每次發送（含重發）產生獨立一筆 VerificationEmail 紀錄

### 前台忘記密碼
- [ ] `POST /api/user/auth/forgot-password` 不論 Email 是否存在皆回傳成功
- [ ] Email 存在且 Status = Active 時發送忘記密碼驗證信，Token 有效 1 小時
- [ ] `POST /api/user/auth/reset-password` 驗證 Token，成功後更新密碼、撤銷所有 UserRefreshToken
- [ ] Token 無效 / 過期 / 已使用分別回傳對應錯誤碼

### 後台列表
- [ ] `GET /api/admin/verifications/register` 支援分頁、排序、keyword 搜尋、篩選 is_success / is_used
- [ ] `GET /api/admin/verifications/forgot-password` 同上，固定 Type = ForgotPassword

### 重發
- [ ] `POST /api/admin/verifications/{verificationId}/resend` 產生新紀錄（新 Token）發送
- [ ] `POST /api/admin/verifications/register/resend` 批量重發，各筆獨立處理
- [ ] `POST /api/admin/verifications/forgot-password/resend` 批量重發
- [ ] 同一用戶、同一類型，10 分鐘內最多重發 1 次，違反回傳 429

### 會員詳情頁
- [ ] `GET /api/admin/members/{memberId}/verifications` 回傳該用戶所有驗證信紀錄（混合兩種類型）、分頁

### AdminMenu
- [ ] Id = 6 改為「註冊驗證信」，Endpoint 更新
- [ ] 新增 Id = 22「忘記密碼驗證信」
- [ ] MenuConstants 更新

---

## Boundaries

✅ Always:
- 每次發送獨立一筆紀錄，不覆蓋舊紀錄
- 忘記密碼不論 Email 是否存在皆回傳成功（防帳號探測）
- 重設密碼後撤銷所有 UserRefreshToken
- 密碼以 Argon2id 雜湊存儲（Salt + Pepper）
- 重發頻率限制：同用戶同類型 10 分鐘內最多 1 次

⚠️ Ask First:
- 修改 VerificationEmail Entity 結構
- 修改 Token 有效期限
- 修改重發頻率限制
- 新增 VerificationEmailType enum 值

🚫 Never:
- 刪除 VerificationEmail 紀錄
- 在 API Response 或日誌中暴露 Token 值
- 允許 Status = Inactive 的用戶觸發忘記密碼流程的實際發送

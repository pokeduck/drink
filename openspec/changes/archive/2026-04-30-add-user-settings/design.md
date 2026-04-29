## Context

### 現有可重用資產

設定 / 收藏 / 上傳所需的後端基礎設施大多已就緒：

| 資產 | 狀態 |
|------|------|
| Upload.API 完整 pipeline（驗證、SkiaSharp、resize、WebP、SHA-256 dedup、API Key） | ✅ |
| `FileUploadService` 含 `ResizeIfNeeded` 邏輯，目前 hardcode `MaxDimension = 4000` | ✅ |
| User.API `UploadController` 已 proxy 到 Upload.API（`/api/user/upload`） | ✅ |
| Client `useImageUploadQueue` composable | ✅ |
| `IPasswordHasher` (Argon2id + Pepper)、`UserAuthService.GenerateAndSaveTokens` / `RevokeAllTokens` 私有 helper | ✅ |
| Admin 端 `ChangePassword` 範例實作可參考 | ✅ |

### Profile 頁現狀（mockup）

`web/apps/client/app/pages/profile.vue` 目前混合了三類東西：
- ✅ 真實：name / email / email_verified / google_connected / avatar / 編輯表單
- ❌ Mockup：Orders Placed `12` / Savings `$350`（hardcode）
- ❌ Dead end：4 個 menu icon (Wallet / Notifications / Security / Settings) 點下去都是同一頁

入口層面則有 4 條路徑都通往 `/profile`：desktop nav、bottom nav、avatar dropdown「Profile」、avatar dropdown「Settings」（後者是同一頁）— 必須整理。

## Goals / Non-Goals

**Goals:**
- 建立 `/profile`（read-only）、`/settings`（可編輯）、`/favorites`（mockup）三頁的清晰職責分工
- Avatar dropdown 從「重複的頁面連結 hub」轉為「動詞 only」（偏好設定 / 我的收藏 / 登出）
- 提供「變更密碼」「登出所有裝置」兩個自助操作
- 提供 avatar 上傳體驗：拖拉 / 點選 → 預覽 → 儲存才寫入；後端自動壓縮成 max 長邊 512px
- 為未來「真實 favorites」與「真實 stats」預留前端版面（mockup 階段先放 placeholder）

**Non-Goals:**
- 真實 favorites entity（依賴店家瀏覽 capability，獨立 change）
- 真實 stats 數據（依賴揪團 / 訂單 capability，獨立 change）
- 多尺寸 srcset（avatar 一份壓縮版本足夠涵蓋目前所有使用情境）
- 外部 URL 頭像（已決定移除）
- 帳號刪除、Email 變更、飲料預設、avatar 歷史紀錄

## Decisions

### Decision 1: 三頁職責分工

**選擇**：

| 頁 | 職責 | 是否可編輯 |
|----|------|----------|
| `/profile` | 「你是誰、你做過什麼」展示頁 | ❌ |
| `/settings` | 「調整你的體驗」單一可編輯入口 | ✅ 全部 |
| `/favorites` | 「你收藏了哪些店家 / 飲料」清單 | ⚠ mockup 階段顯示假資料 |

**理由**：
- 之前 profile 同時是展示又是編輯入口造成混亂（齒輪 + 4 個 menu icon + dropdown 各種重複）
- 把編輯動作集中到一頁讓 future settings 項目擴充時不用再煩惱「該放哪」
- Favorites 獨立頁讓未來真實 favorites 直接接通即可，不用再拆

**Alternatives considered**：
- 把 settings 嵌進 profile 內 tab：mockup 期看似 OK，但 settings 內容會持續長（密碼 / 通知 / 主題 / 飲料預設 / 連結帳號⋯⋯），tab 撐不住
- 不做獨立 `/favorites`，把 favorites 列表放 profile：profile 會變成新的「四不像」

### Decision 2: Avatar dropdown 改為「動詞 only」

**選擇**：dropdown 內容改為

```
Header (display only):  username
─────────
偏好設定 → /settings
我的收藏 → /favorites
─────────
登出
```

移除原本的 Profile / My Orders 連結（main nav 已有），移除原本指向 `/profile` 的 Settings 假連結。

**理由**：消除 4 條路徑通往 `/profile` 的重複；dropdown 用作「跨頁快捷動作」而非「頁面目錄」。

### Decision 3: Avatar 上傳採新 endpoint，max 長邊 512px

**選擇**：

- Upload.API 新增 `POST /api/upload/avatar`（不修改現有 `/api/upload`，避免影響 admin shop image 流程）
- `FileUploadService.Upload()` 重構成接收 `int maxDimension` 參數，現有 endpoint 傳 4000、新 avatar endpoint 傳 512
- User.API 新增 `POST /api/user/upload/avatar` proxy
- 前端用 `useImageUploadQueue` 上傳，拿到 URL 後**僅顯示預覽**

**理由**：
- 256px 顯示 + retina 2x = 512px 已涵蓋目前所有 avatar 使用情境（header 40 / 各處 64 / profile 256）
- 新 endpoint 比 query 參數更明確（`?mode=avatar` 容易被人誤用、spec 也比較難寫）
- 沿用 SkiaSharp / EXIF 清除 / WebP / dedup 全部既有邏輯，僅 max dimension 不同
- 保留 4000px endpoint 給 admin shop image（菜單照片要更大）

**Alternatives considered**：
- 多尺寸 srcset：本階段過度設計
- 沿用 4000px：avatar 檔案 ~200-500KB，違反「適配前端大小」需求

### Decision 4: 拖拉上傳採「預覽 → 儲存才寫入」

**選擇**：

```
拖拉 / 點選檔案
   │
   ▼
POST /api/user/upload/avatar  ← 立即上傳到 Upload.API
   │
   ▼
回傳 url，前端顯示預覽（但 User.Avatar 還沒變）
   │
   ▼
使用者按「儲存」 ──────► PUT /api/user/profile { avatar: url }
   │                          ↓
   └─►「取消」就放棄這個 url（檔案仍在 Upload.API，dedup 會處理）
```

**理由**：避免「拖錯圖直接生效，要再傳一次」的痛點。

**副作用**：取消的圖會留在 Upload.API 但 `User.Avatar` 未指向它，靠 SHA-256 dedup 控制重覆，未來如果有清理需求另起 cleanup job — 不在本次 scope。

### Decision 5: 「變更密碼」採 inline 表單，且**不**自動撤銷 token

**選擇**：在 `/settings` 頁的 Account section 內展開 3 欄表單（舊密碼 / 新密碼 / 確認新密碼 + 儲存按鈕），不另開 `/settings/change-password`。變更成功**不撤銷**任何 refresh token，session 保持原狀；如使用者擔心其他裝置安全，自行按下方「登出所有裝置」。

**理由**：
- admin 端是另開頁因為 admin 那邊還有「強制改密碼」「忘記密碼」等情境，user 這邊只有單一情境，inline 即可
- 「踢出全部 session」是獨立動作（settings 已有「登出所有裝置」按鈕），不應該被「變更密碼」隱含觸發；耦合在一起會造成「使用者在自己機器上換密碼但被自己踢出」的常見情境，UX 不佳
- 安全考量：admin 端權限大保留撤銷設計；user 端權限相對小，將決策交給使用者

**業務規則**：變更密碼成功後 MUST NOT 撤銷任何 refresh token；前端 MUST NOT 主動 redirect / clearTokens；MUST 清空密碼表單欄位 + 顯示「密碼已變更」toast。UI MUST 在區塊內提示「本機現有 session 不會被中斷；若需踢出其他裝置請使用『登出所有裝置』」，讓使用者明確知道差異。

### Decision 6: 「登出所有裝置」獨立 endpoint

**選擇**：新增 `POST /api/user/auth/logout-all`（需 `[Authorize]`），呼叫 `RevokeAllTokens(CurrentUserId)`，回 200。前端收到成功後呼叫 `authStore.clearTokens()` 並導回 `/login`。

**理由**：與「變更密碼附帶撤銷」邏輯不同，這個是顯式動作；獨立 endpoint 比較清楚，且未來可加上「保留當前裝置」這類選項。

### Decision 7: Stats 與 Favorites 採 mockup 顯示

**選擇**：

- Stats 4 卡片寫死成預告未來樣貌：「本月花費 NT$ 480」、「累計揪團 12」、「Top Shop 五十嵐」、「Last Drink 蜂蜜檸檬綠（半糖少冰）」
- Favorites 區塊寫死 5 家店、3 杯飲料；標題旁加 [編輯] 按鈕指向 `/favorites`
- `/favorites` 頁的列表也是 mockup；移除按鈕顯示 toast「Mockup 階段尚未支援」

**理由**：等真實 stats / favorites capability 完成時，把 mockup 換真實資料，版型不用改。

### Decision 8: 頁面職責 vs 入口路徑表

```
                 main nav    bottom nav    avatar drop    profile 內
/profile         ✓           ✓             ✗             ─
/settings        ✗           ✗             ✓ (偏好設定)   ✓ (齒輪)
/favorites       ✗           ✗             ✓ (我的收藏)   ✓ (編輯按鈕)
```

每頁有獨立、不重複的入口；`/settings` 與 `/favorites` 因為從 `/profile` 內也有捷徑，仍維持單一語意。

## Risks / Trade-offs

- **[Mockup stats / favorites 顯示假資料造成使用者誤會]** → UI 上加「Mockup data — coming soon」標籤；favorites 頁底部明確說明「真實功能將於店家瀏覽功能上線後啟用」
- **[拖拉上傳但取消後檔案殘留 Upload.API]** → 由 SHA-256 dedup 控制重覆；未來如有清理需求另起 cleanup job
- **[變更密碼後使用者沒被踢出]** → 後端 `RevokeAllTokens` + 前端強制 logout，雙重保險
- **[Avatar 512px 不夠未來高清螢幕]** → 將來真要 srcset 時，FileUploadService 已支援注入式 maxDimension，可以再加新 endpoint 不影響舊資料
- **[移除 dropdown 的 Profile 連結後使用者不知道怎麼進 profile]** → main nav 與 bottom nav 都仍有「個人檔案」連結，覆蓋桌機與行動兩種情境

## Migration Plan

純 additive，不需要 DB migration、不需要既有資料轉換。部署順序：

1. 後端先部署：新 endpoints 上線，舊 client 不受影響
2. 前端部署：`/settings` / `/favorites` / 重構的 `/profile` 一起上
3. 既有使用者打開 app：
   - 若停在 `/profile` → 看到新版（read-only + mockup 區塊）
   - 點齒輪 / 偏好設定 → 進 `/settings`
   - dropdown 不再有 My Orders（main / bottom nav 都有，不影響可達性）

Rollback：純前端 / 純後端各自獨立可 rollback。

## Open Questions

無。範圍與決策都已在 explore 階段對齊。

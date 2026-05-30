## Why

目前 `auth.global` middleware 只讓 `/login`、`/register`、`/verify-email` 三條路徑公開，導致**所有實際使用價值的頁面（首頁揪團牆、揪團詳情）對未登入使用者都不可見**，迫使想看一眼產品再決定要不要註冊的訪客直接被擋在 `/login` 牆外。但揪團牆與單筆揪團這類「公開內容」其實沒有理由要強制登入 — 看完內容、想加入時再要求登入即可（业界標準的「登入牆放在動作上、不放在頁面上」模式）。

同時目前 dropdown 的 guest 版本與 logged-in 版本結構差異很大（guest 有「登入 + 註冊」、logged-in 有「偏好設定 + 我的收藏 + 登出」），不一致；想看「偏好設定」入口、發現自己沒登入的訪客需要回 dropdown 切換思路重新點「登入」，多繞一圈。

## What Changes

### Middleware（`auth.global.ts`）

- 公開路徑擴大：除原本三項外，新增 **`/`（首頁）** 與 prefix **`/group/`（揪團詳情）** 為 public
- 受保護頁面在被未登入使用者訪問時 MUST 加上 `?redirect=<encoded path>` query，讓 `/login` 在登入後能導回原本想去的地方
- 已登入使用者訪問 `/login` / `/register` 仍然導回 `/`（不變）

### `/login` 頁

- 讀 `useRoute().query.redirect`，登入成功後 `navigateTo(redirect ?? '/')`
- 註冊連結保持在表單下方（已存在），不再放進 dropdown

### Avatar Dropdown 統一結構（`layouts/default.vue`）

```
已登入                       未登入（新）
─────                       ─────
Wayne                        Guest
─────                        ─────
偏好設定 → /settings           偏好設定 → /settings  (NuxtLink，middleware 攔)
我的收藏 → /favorites          我的收藏 → /favorites (NuxtLink，middleware 攔)
─────                        ─────
登出                          登入 → /login
```

- 移除 guest dropdown 內的「註冊」連結（仰賴 `/login` 頁底部的「建立新帳號」連結）

### `/group/[id]` Guest gating

- 整頁變為 public（middleware 不再擋）
- **Host Controls** 區塊加 `authStore.isLoggedIn` 判斷，guest 永不顯示（避免 mock data 導致誤判）
- **Order Modal「Place Order」按鈕**：guest 點擊時 — 按鈕標籤改為「登入後下單」+ click 直接 `navigateTo('/login?redirect=/group/{id}')`

### 不在此範圍

- 商家搜尋頁（`/shops`）— 待後續 capability
- 真實 shop / group API（仍 mock）
- `/register` 頁的 redirect query 支援（多步驟驗證流程不直接適用）

## Capabilities

### New Capabilities

無。

### Modified Capabilities

- `client-layout`：全域認證守衛 + User Avatar Dropdown 兩條 requirement 變更
- `client-pages`：Group Detail 頁面 guest 行為條款新增

## Impact

### Affected code

- `web/apps/client/app/middleware/auth.global.ts`：白名單擴大、prefix match、redirect query
- `web/apps/client/app/pages/login.vue`：讀 redirect query
- `web/apps/client/app/layouts/default.vue`：Guest dropdown 結構重組
- `web/apps/client/app/pages/group/[id].vue`：Host Controls 加 isLoggedIn、Place Order guest 替換按鈕

### Dependencies

無新增；無 codegen；無後端動作；無 DB migration。

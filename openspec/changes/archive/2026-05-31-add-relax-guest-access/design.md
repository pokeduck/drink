## Context

`add-user-auth` 完成時設定的 middleware 是「預設要登入、列舉公開路徑」模式：

```ts
const publicPaths = ['/login', '/register', '/verify-email']

if (publicPaths.includes(to.path)) {
  // public
} else if (!authStore.isLoggedIn) {
  return navigateTo('/login')
}
```

當時目的是把 client 設定為「全部要登入」的安全預設，但實際使用後發現對訪客的探索體驗太差 —— 揪團牆和單筆揪團詳情其實是公開內容，業界一般做法是「登入牆放在動作（加入 / 下單）上，不擋瀏覽」。

同時 dropdown 在 guest 模式呈現「登入 + 註冊」、logged-in 模式呈現「偏好設定 + 我的收藏 + 登出」，結構差異大、認知負擔高。

## Goals / Non-Goals

**Goals:**
- 訪客可在不登入的情況下瀏覽首頁、揪團詳情，理解產品價值
- 觸發實際動作（發起揪團、加入揪團、查看個人頁面）才要求登入
- 登入後自動導回原本要去的頁面，不打斷流程
- Dropdown 結構在 guest / logged-in 兩種狀態下一致（只差最後一個按鈕）

**Non-Goals:**
- 提供 SSR / SEO 優化（client 仍 SPA，純 client-side gating）
- 商家搜尋頁
- `/register` 流程的 redirect 支援（多步驟流程，要等 verify-email → login 才生效，邏輯複雜且使用情境少）
- 在 in-page action 上做更精緻的 modal 提示（本次選擇最簡單的「按鈕變身為連結」模式）

## Decisions

### Decision 1: 「登入牆放在動作、不放頁面」

**選擇**：把 `/`、`/group/[id]` 解封為 public；登入要求改放在 in-page action（Place Order button）。

**理由**：
- 訪客瀏覽公開內容是「無風險動作」，沒理由擋
- 揪團牆是分享連結的入口（`https://app/group/abc123`），擋登入會破壞分享邏輯
- 觸發動作（建立 / 加入）才會產生 side effect，才需要 identity

### Decision 2: middleware 採 publicPaths + publicPrefixes 雙白名單

**選擇**：

```ts
const publicPaths = ['/', '/login', '/register', '/verify-email']
const publicPrefixes = ['/group/']

const isPublic = publicPaths.includes(to.path)
  || publicPrefixes.some(p => to.path.startsWith(p))
```

**理由**：
- `publicPaths` 涵蓋固定路徑
- `publicPrefixes` 涵蓋動態路由（`/group/[id]` → `/group/abc123`）
- 易於擴充：未來新增 `/shops/[id]` 之類的詳情頁直接加 prefix

**Alternatives considered**：
- Regex / minimatch — 過度工程，目前情境兩種匹配模式夠用
- meta 標記每個 page 自己宣告 `requiresAuth`：散落在 page 內，全域可見性差

### Decision 3: Redirect query 機制

**選擇**：

```ts
// middleware 受保護頁面被攔截時
const redirect = encodeURIComponent(to.fullPath)
return navigateTo(`/login?redirect=${redirect}`)

// /login 頁登入成功後
const target = (route.query.redirect as string) || '/'
await navigateTo(target)
```

**理由**：
- 標準業界模式（GitHub / Slack / Notion 等都這樣）
- `to.fullPath` 包含 query / hash，比 `to.path` 完整
- `decodeURIComponent` 由 Vue Router 自動處理（`route.query.redirect` 已 decoded）

**安全考量**：
- `redirect` 應限制為相對路徑（`/...` 開頭），避免被當 open redirect（攻擊者塞 `?redirect=https://evil.com`）
- 實作時 MUST 驗證 `redirect.startsWith('/')` 且 `!redirect.startsWith('//')`（後者是 protocol-relative URL）

### Decision 4: Guest dropdown 對齊 logged-in 結構

**選擇**：

```
Guest:  [Guest header] / 偏好設定 / 我的收藏 / 登入
Login:  [name header]  / 偏好設定 / 我的收藏 / 登出
```

兩個版本結構鏡像，唯一差異：
- header 文字（Guest vs username）
- 末位按鈕（登入 vs 登出）

**理由**：
- 訪客也能看到「偏好設定」「我的收藏」入口，被 middleware 帶進登入頁、登入後自動回到原意圖
- 視覺上 dropdown 不會「狀態突變」，使用者切換登入登出時 affordance 一致
- 「註冊」入口從 dropdown 移到 `/login` 頁底部（已存在），dropdown 不囉唆

### Decision 5: In-page action 的 guest gating 模式

**選擇**：對於需要登入的 in-page action button（如 Place Order），未登入時把按鈕本身**改成「登入提示 + 連結」**。

```vue
<button v-if="authStore.isLoggedIn" @click="handlePlaceOrder">Place Order</button>
<NuxtLink
  v-else
  :to="`/login?redirect=${encodeURIComponent(currentPath)}`"
  class="brutalist-button brutalist-button-primary ..."
>
  登入後下單
</NuxtLink>
```

**理由**：
- 一個 affordance 既是「提示」也是「連結」，不用 toast / modal 兩階段
- 按鈕視覺位置不變，不破壞版面
- 登入完直接回到原頁，使用者繼續流程

**Alternatives considered**：
- 點下去先 toast「請先登入」再 setTimeout navigateTo：兩段體驗、使用者先不知道為什麼按不動
- Modal「需要登入才能下單」+ 確認鈕：mockup 階段太重

### Decision 6: Host Controls 看不見 vs middleware 攔截

**選擇**：`/group/[id]` 整頁 public，但內部 Host Controls 區塊用 `v-if="isHost && authStore.isLoggedIn"` 守。

**理由**：
- 目前 `isHost` 由 mock data 算（`group.hostId === currentUser.id`），對 guest 容易誤判
- 即使將來接真實 API，guest 的 `currentUser` 為 null，`isHost` 自然 false，但加上 `isLoggedIn` 雙保險、語意清楚

## Risks / Trade-offs

- **[Open redirect 攻擊]** → middleware 與 `/login` 都 MUST 驗證 redirect 為相對路徑（`/...` 開頭、不以 `//` 開頭），否則 fallback 到 `/`
- **[Guest 看 mockup data]** → 揪團牆 / 詳情都是 mockup，沒有風險；真實 API 接通後同樣是公開內容
- **[Guest 看到看似可點的按鈕但其實不能用]** → Place Order 按鈕直接改成「登入後下單」，行為與外觀對應
- **[`/register` 沒有 redirect 支援]** → 註冊流程包含 verify email 多步驟，redirect query 在這條 path 上要傳遞跨多頁，本次明確不做。使用者註冊完仍走「verify → login」標準流程

## Migration Plan

純前端，三個檔案改動，無 DB / 後端動作。直接 deploy：

1. 部署後 guest 第一次訪問 `/`：直接看到揪團牆（之前是被導 `/login`）
2. 既有 logged-in 使用者：行為完全不變
3. Rollback：純前端 git revert 即可

## Open Questions

無。

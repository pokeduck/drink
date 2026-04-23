## Why

前台 Nuxt client app（`web/apps/client`）目前只有 starter 模板，尚無任何業務頁面。團隊已有一套由 AI Studio 產生的 React UI 模板（`web/template-ui`），包含完整的 Brutalist 風格設計系統和 6 個核心頁面。需要將這套 UI 1:1 翻譯為 Nuxt 4 + Tailwind CSS，作為前台的 UI 基礎。

## What Changes

- **移除 `@nuxt/ui`**：拆掉 Nuxt UI 依賴，改為純 Tailwind CSS 手刻所有元件
- **新增 Brutalist 設計系統**：將 React 版的 CSS 設計系統（brand color、brutalist-card、brutalist-button、dark mode tokens）移植到 `main.css`
- **新增依賴**：`lucide-vue-next`（icon）、`@nuxtjs/color-mode`（dark mode）、`@nuxtjs/google-fonts`（Inter + Space Grotesk）、`@vueuse/core`（onClickOutside 等）
- **新增 Layout**：手刻 Header（Logo + Nav + Dark mode toggle + User dropdown）+ Mobile Bottom Nav + Desktop Footer
- **新增 6 個頁面**：
  - `pages/index.vue` — 首頁（活動揪團列表 + 歷史）
  - `pages/create.vue` — 建立揪團（店家選擇 Modal + 表單）
  - `pages/group/[id].vue` — 揪團詳情 + 菜單 + 點餐 Modal
  - `pages/my-orders.vue` — 我的訂單列表
  - `pages/profile.vue` — 個人頁面
  - `pages/admin/[id].vue` — 團主管理面板（進度追蹤 + 付款管理）
- **新增共用元件**：`GroupCard.vue`（揪團卡片）
- **新增 Mock 資料**：將 React 版的 types + constants 原封搬入，供開發階段使用
- **清除 starter 模板遺留**：刪除 `AppLogo.vue`、`TemplateMenu.vue`、`useUserApi.ts` 等

## Capabilities

### New Capabilities
- `client-design-system`: Brutalist 風格設計系統（CSS tokens、card/button 樣式、dark mode、字體）
- `client-layout`: 前台 Layout（Header、Mobile Nav、Footer、User dropdown）
- `client-pages`: 前台 6 個頁面的 UI 結構與互動（Home、Create、GroupDetail、MyOrders、Profile、AdminPanel）

### Modified Capabilities

_無修改既有 capabilities_

## Impact

- **`web/apps/client/`**：整個 app 目錄結構重寫
- **`package.json`**：移除 `@nuxt/ui` 相關依賴，新增 `lucide-vue-next`、`@nuxtjs/color-mode`、`@nuxtjs/google-fonts`、`@vueuse/core`
- **`nuxt.config.ts`**：移除 `@nuxt/ui` module，新增 color-mode、google-fonts 設定
- **`app.config.ts`**：移除 Nuxt UI theme 設定
- **不影響**：後端 API、admin app、shared packages、OpenSpec specs

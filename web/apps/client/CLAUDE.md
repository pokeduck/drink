# apps/client

`@drink/client`：揪團訂飲料平台前台 Nuxt 4 app，Neo-Brutalism 設計風格。Port 8082。

## 啟動 / 指令

```bash
pnpm -C web --filter @drink/client dev       # dev server, http://localhost:8082
pnpm -C web --filter @drink/client build     # nuxt build
pnpm -C web --filter @drink/client preview   # 預覽 build 結果
pnpm -C web --filter @drink/client lint      # eslint .
pnpm -C web --filter @drink/client typecheck # nuxt typecheck (vue-tsc)
```

`ssr: false`（純 SPA）。需要呼叫後端 API → 先啟 User.API (5102) + Upload.API (5103)。

## 技術棧

- Nuxt 4 (`^4.3.1`) + Vue 3
- Tailwind CSS v4（`@tailwindcss/vite`），唯一全域樣式 `app/assets/css/main.css`
- `@nuxtjs/color-mode`：light / dark，class-based，`<html>` 上 class
- `@nuxtjs/google-fonts`：Inter（內文）+ Space Grotesk（heading）
- `lucide-vue-next`：唯一 icon 來源
- `@vueuse/core`：`onClickOutside` 等 utility
- Pinia + `@pinia/nuxt`
- `openapi-fetch`（搭配 `@app/api-types`）

## 主要檔案 / 子層

`app/` 目錄底下：

| 子層 | 內容 |
|------|------|
| `app.vue` | 只放 `useHead` / `useSeoMeta` + `<NuxtLayout>` |
| `app.config.ts` | app-level config |
| `assets/css/main.css` | 唯一全域 CSS（`@theme` tokens + base + utilities + transitions） |
| `components/` | auto-import 共用元件（`BackButton` / `SectionHeader` / `EmptyState` / `GroupCard` / `BrutalistModal` / `StoreLogo` / `FormLabel` / `StatusBadge`） |
| `composables/` | auto-import composable（`useMockData` / `useAssetHost` / `useImageUploadQueue`） |
| `layouts/default.vue` | 唯一 layout（header + main + bottom nav + footer status bar） |
| `pages/` | file-based routing |
| `middleware/` | `auth.global.ts` 等 |
| `stores/` | Pinia store |
| `utils/format.ts` | auto-import `formatPrice` / `formatTime` |

⚠️ 目前 `composable/`（空）與 `composables/` 並存，新檔一律放 `composables/`（Nuxt 4 預設）。

## 設計 / UI 規範

**所有前台 UI / 設計 token / 元件用法請依下列規則檔，本檔不重抄：**

- [`../../../.claude/rules/client-ui.md`](../../../.claude/rules/client-ui.md) — Layout、頁面結構、navigation、互動
- [`../../../.claude/rules/client-ui-design-system.md`](../../../.claude/rules/client-ui-design-system.md) — 色彩 / 字體 / shadow / motion token
- [`../../../.claude/rules/client-ui-components.md`](../../../.claude/rules/client-ui-components.md) — 共用 component API
- [`../../../.claude/rules/nuxt.md`](../../../.claude/rules/nuxt.md) — Nuxt / Nuxt UI 文件索引

## 與其他套件的關係

依賴：
- `@app/api-types`（type-only，後端 Swagger 產出）
- `@app/core`（共用 fetch / auth / error code / menu constants）
- `@app/models`（共用 enum 與型別）
- `@app/tsconfig`（tsconfig base）

被依賴：無（端點 app）。

## 不要做的事

- 不要引入 Element Plus、Nuxt UI v4、heroicons 或其他 UI 套件（與 admin 完全切開）
- 不要寫 inline SVG icon，一律 `lucide-vue-next`
- 不要使用 Tailwind 預設 `shadow-md` / `shadow-lg`，要用 `shadow-brutalist*` token
- 不要寫 `text-gray-XXX` / `border-gray-XXX`，brutalist 邊框一律 `border-black dark:border-white`
- 不要在 page `<style scoped>` 重複定義已抽到 `main.css` 的動畫（`animate-fade-in` / `animate-slide-in`）
- 不要把空狀態寫成 `<div>沒有資料</div>`，必用 `<EmptyState>`
- 不要引入 admin 的 `useFormLayout` / `AppBreadcrumb` / `AppPagination`

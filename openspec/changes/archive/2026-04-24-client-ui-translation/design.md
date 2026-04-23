## Context

`web/apps/client/` 目前是 Nuxt UI starter 模板，需要翻譯 `web/template-ui/`（React + Tailwind v4 + motion/react）的 Brutalist 風格 UI。

現有 client app 結構：
- `nuxt.config.ts`：已配置 `ssr: false`、port 8082、`@app/models` / `@app/core` alias
- `package.json`：monorepo workspace package `@drink/client`
- `app/`：starter 模板內容，全部要替換

React 模板結構：6 頁面、1 共用元件、1 layout、mock 資料、Brutalist CSS 設計系統。

## Goals / Non-Goals

**Goals:**
- UI 1:1 還原 React 模板的視覺與互動
- 全部使用 Vue/Nuxt 慣例寫法（Composition API、file-based routing、`<script setup>`）
- 純 Tailwind CSS 手刻，不依賴 UI 框架
- 動畫使用 Nuxt 原生 `pageTransition` + Vue `<Transition>`
- Mock 資料原封帶入，可直接跑起來看效果

**Non-Goals:**
- 不串接後端 API（mock 資料即可）
- 不使用 shared packages（`@app/models`、`@app/core`）
- 不做 SSR（維持 SPA 模式）
- 不做 i18n 或多語系
- 不做 responsive 以外的 mobile native 功能

## Decisions

### 1. 移除 `@nuxt/ui`，純 Tailwind CSS

**選擇**：完全移除 `@nuxt/ui`，所有 UI 手刻

**原因**：React 模板是 Brutalist 風格，與 Nuxt UI 的設計語言完全不同。套 Nuxt UI 再覆寫反而增加複雜度。

**替代方案**：用 Nuxt UI + 自訂 theme → 覆寫量太大，不如手刻乾淨

### 2. Icon 方案：`lucide-vue-next`

**選擇**：直接 import Vue component（`<Coffee />`, `<Sun />`）

**原因**：React 版用 `lucide-react` 同樣是 component import 風格，翻譯最直覺。Tree-shakable，不會打包多餘 icon。

**替代方案**：`@iconify/vue` 字串式 → 多一層抽象，且失去 TypeScript 自動完成

### 3. Dark mode：`@nuxtjs/color-mode`

**選擇**：使用 `@nuxtjs/color-mode` 模組

**原因**：React 版自己寫了 ThemeContext，但 Nuxt 生態已有成熟方案。`@nuxtjs/color-mode` 自動管 class、localStorage、prefers-color-scheme，比自己寫更可靠。

### 4. 字體載入：`@nuxtjs/google-fonts`

**選擇**：使用 `@nuxtjs/google-fonts` 模組載入 Inter + Space Grotesk

**原因**：自動 preload、font-display swap、避免 FOUT。比 CSS `@import` 效能更好。

### 5. 動畫策略：Vue `<Transition>` + CSS

**選擇**：
- 頁面轉場 → Nuxt `pageTransition`（fade）
- Modal 進出 → Vue `<Transition>`（fade + slide-up）
- 列表 stagger → CSS `animation-delay` 搭配 `v-for` index
- Hover/tap 效果 → 純 CSS `hover:` / `active:` + Tailwind

**原因**：React 版用 `motion/react` 做了大量動畫，但核心體感（fade、slide、scale）用 CSS transition 完全能達成。不需要引入額外動畫庫。

**捨棄的效果**：`whileTap` scale（改用 CSS `active:scale-95`）、staggered 入場精確時間控制（改用 CSS animation-delay 近似）

### 6. Click outside：`@vueuse/core`

**選擇**：使用 `onClickOutside` composable

**原因**：Vue 生態標準方案，比自己寫 event listener 更可靠（處理 shadow DOM、iframe 等邊界情況）。`@vueuse/core` 是 tree-shakable 的，只打包用到的函式。

### 7. 檔案結構

```
web/apps/client/app/
├── assets/css/main.css          ← Brutalist 設計系統
├── composables/
│   └── useMockData.ts           ← types + mock 資料（合併）
├── utils/
│   └── format.ts                ← formatPrice(), formatTime()
├── layouts/
│   └── default.vue              ← Header + Mobile Nav + Footer
├── components/
│   └── GroupCard.vue            ← 揪團卡片
├── pages/
│   ├── index.vue                ← Home
│   ├── create.vue               ← CreateGroup
│   ├── group/
│   │   └── [id].vue             ← GroupDetail
│   ├── my-orders.vue            ← MyOrders
│   ├── profile.vue              ← Profile
│   └── admin/
│       └── [id].vue             ← AdminPanel
├── app.vue                      ← 精簡殼
└── app.config.ts                ← 移除 Nuxt UI 設定
```

## Risks / Trade-offs

**[移除 `@nuxt/ui` 後 CSS reset 差異]** → Tailwind 自帶 preflight 已足夠，但需確認 `@nuxt/ui` 移除後沒有遺漏的 base style。若有問題在 `main.css` 補上。

**[動畫體感降級]** → CSS transition 無法完全複製 `motion/react` 的 spring physics 和 stagger 精度。可接受的取捨，核心體感（fade、slide、scale）不受影響。

**[Mock 資料硬編碼]** → 未來串 API 時需要重構 data layer。目前刻意如此，讓 UI 能獨立開發驗證。

**[`@vueuse/core` bundle size]** → Tree-shakable，只用 `onClickOutside` 不會增加太多體積。未來可能會用到更多 composable，提前裝好。

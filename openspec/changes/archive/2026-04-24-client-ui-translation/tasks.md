## 1. 環境設定與清理

- [x] 1.1 移除 `@nuxt/ui` 相關依賴，安裝新依賴（`lucide-vue-next`、`@nuxtjs/color-mode`、`@nuxtjs/google-fonts`、`@vueuse/core`）
- [x] 1.2 更新 `nuxt.config.ts`：移除 `@nuxt/ui` module，新增 color-mode、google-fonts 設定
- [x] 1.3 清除 starter 模板遺留檔案（`AppLogo.vue`、`TemplateMenu.vue`、`useUserApi.ts`、舊 `pages/index.vue`）
- [x] 1.4 精簡 `app.vue`（移除 UApp/UHeader/UFooter，改為 NuxtLayout + NuxtPage）
- [x] 1.5 清空或移除 `app.config.ts`（移除 Nuxt UI theme 設定）

## 2. 設計系統

- [x] 2.1 翻譯 `index.css` → `assets/css/main.css`（Brutalist 設計系統、@theme tokens、dark mode、scrollbar）
- [x] 2.2 新增頁面轉場 CSS（fade transition for Nuxt pageTransition）

## 3. 共用資源

- [x] 3.1 建立 `composables/useMockData.ts`（types 定義 + mock 資料，從 React 版 types.ts + constants.ts 翻譯）
- [x] 3.2 建立 `utils/format.ts`（formatPrice、formatTime 工具函式）

## 4. Layout

- [x] 4.1 建立 `layouts/default.vue`（Desktop Header + Mobile Bottom Nav + Desktop Footer Status Bar）
- [x] 4.2 實作 User Avatar Dropdown（含 onClickOutside 關閉、Transition 動畫）
- [x] 4.3 實作 Dark mode toggle 按鈕（使用 useColorMode）
- [x] 4.4 實作 Mobile Bottom Nav active 狀態樣式

## 5. 共用元件

- [x] 5.1 建立 `components/GroupCard.vue`（狀態 badge、店家資訊、成員數、CTA）

## 6. 頁面 — Home

- [x] 6.1 建立 `pages/index.vue`（Active Groups grid + History grid + 空狀態）

## 7. 頁面 — Create Group

- [x] 7.1 建立 `pages/create.vue`（3 步驟表單 + 店家選擇 Modal + 搜尋過濾）

## 8. 頁面 — Group Detail

- [x] 8.1 建立 `pages/group/[id].vue`（Header + Host Controls + Menu Grid + 點餐 Modal）

## 9. 頁面 — My Orders

- [x] 9.1 建立 `pages/my-orders.vue`（訂單列表 + 付款狀態 + 空狀態）

## 10. 頁面 — Profile

- [x] 10.1 建立 `pages/profile.vue`（Profile Header + Stats Sidebar + Account Menu + 響應式 layout）

## 11. 頁面 — Admin Panel

- [x] 11.1 建立 `pages/admin/[id].vue`（Progress Tracker + Financial Summary + Order List + Broadcast 按鈕）

## 12. 驗證

- [x] 12.1 啟動 dev server，確認所有頁面可正常瀏覽、dark mode 切換正常、mobile/desktop 響應式正確

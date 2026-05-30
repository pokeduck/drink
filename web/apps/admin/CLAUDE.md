# apps/admin

`@drink/admin`：揪團訂飲料平台後台 Nuxt 4 app，使用 Element Plus 元件庫。Port 8081。

## 啟動 / 指令

```bash
pnpm -C web --filter @drink/admin dev        # dev server, http://localhost:8081
pnpm -C web --filter @drink/admin build      # nuxt build
pnpm -C web --filter @drink/admin preview    # 預覽 build 結果
pnpm -C web --filter @drink/admin generate   # 靜態產出（非常用）
```

需要呼叫後端 API → 先啟 Admin.API (5101)；要重生型別請從根跑 `pnpm generate`。

## 技術棧

- Nuxt 4 (`^4.3.1`) + Vue 3.5
- Element Plus + `@element-plus/nuxt`
- Pinia + `@pinia/nuxt`
- `vuedraggable`（排序拖拉）
- `openapi-fetch`（搭配 `@app/api-types` 的型別）

## 主要檔案 / 子層

`app/` 目錄底下：

| 子層 | 內容 |
|------|------|
| `components/` | 共用元件（`AppBreadcrumb`、`AppPagination`、`AppTimestamp`、`FormHint` 等） |
| `composable/` | composable（`useFormLayout`、`useLoading`、`useUnsavedGuard` 等） |
| `pages/` | file-based routing，list / create / edit 三段式 |
| `layouts/` | 後台 layout（含 `el-backtop` scroll-to-top） |
| `middleware/` | 認證 / 角色中介層 |
| `stores/` | Pinia store |
| `plugins/` | 全域 plugin（如 `input-number-defaults.client.ts`） |
| `utils/` | `formatDateTime()` 等共用 utility |
| `assets/` | 全域 CSS、靜態資源 |

## UI / 表單規範

**所有後台 UI 規則（表單、表格、loading、menu、分頁、編輯頁離開保護等）一律以**
[`../../../.claude/rules/admin-ui.md`](../../../.claude/rules/admin-ui.md) **為準，不在本檔重複。**

## 與其他套件的關係

依賴：
- `@app/api-types`（type-only，後端 Swagger 產出）
- `@app/core`（共用 fetch / auth / error code / menu constants）
- `@app/models`（共用 enum 與型別）
- `@app/tsconfig`（tsconfig base）

被依賴：無（端點 app）。

## 不要做的事

- 不要引入 Tailwind / Brutalist 樣式或 `lucide-vue-next`（屬於 client 前台設計系統）
- 不要在按鈕上掛 `:loading`，改用 `v-loading` 蓋容器（見 admin-ui.md）
- 不要用 `el-page-header` 做返回，請用 card header 內的 text button + `ArrowLeft`
- 不要自寫後端 API 型別，必須走 `@app/api-types`
- 不要在 `composable/` 與 `composables/` 之間亂放（admin 目前用 `composable/`，沿用即可）
- 不要在 menu 顯示與否上重複後端的角色判斷，前端拿到什麼就渲染什麼

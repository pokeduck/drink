## Why

後端 `admin-order` capability 已上線(列表/詳情/狀態管理/取消/匯出/通知 共六個 endpoint),但前端 Admin Nuxt App 的 `/order/list` 頁面目前還是 `<el-result icon="info" title="訂單列表" sub-title="此功能尚未開發" />` 的 stub。所有後台訂單管理動作目前只能透過 Swagger 手動敲,管理員無法在實際的後台介面看到訂單、推進狀態、匯出 Excel、發通知。本 change 把這條 UI 路徑打通,讓 admin 角色拿到 menu 後可以完成所有訂單管理動作。

## What Changes

- 重寫 `web/apps/admin/app/pages/order/list.vue`:列表頁(分頁/排序/keyword/status/shop/created_from/to/deadline_from/to 篩選 + 跳轉詳情)
- 新增 `web/apps/admin/app/pages/order/list/[id].vue`:詳情頁(訂單資訊 + summary + 飲料明細扁平表 + 四個操作按鈕:改狀態 / 取消 / 匯出 / 通知)
- 共用元件:`OrderStatusTag.vue`(顏色 + 中文 label)、`OrderStatusChangeDialog.vue`(改狀態選擇器,內含白名單邏輯)
- 全部使用 `@app/api-types/admin` 已產出的型別,不重寫 fetch 與型別
- 篩選 / 操作 / 錯誤回饋一律依 `.claude/rules/admin-ui.md` 的 pattern(v-loading 蓋 card、stripe table、AppBreadcrumb、AppPagination、useApiFeedback、useLoading)

## Capabilities

### New Capabilities

- `admin-order-ui`:後台訂單管理 UI — 對應 admin-order capability 的六個 endpoint,涵蓋列表頁、詳情頁、共用元件與 UX 規格

### Modified Capabilities

(無 — admin-order 後端 capability 不變動,本 change 純前端對接)

## Impact

**新增前端檔案**

- `web/apps/admin/app/pages/order/list.vue`(重寫,目前 stub)
- `web/apps/admin/app/pages/order/list/[id].vue`(新增,詳情頁)
- `web/apps/admin/app/components/OrderStatusTag.vue`(共用狀態 tag)
- `web/apps/admin/app/components/OrderStatusChangeDialog.vue`(共用狀態變更 dialog)

**不變動**

- 後端任何檔案(admin-order capability 已完成)
- AdminMenu seed(訂單管理 menu id=7 / id=8 早已存在於 `AdminMenuSeeder`)
- `@app/api-types/admin` 型別檔(`pnpm generate` 已產生 6 個 endpoint 對應型別)

**不含**

- 通知歷史紀錄頁面(屬於另一個尚未實作的 notification capability)
- 推送通知設定 UI(同上)
- 前台 user-order UI(屬於 client app,另一個 capability)

## 1. 列表頁時間格式與寬度統一

- [x] 1.1 admin-account/list.vue：import `formatDateTime`，建立時間改用 `formatDateTime(row.created_at)`，欄位寬度改為 `width="160"`
- [x] 1.2 admin-account/role/index.vue：import `formatDateTime`，建立時間改用 `formatDateTime(row.created_at)`，欄位寬度改為 `width="160"`
- [x] 1.3 member/list.vue：import `formatDateTime`，建立時間改用 `formatDateTime(row.created_at)`，欄位寬度改為 `width="160"`
- [x] 1.4 drink-option/ice/list.vue：import `formatDateTime`，建立時間改用 `formatDateTime(row.created_at)`，欄位寬度改為 `width="160"`
- [x] 1.5 drink-option/sugar/list.vue：import `formatDateTime`，建立時間改用 `formatDateTime(row.created_at)`，欄位寬度改為 `width="160"`
- [x] 1.6 drink-option/size/list.vue：import `formatDateTime`，建立時間改用 `formatDateTime(row.created_at)`，欄位寬度改為 `width="160"`
- [x] 1.7 drink-option/topping/list.vue：import `formatDateTime`，建立時間改用 `formatDateTime(row.created_at)`，欄位寬度改為 `width="160"`
- [x] 1.8 drink-option/item/list.vue：import `formatDateTime`，建立時間改用 `formatDateTime(row.created_at)`，欄位寬度改為 `width="160"`
- [x] 1.9 member/verification/register.vue：import `formatDateTime`，發送時間與過期時間改用 `formatDateTime()`，兩欄寬度改為 `width="160"`
- [x] 1.10 member/verification/forgot-password.vue：import `formatDateTime`，發送時間與過期時間改用 `formatDateTime()`，兩欄寬度改為 `width="160"`

## 2. 列表頁表格內排序欄位修正（drink-option 5 頁）

- [x] 2.1 ice/list.vue：排序欄移除 `controls-position="right"`、移除 `size="small"`，寬度改為 `120px`，el-table-column `width="150"`
- [x] 2.2 sugar/list.vue：同上
- [x] 2.3 size/list.vue：同上
- [x] 2.4 topping/list.vue：同上
- [x] 2.5 item/list.vue：同上

## 3. 表單頁 el-input-number 修正（create + edit）

- [x] 3.1 ice/create.vue：排序欄移除 `controls-position="right"`，寬度改為 `width: 180px; max-width: 100%`
- [x] 3.2 ice/[id]/edit.vue：同上
- [x] 3.3 sugar/create.vue：預設價格 + 排序欄移除 `controls-position="right"`，寬度改為 `width: 180px; max-width: 100%`
- [x] 3.4 sugar/[id]/edit.vue：同上
- [x] 3.5 size/create.vue：排序欄移除 `controls-position="right"`，寬度改為 `width: 180px; max-width: 100%`
- [x] 3.6 size/[id]/edit.vue：同上
- [x] 3.7 topping/create.vue：預設價格 + 排序欄移除 `controls-position="right"`，寬度改為 `width: 180px; max-width: 100%`
- [x] 3.8 topping/[id]/edit.vue：同上
- [x] 3.9 item/create.vue：排序欄移除 `controls-position="right"`，寬度改為 `width: 180px; max-width: 100%`
- [x] 3.10 item/[id]/edit.vue：同上
- [x] 3.11 shop/create.vue：排序 + 加料上限移除 `controls-position="right"`，寬度改為 `width: 180px; max-width: 100%`

## 4. shop/override.vue 表格內 el-input-number 修正

- [x] 4.1 shop/override.vue：4 處覆寫欄位移除 `controls-position="right"`，寬度改為 `180px`

## 5. 編輯頁面 AppTimestamp（大單元）

- [x] 5.1 admin-account/[id]/edit.vue：el-card 加上 header slot 顯示 `<AppTimestamp>`，移除建立時間/更新時間的 disabled input 和 `toLocaleString` 格式化，改存原始日期字串
- [x] 5.2 member/[id]/edit.vue：同上，移除建立時間/更新時間的 disabled input，改用 `<AppTimestamp>`

## 6. 編輯頁面 useUnsavedGuard

- [x] 6.1 admin-account/[id]/edit.vue：加入 `useUnsavedGuard(form)` + 載入後 `takeSnapshot()` + 儲存成功後 `takeSnapshot()`
- [x] 6.2 admin-account/role/[roleId]/edit.vue：加入 `useUnsavedGuard` 保護 form 和 menuCrudList（需確認 guard 範圍）
- [x] 6.3 member/[id]/edit.vue：加入 `useUnsavedGuard(form)` + `takeSnapshot()`
- [x] 6.4 drink-option/ice/[id]/edit.vue：加入 `useUnsavedGuard(form)` + `takeSnapshot()`
- [x] 6.5 drink-option/sugar/[id]/edit.vue：同上
- [x] 6.6 drink-option/size/[id]/edit.vue：同上
- [x] 6.7 drink-option/topping/[id]/edit.vue：同上
- [x] 6.8 drink-option/item/[id]/edit.vue：同上
- [x] 6.9 shop/[id]/edit.vue：將儲存成功後的 `markSaved()` 改為 `takeSnapshot()`（符合規範，且語意更正確）

## 7. role/index.vue 排序功能

- [x] 7.1 確認後端 `/api/admin/roles` 是否支援 sort_by / sort_order 參數
- [ ] ~~7.2 若支援：加入 `@sort-change` handler、ID 與建立時間欄位加 `sortable="custom"`~~（不適用：後端不支援）
- [x] 7.3 若不支援：僅記錄為已知差異，不強行加入前端排序避免不一致

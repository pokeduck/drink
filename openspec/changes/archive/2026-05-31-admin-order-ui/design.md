## Context

後端 `admin-order` capability 提供六個 endpoint 已上線並通過手動測試,Admin Nuxt App 需對接成完整 UI。Admin app 已有一套穩定的 list/edit 頁面模式(可參考 `app/pages/shop/list.vue`、`app/pages/shop/[id].vue`),以及對應的 composable 與 utility:

- `useAdminApi()`:`openapi-fetch` client,自動注入 access token + 401 refresh
- `useApiFeedback()`:統一 ElNotification 成功、ElMessageBox 錯誤、fullscreen loading 最低 1 秒
- `useLoading()`:`v-loading` 蓋容器用的 loading state(最低 1 秒)
- `usePermission()`:依 menu role 控制按鈕顯示
- `AppPagination`、`AppBreadcrumb`、`AppTimestamp`、`FormHint` 等共用元件
- `formatDateTime()`(`utils/format.ts`)、`formatPrice` 暫無 → 直接 toFixed(0) 即可

本 change 完全跟隨既有 pattern,**不重造輪子、不引入新 composable**,亦無權限模型更動(menu id=8 `OrderList` 已存在)。

## Goals / Non-Goals

### Goals

- 列表頁完整支援:keyword、status、shop、created_from/to、deadline_from/to 篩選 + id/created_at/deadline 排序 + 分頁,行為與 ShopList 一致
- 詳情頁完整支援:訂單資訊 + summary + 飲料明細扁平表 + 四個操作按鈕(改狀態 / 取消 / 匯出 / 通知)
- 狀態流轉白名單在前端 dialog 內收斂,只列允許的目標狀態
- 操作成功 / 失敗回饋使用 `useApiFeedback`,與其他 admin 頁面一致

### Non-Goals

- 不做通知歷史頁(待 notification capability)
- 不做訂單建立 / 編輯 / 刪除按鈕(admin-order spec 明文禁止)
- 不做飲料明細的 inline edit / batch action
- 不做 UI 端的權限細分(後端 RoleMiddleware 已守住,前端只判斷 menu 拿到沒)

## Decisions

### 1. 詳情頁路由:`/order/list/[id]` 而非 `/order/[id]`

**Why:** Admin app 既有的列表-詳情 pattern 一律用 `/{module}/list/[id]`(e.g. `shop/list/[id].vue`,雖然檔案放在 `shop/[id].vue` 但 menu route 指向 `/shop/list/{id}`)。沿用這個慣例,menu 結構 / breadcrumb / 返回行為都能複用既有元件。

**Trade-off:** URL 多一層 `/list/`,語意稍冗;但與全站一致比短 URL 更重要。

### 2. 狀態 tag:獨立 `OrderStatusTag.vue` component

**Why:** Status 顏色 + 中文 label 在列表、詳情、狀態變更 dialog 內都會用到(至少三處),抽 component 避免常數散落。

**結構:**

```vue
<!-- 用法 -->
<OrderStatusTag :status="row.status" />
```

內部維護 `Record<GroupOrderStatus, { label, type }>` 對應表,直接渲染 `<el-tag :type>`。

### 3. 改狀態:獨立 `OrderStatusChangeDialog.vue`

**Why:** 狀態流轉白名單(spec admin-order 內 `Active → Closed/Delivered/Cancelled` 等)需在前端先過濾,避免讓使用者選了會被後端打回的目標。把這段邏輯抽進 dialog component 集中維護。

**互動:**

1. 詳情頁主按鈕「變更狀態」(主色) → 開 dialog
2. Dialog 內列出當前 status 的可達目標(checkbox? radio? → 採 radio)
3. 終態(`Completed` / `Cancelled`)時不應該能開 dialog → 詳情頁直接 disable 按鈕並顯示 tooltip「終態無法變更」
4. 確認 → 呼叫 `PUT /api/admin/orders/{id}/status`,成功後 `ElNotification` + 重抓詳情
5. 失敗 → `ElMessageBox.alert` 顯示後端 `message`

**Trade-off:** 改狀態用 dialog 比 dropdown / inline select 多一個點擊,但有「顯示目標清單 + 二次確認」的清晰意圖。

### 4. 取消:獨立按鈕 + `ElMessageBox.confirm`

**Why:** Cancel 跟 status change 是兩個獨立 endpoint。spec 明文 cancel 只開放 Active/Closed,且取消是不可逆動作 → 用一個獨立紅色按鈕 + 二次確認,避免和「變更狀態 → Cancelled」混淆。

**顯示條件:** 只在 `status ∈ {Active, Closed}` 時顯示;其他狀態不渲染。

### 5. 匯出 Excel:用 `fetch` blob 下載

**Why:** `openapi-fetch` 內建型別綁定不能直接拿 binary,且匯出回的是 file stream,不是 JSON。直接用 `useAdminApi` 取得 client 後,用底層 `fetch` 帶 `Authorization` header 呼叫 `/api/admin/orders/{id}/export`,把 response 轉成 blob 用 anchor download。

**Content-Disposition 解析:** 後端用 `File(bytes, mime, fileName)` 回傳,ASP.NET Core 會把 fileName 放到 `Content-Disposition: attachment; filename*=UTF-8''...`。前端用 regex 抓 `filename\*?=(?:UTF-8'')?(.+)$` 解出檔名,失敗則 fallback 用 `order_{id}_{yyyyMMdd}.xlsx`。

**Loading:** 用 `useApiFeedback().startLoading` fullscreen lock + 最低 1 秒。

### 6. 發送通知:fire-and-show

**Why:** Notify endpoint 永遠回 200(內部 try/catch per recipient),只要 order 存在就會有 stats。前端不需 confirm,點下去直接送,回來把 `total_recipients` / `email_sent` / `push_skipped` / `none_skipped` / `failed` 用 `ElNotification` 列出,讓 admin 看到實際送達情況。

**Trade-off:** 沒有 confirm 可能會誤觸。但 admin-order spec 沒規定 idempotency,且通知本身就是「再送一次」這種重複動作,UX 上不需要重二次確認。

(註:現行 admin-order spec 描述 notify 為「將通知發送給所有訂單參與者」,並未要求前端確認框。)

### 7. 飲料明細:扁平 `el-table`,不分組

**Why:** spec admin-order 明文「分組顯示由前端處理」,本期決定**不分組**,理由:

- Excel 匯出本身也是扁平表(15 欄),admin 心智模型是「看 Excel」
- 扁平表能跟 `el-table` 排序 / 多選 / 拷貝 對齊
- 後續需要分組(例如按 recipient 印出小紙條)再做也不遲

**欄位:** recipient_name / user_name / menu_item_name / size_name / sugar_name / ice_name / toppings(`t.topping_name + ' $' + t.price` 串接)/ item_price / sugar_price / topping_price / total_price / quantity / note / created_at。

### 8. 篩選列:用 `el-form` inline mode + `useFormLayout` 桌面 inline / 手機堆疊

**Why:** 既有 admin 頁面(`shop/list.vue` 等)使用 inline form 在 desktop 排成一橫排、在 mobile 堆疊。沿用同樣 layout 不引入新規。

### 9. shopId 篩選:用 `el-select` 拉店家列表

**Why:** 店家數量目前少,一次撈全部用 select 比 cascader / search 簡單。重用既有 `GET /api/admin/shops?page_size=100` endpoint。如未來店家數爆炸再改 remote search。

### 10. 詳情頁 not found:`EmptyState` + 返回按鈕

**Why:** 404 不存在的 ID 在後端會回 `ORDER_NOT_FOUND`,前端 catch 後渲染 `<el-empty>` 並提供「返回列表」按鈕。不用 `el-result` 因為 admin app 既有 not-found pattern 都用 `el-empty`。

## Risks / Trade-offs

- **狀態白名單前後端重複定義:** 前端 `OrderStatusChangeDialog` 內會硬編一份 `ValidTransitions` Map,與後端 `AdminOrderService.ValidTransitions` 是兩份。後續 spec 改動需同步兩處。
  **Mitigation:** spec admin-order 內已記載完整白名單,如改動會由 spec change 強制提醒兩邊都改。
- **匯出檔名 fallback:** 若 Content-Disposition header 被 CORS / 中介層擋掉、或解析失敗,fallback 檔名會丟掉原 title。
  **Mitigation:** fallback 名稱仍可識別(含 order id + 日期),不會 break;真出問題時加日誌 trace。
- **通知 fire-and-show 可能誤觸:** admin 可能不小心連點兩次發出兩封 email。
  **Mitigation:** 按鈕點下後立刻 disabled 至 response 回來,加上最低 1 秒 loading 避免雙擊。

## Migration Plan

- 本 change 純前端,**不需 DB migration、不需 backend 改動**
- 既有 `/order/list` 路由的 stub 直接被 `list.vue` 重寫覆蓋,使用者沒感知
- 新增 `/order/list/[id]` 路由,既無歷史也無連結指向,風險為零
- 完成後手動驗證流程:登入 admin → 點選單「訂單列表」→ 看到 seed 訂單列表 → 點進詳情 → 試四個按鈕

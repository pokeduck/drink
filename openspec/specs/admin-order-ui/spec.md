# admin-order-ui

## Purpose

後台訂單管理 UI — Admin Nuxt App 對接 `admin-order` capability 的六個 endpoint(列表 / 詳情 / 變更狀態 / 取消 / 匯出 Excel / 發送通知),涵蓋列表頁、詳情頁、共用元件(`OrderStatusTag` / `OrderStatusChangeDialog`)與 UX 規格。

## Requirements

### Requirement: 訂單列表頁

系統 SHALL 提供 `/order/list` 頁面,顯示 `GET /api/admin/orders` 結果,支援分頁、排序、keyword、status、shop、created_from/to、deadline_from/to 篩選,並依 `.claude/rules/admin-ui.md` 套用 `AppBreadcrumb`、`v-loading`、`stripe` el-table、`AppPagination`。頁面 SHALL NOT 提供「新增訂單」按鈕。

表格欄位至少包含:`id`、`title`、`shop_name`、`initiator_name`、`status`(以 `OrderStatusTag` 渲染)、`deadline`、`order_item_count`、`total_amount`、`created_at`、操作。`id`、`created_at`、`deadline` 三欄 SHALL 啟用 `sortable="custom"`,透過 `@sort-change` 觸發 server-side 排序。

#### Scenario: 預設載入
- **WHEN** 使用者進入 `/order/list`
- **THEN** 頁面以預設參數 `page=1, page_size=20` 呼叫 list endpoint,顯示結果並依 `created_at DESC, id ASC` 排序;loading 狀態以 `v-loading` 蓋整個 el-card

#### Scenario: 套用 keyword 搜尋
- **WHEN** 使用者在 keyword 輸入框輸入 `wayne` 後送出
- **THEN** 頁面以 `keyword=wayne` 重新呼叫 list,結果僅包含 title 或 initiator name 含 `wayne` 的列;清空 keyword 後恢復全列

#### Scenario: 套用狀態與店家篩選
- **WHEN** 使用者選擇 status=`Active` 且 shop=`某店家`
- **THEN** 頁面以 `status=1&shop_id={shopId}` 重新呼叫 list

#### Scenario: 排序欄位切換
- **WHEN** 使用者點擊 `deadline` 欄位 header 切換排序方向
- **THEN** 頁面以 `sort_by=deadline&sort_order=asc|desc` 重新呼叫;再次點擊取消排序時 SHALL 回到預設排序

#### Scenario: 列表項目點擊跳轉
- **WHEN** 使用者點擊任何一列的「檢視」操作或 title
- **THEN** 路由跳轉至 `/order/list/{id}` 詳情頁

### Requirement: 訂單詳情頁

系統 SHALL 提供 `/order/list/:id` 頁面,顯示 `GET /api/admin/orders/{id}` 結果,內容含訂單基本資訊區、`summary` 區、扁平 `el-table` 顯示 `order_items`,以及四個操作按鈕(變更狀態 / 取消 / 匯出 Excel / 發送通知)。頁面 header SHALL 含返回列表按鈕與 `AppTimestamp`(顯示 `created_at` / `updated_at`)。

#### Scenario: 詳情成功載入
- **WHEN** 使用者進入存在訂單的 `/order/list/{id}`
- **THEN** 頁面顯示訂單基本資訊、summary、扁平 `order_items` 表格;table 預設依 `created_at ASC` 排序

#### Scenario: 訂單不存在
- **WHEN** 使用者進入不存在 ID 的詳情頁,後端回 404 `ORDER_NOT_FOUND`
- **THEN** 頁面顯示 `el-empty` 「找不到此訂單」並提供「返回列表」按鈕,不渲染操作區

#### Scenario: 飲料明細欄位
- **WHEN** 詳情頁渲染 `order_items`
- **THEN** 表格欄位至少包含 `recipient_name`、`user_name`、`menu_item_name`、`size_name`、`sugar_name`、`ice_name`、`toppings`(串接 topping_name + 價格)、`item_price`、`sugar_price`、`topping_price`、`total_price`、`quantity`、`note`、`created_at`

### Requirement: 變更狀態互動

系統 SHALL 在詳情頁提供「變更狀態」主按鈕,點擊後開啟 `OrderStatusChangeDialog`,dialog 內依當前狀態列出 admin-order spec 定義的合法目標(白名單),使用者選擇後呼叫 `PUT /api/admin/orders/{id}/status` 並依 `useApiFeedback` 規則回饋。

#### Scenario: Active 訂單可達目標
- **WHEN** 訂單為 `Active`,使用者點開狀態變更 dialog
- **THEN** dialog 列出 `Closed`、`Delivered`、`Cancelled` 三個目標,不列 `Active` / `Completed`

#### Scenario: Delivered 訂單可達目標
- **WHEN** 訂單為 `Delivered`,使用者點開狀態變更 dialog
- **THEN** dialog 列出 `Active`、`Closed`、`Completed` 三個目標

#### Scenario: 終態訂單無法變更
- **WHEN** 訂單為 `Completed` 或 `Cancelled`
- **THEN** 詳情頁的「變更狀態」按鈕 SHALL 為 disabled,並以 tooltip 說明「終態無法變更」;dialog 不會開啟

#### Scenario: 變更成功
- **WHEN** 使用者於 dialog 選擇合法目標並按確認,後端回 200
- **THEN** dialog 關閉,`ElNotification` 顯示成功,詳情頁自動重抓更新最新狀態與 `updated_at`

#### Scenario: 變更失敗
- **WHEN** 後端回 400 `INVALID_STATUS_TRANSITION` 或其他錯誤
- **THEN** `ElMessageBox.alert` 顯示後端 `message`,dialog 維持開啟讓使用者重選或關閉

### Requirement: 取消訂單互動

系統 SHALL 在詳情頁僅當訂單 `status ∈ { Active, Closed }` 時顯示「取消訂單」按鈕(危險色),點擊後 SHALL 跳 `ElMessageBox.confirm` 二次確認;確認後呼叫 `PUT /api/admin/orders/{id}/cancel`。

#### Scenario: Active / Closed 顯示按鈕
- **WHEN** 訂單為 `Active` 或 `Closed`
- **THEN** 詳情頁渲染「取消訂單」按鈕

#### Scenario: 其他狀態不顯示按鈕
- **WHEN** 訂單為 `Delivered` / `Completed` / `Cancelled`
- **THEN** 詳情頁不渲染「取消訂單」按鈕

#### Scenario: 確認流程
- **WHEN** 使用者按下「取消訂單」並於 ElMessageBox 按「確定」
- **THEN** 呼叫 cancel endpoint,成功 → `ElNotification` 成功 + 詳情自動重抓;失敗 → `ElMessageBox.alert` 顯示 `message`

### Requirement: 匯出 Excel 互動

系統 SHALL 在詳情頁提供「匯出 Excel」按鈕,點擊後以帶 `Authorization` header 的 `fetch` 呼叫 `GET /api/admin/orders/{id}/export`,將 response 轉為 Blob 並用隱藏 anchor 觸發瀏覽器下載。檔名 SHALL 從 `Content-Disposition` header 解析,解析失敗時 fallback 為 `order_{id}_{yyyyMMdd}.xlsx`。下載期間 SHALL 套用最低 1 秒的 fullscreen loading。

#### Scenario: 匯出成功
- **WHEN** 使用者按下匯出,後端回 200 含 xlsx 檔
- **THEN** 瀏覽器觸發下載,檔名取自 `Content-Disposition`;loading 結束;`ElNotification` 顯示「匯出成功」

#### Scenario: 匯出失敗
- **WHEN** 後端回非 200(例如 404)
- **THEN** loading 結束;`ElMessageBox.alert` 顯示錯誤訊息;不下載任何檔案

### Requirement: 發送通知互動

系統 SHALL 在詳情頁提供「發送通知」按鈕,點擊後直接呼叫 `POST /api/admin/orders/{id}/notify`(不需二次確認),按鈕 SHALL 在 request 進行中 disabled 以避免雙擊;成功後 SHALL 用 `ElNotification` 顯示後端回的 `total_recipients`、`email_sent`、`push_skipped`、`none_skipped`、`failed` 五個計數。

#### Scenario: 通知成功
- **WHEN** 使用者按下發送通知,後端回 200 `{ total_recipients: 3, email_sent: 2, push_skipped: 1, none_skipped: 0, failed: 0 }`
- **THEN** `ElNotification` 顯示「已通知 3 人 (Email: 2, Push 略過: 1, 未訂閱: 0, 失敗: 0)」

#### Scenario: 通知失敗(404)
- **WHEN** 後端回 404
- **THEN** `ElMessageBox.alert` 顯示後端 `message`;按鈕恢復可點擊

### Requirement: 共用元件

系統 SHALL 提供共用元件 `OrderStatusTag` 與 `OrderStatusChangeDialog`,集中維護狀態的中文 label、顏色、與白名單轉換規則,避免常數散落在多個頁面。

#### Scenario: OrderStatusTag 渲染
- **WHEN** 任何頁面使用 `<OrderStatusTag :status="1" />`
- **THEN** 渲染對應的 `el-tag`,顏色與文字符合 `GroupOrderStatus` 對應表(Active 綠 / Closed 黃 / Delivered 藍 / Completed 灰 / Cancelled 紅,文字一律繁體中文)

#### Scenario: OrderStatusChangeDialog 白名單
- **WHEN** 詳情頁傳入 `currentStatus` 給 `OrderStatusChangeDialog`
- **THEN** Dialog 僅渲染白名單內合法的目標 status 作為 radio 選項;選項數量為 0 時 dialog 不渲染選項而直接顯示「無可變更狀態」(理論上不會被觸發,因為終態按鈕已 disabled)

### Requirement: 與後端對接約束

實作 SHALL 使用 `@app/api-types/admin` 已生成的型別取得 `paths`、`components['schemas']`,以及 `useAdminApi()` 已建立的 client 呼叫六個 endpoint,不重寫 fetch wrapper。錯誤回饋一律走 `useApiFeedback`,loading 一律走 `useLoading` 或 `useApiFeedback.startLoading`。

#### Scenario: 型別來源
- **WHEN** 任何頁面引用訂單列表或詳情 response 型別
- **THEN** import 自 `@app/api-types/admin` 而非手寫

#### Scenario: 401 處理
- **WHEN** 任何 endpoint 回 401
- **THEN** `useAdminApi` 既有的 refresh middleware 處理 token 更新,UI 不需額外攔截

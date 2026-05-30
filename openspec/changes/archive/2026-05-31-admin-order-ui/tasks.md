## 1. 共用元件

- [x] 1.1 新增 `web/apps/admin/app/components/OrderStatusTag.vue`:接收 `status: number` prop,內部以 Map 對應 `el-tag` type + 中文 label(Active 綠 / Closed 黃 / Delivered 藍 / Completed 灰 / Cancelled 紅)
- [x] 1.2 新增 `web/apps/admin/app/components/OrderStatusChangeDialog.vue`:接收 `v-model:visible`、`orderId: number`、`currentStatus: number`;內部硬編 `ValidTransitions` Map(對照 `openspec/specs/admin-order/spec.md` 內定義);使用 `el-radio-group` 列出合法目標;按確認呼叫 `PUT /api/admin/orders/{id}/status` 並 emit `success`

## 2. 列表頁

- [x] 2.1 重寫 `web/apps/admin/app/pages/order/list.vue`:刪除目前 stub,參照 `web/apps/admin/app/pages/shop/list.vue` 結構新建
- [x] 2.2 加入 `<AppBreadcrumb />` 與 `<el-card>` + `<template #header>訂單列表</template>`
- [x] 2.3 篩選列:`el-form` inline / `useFormLayout`,欄位 keyword(`el-input`)、status(`el-select`,5 個選項 + 全部)、shop(`el-select`,呼叫 `GET /api/admin/shops?page_size=100`)、created_from/to(`el-date-picker`)、deadline_from/to(`el-date-picker`)、查詢 + 重設按鈕
- [x] 2.4 表格 `el-table` `stripe`,欄位 `id`(sortable)/title/shop_name/initiator_name/`<OrderStatusTag>`/deadline(sortable, formatDateTime)/order_item_count/total_amount(toFixed(0))/created_at(sortable, formatDateTime)/操作(檢視按鈕跳 `/order/list/{id}`)
- [x] 2.5 `@sort-change` 更新 `sortBy` / `sortOrder` reactive 並重抓
- [x] 2.6 `v-loading` 蓋 `el-card`;`useLoading` 控制
- [x] 2.7 分頁 `<AppPagination>` 綁到 `page` / `pageSize` / `total`,預設 20 筆;支援 10/20/50/100
- [x] 2.8 把 `total_amount` 顯示為 `$ {value}`(整數,參考 client utils 的 formatPrice 但 admin 沒這個 util,直接 `.toFixed(0)` + prefix)

## 3. 詳情頁

- [x] 3.1 新增 `web/apps/admin/app/pages/order/list/[id].vue`
- [x] 3.2 路由 param 取 `id`;`onMounted` 呼叫 `GET /api/admin/orders/{id}`,失敗 404 → 顯示 `<el-empty description="找不到此訂單">` + 「返回列表」按鈕(`router.push('/order/list')`),不渲染下面所有區塊
- [x] 3.3 Card header 左側:返回按鈕(`<el-button text>`+`<ArrowLeft>`)+「訂單詳情 #{id}」;右側 `<AppTimestamp :created-at :updated-at>`
- [x] 3.4 訂單基本資訊區:title / shop_name / initiator_name / `<OrderStatusTag>` / deadline(formatDateTime)/ note;用 `el-descriptions` 或 grid 排版
- [x] 3.5 Summary 區:total_items / total_amount(整數)/ recipient_count,用三個 `el-statistic` 或 grid card
- [x] 3.6 操作區(四個按鈕):
  - 「變更狀態」(primary,主按鈕):終態時 `disabled` + tooltip;點擊開 `<OrderStatusChangeDialog>`;成功後重抓詳情
  - 「取消訂單」(danger):僅在 status ∈ {Active, Closed} 顯示;點擊 `ElMessageBox.confirm`;確認後 `PUT /cancel`,成功重抓
  - 「匯出 Excel」:用底層 `fetch` 帶 `Authorization` 呼叫 export endpoint,response 轉 Blob,解析 `Content-Disposition` 取檔名(regex `filename\*?=(?:UTF-8'')?(.+)$`,fallback `order_{id}_{yyyyMMdd}.xlsx`),用隱藏 anchor 觸發下載;`useApiFeedback.startLoading` 蓋 fullscreen
  - 「發送通知」:`POST /notify`;成功後 `ElNotification` 顯示 stats 五個欄位;進行中 disable 按鈕
- [x] 3.7 飲料明細 `el-table` `stripe`:扁平展開 `order_items`,欄位 recipient_name / user_name / menu_item_name / size_name / sugar_name / ice_name / toppings(自訂 slot 串接 `topping_name + ' +$' + price`)/ item_price / sugar_price / topping_price / total_price / quantity / note / created_at(formatDateTime)
- [x] 3.8 整頁 `v-loading` 控制(載入詳情、cancel、change status 操作)

## 4. 對接與型別

- [x] 4.1 確認 `@app/api-types/admin` 已有 6 個 endpoint 型別(`pnpm generate` 已執行,sanity check)
- [x] 4.2 列表 / 詳情 / 各操作一律用 `useAdminApi()` 取得 client,不重寫 fetch wrapper(匯出例外:用底層 fetch 處理 binary)
- [x] 4.3 錯誤回饋一律走 `useApiFeedback`(`handleError(error)` 或 `ElMessageBox.alert`),成功用 `showSuccess` 或 `ElNotification`
- [x] 4.4 Loading 一律走 `useLoading` / `useApiFeedback.startLoading`,保底最低 1 秒

## 5. 驗證

- [x] 5.1 `pnpm -C web --filter @drink/admin dev` 啟動 8081,瀏覽器登入 admin 後點選單「訂單列表」
- [x] 5.2 看到至少一筆 seed 訂單(必要時 SQL 補資料);切排序、套篩選、改 page size、跳頁
- [x] 5.3 點進詳情;確認資訊區 / summary / 飲料明細表都正確
- [x] 5.4 試「變更狀態」:確認 dialog 內只有合法目標;確認後狀態更新且 AppTimestamp 重整
- [x] 5.5 試「取消訂單」:確認只有 Active/Closed 顯示按鈕;按取消 → confirm → 成功
- [x] 5.6 試「匯出 Excel」:檔案下載成功,打開檢查 15 欄表頭與資料正確
- [x] 5.7 試「發送通知」:看到 ElNotification 顯示 5 個 stats 數字

## 6. OpenSpec 驗證

- [x] 6.1 `openspec validate admin-order-ui --strict` 通過
- [x] 6.2 對照 spec 內 8 個 Requirement、所有 Scenario 逐項勾選實作覆蓋

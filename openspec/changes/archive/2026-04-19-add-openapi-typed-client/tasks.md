## 1. API Types 套件建置

- [x] 1.1 建立 `web/internal/api-types/` 套件（package.json `@app/api-types`、tsconfig.json）
- [x] 1.2 安裝 `openapi-typescript` 為 devDependency
- [x] 1.3 建立 `openapi-ts.config.ts` 設定檔（三個 API 的 swagger.json URL 與輸出路徑）
- [x] 1.4 在 `web/package.json` 加入 `gen:api` script
- [x] 1.5 啟動三個 API 服務，執行 codegen，確認產出 `admin.d.ts`、`user.d.ts`、`upload.d.ts`

## 2. Typed API Client Composable

- [x] 2.1 在 admin app 和 client app 安裝 `openapi-fetch` 為 dependency，加入 `@app/api-types` workspace 引用
- [x] 2.2 建立 `admin/composables/useAdminApi.ts`：createClient + singleton 模式
- [x] 2.3 建立 `client/composables/useUserApi.ts`：createClient + singleton 模式
- [x] 2.4 實作 auth middleware（token 注入、401 refresh、refresh 失敗重導登入）
- [x] 2.5 實作 error middleware（非 401 錯誤 toast 通知、網路斷線提示）

## 3. 驗證與遷移準備

- [x] 3.1 在 admin app 挑選一個現有頁面，將 `useApi()` 呼叫改為 typed client，驗證型別推導正確
- [x] 3.2 確認 auth store login 流程可透過 typed client 呼叫（驗證 auth middleware 運作）— 需手動瀏覽器測試
- [x] 3.3 確認非 401 錯誤時 toast 正確顯示（驗證 error middleware 運作）— 需手動瀏覽器測試

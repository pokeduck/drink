## MODIFIED Requirements

### Requirement: 路由級權限控制
前端 MUST 透過 Nuxt route middleware 阻止使用者直接輸入 URL 進入無權限的頁面。Menu 權限資料 MUST 在 route middleware 執行權限判定前已載入完成，避免重新整理頁面時因權限未載入而誤判為無權限。

由於 menu store 僅能在 client 端載入（後端為 self-signed HTTPS、SSR 端 Pinia store 為 per-request 空狀態），permission middleware MUST 在 SSR 端跳過權限判定（直接 `return`），由 client 端完成所有權限檢查。

#### Scenario: 直接輸入無權限頁面 URL
- **WHEN** 使用者直接輸入 `/admin-account/create` 但無 AdminAccountList 的 Create 權限
- **THEN** SHALL 重導至首頁並顯示無權限提示

#### Scenario: 有權限的頁面正常進入
- **WHEN** 使用者直接輸入 `/drink-option/item` 且有 DrinkItem 的 Read 權限
- **THEN** SHALL 正常載入頁面

#### Scenario: 重新整理有權限的頁面
- **WHEN** 使用者已登入且在 `/shop/list` 頁面按下重新整理（Cmd+R 或 F5），其角色具有 ShopList 的 Read 權限
- **THEN** SHALL 在 permission middleware 執行前完成 menu 權限載入，使用者 SHALL 留在 `/shop/list` 而非被誤判為無權限重導至首頁

#### Scenario: 登入後首次導航
- **WHEN** 使用者剛登入完成、即將首次進入需要權限的頁面
- **THEN** permission middleware SHALL 在判定權限前確保 menu 資料已載入（若尚未載入則 await 載入完成），確保有權限的頁面能正常進入

#### Scenario: SSR 端執行 middleware
- **WHEN** Nuxt 在 server 端為已登入使用者重整需要權限的頁面執行 permission middleware
- **THEN** middleware SHALL 在 SSR 環境（`import.meta.server === true`）下直接 return 放行，不進行 redirect，避免使用 server 端尚未載入的 menu 權限做出錯誤判定

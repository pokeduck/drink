## ADDED Requirements

### Requirement: 啟用過濾資料模型
系統 SHALL 為每間店家維護四張獨立的啟用清單表 `ShopEnabledSugar` / `ShopEnabledIce` / `ShopEnabledTopping` / `ShopEnabledSize`。每筆 row 表示「該店家啟用對應的全域選項」；無 row 視為停用。每張表 SHALL 沿用 `BaseDataEntity`（Id 為 PK）並具有 `Unique(ShopId, OptionId)` 索引、`Sort`（int）欄位作為店家內排序唯一來源，以及完整 audit 欄位（CreatedAt / Creator / UpdatedAt / Updater）。

#### Scenario: 啟用 row 表示啟用
- **WHEN** `ShopEnabledSugar` 中有 `(ShopId=1, SugarId=5)` row
- **THEN** 店家 1 視為啟用 SugarId=5

#### Scenario: 無 row 視為停用
- **WHEN** `ShopEnabledIce` 中不存在 `(ShopId=1, IceId=9)` row
- **THEN** 店家 1 視為停用 IceId=9

#### Scenario: 唯一鍵衝突
- **WHEN** 系統嘗試插入第二筆 `(ShopId=1, ToppingId=3)` row
- **THEN** 系統 SHALL 回傳資料庫唯一鍵衝突錯誤

### Requirement: 取得店家啟用設定
系統 SHALL 提供 `GET /api/admin/shops/{shopId}/options` 端點，回傳該店家四種選項（sugars / ices / toppings / sizes）的全域 pool 清單，每筆包含全域基本欄位（id、name、預設價格 / 預設排序若有）、`is_enabled`（該店家是否啟用）與 `sort`（店家內排序，未啟用時為 null 或 0）。

#### Scenario: 成功取得設定
- **WHEN** 管理員請求 `GET /api/admin/shops/1/options`
- **THEN** 系統 SHALL 回傳 `sugars` / `ices` / `toppings` / `sizes` 四個陣列，並為每筆全域選項標記 `is_enabled` 與 `sort`

#### Scenario: 全域 pool 與店家無啟用
- **WHEN** 店家 1 從未設定啟用清單，呼叫 `GET /api/admin/shops/1/options`
- **THEN** 系統 SHALL 回傳四個陣列，所有元素 `is_enabled=false`、`sort` 為 0 或 null

#### Scenario: 店家不存在
- **WHEN** 管理員請求一個不存在的 shopId
- **THEN** 系統 SHALL 回傳 404

### Requirement: 預覽啟用設定變更
系統 SHALL 提供 `POST /api/admin/shops/{shopId}/options/preview` 端點，body 與 PUT 一致，但僅模擬不寫入。回應 SHALL 包含 `newly_disabled`（本次新停用的 sugar_ids / ice_ids / topping_ids / size_ids）、`affected_menu_items`（每個會被連帶移除選項的品項 id、name 與被移除的選項 ids），以及 `affected_menu_items_count`（受影響品項總數）。

#### Scenario: 模擬停用某糖度
- **WHEN** 管理員 POST preview 並未在請求中包含 SugarId=5（原本啟用）
- **THEN** 回應 `newly_disabled.sugar_ids` SHALL 包含 5；`affected_menu_items` SHALL 列出所有勾選 SugarId=5 的品項

#### Scenario: 沒有新停用
- **WHEN** 管理員 POST preview，請求內容與目前啟用清單完全相同
- **THEN** 回應 `newly_disabled` 四個陣列 SHALL 皆為空，`affected_menu_items_count` SHALL 為 0

#### Scenario: dry-run 不寫入資料
- **WHEN** 管理員 POST preview 後再次 GET 啟用設定
- **THEN** 啟用清單 SHALL 與 preview 前完全相同

### Requirement: 整批更新店家啟用設定
系統 SHALL 提供 `PUT /api/admin/shops/{shopId}/options` 端點，整批 delete-then-insert 啟用設定，body 為四個陣列 `{ sugars, ices, toppings, sizes }`，每筆含 `option_id` 與 `sort`。請求中未列出的 option_id 視為停用。同 transaction 內，系統 SHALL 找出本次新停用的 option ids，並 cascade 刪除該店家底下對應的 `ShopMenuItemSugar` / `ShopMenuItemIce` / `ShopMenuItemTopping` / `ShopMenuItemSize` row。回應 SHALL 包含 `affected_menu_items_count`（實際被連帶移除選項的品項數）。

#### Scenario: 成功更新
- **WHEN** 管理員 PUT 含合法 ids 與 sort 的啟用清單
- **THEN** 系統 SHALL 寫入啟用清單並回傳 200 與 `affected_menu_items_count`

#### Scenario: 停用某糖度連帶清空品項
- **WHEN** 管理員 PUT 移除原本啟用的 SugarId=5，且有 3 個品項勾選了 SugarId=5
- **THEN** 系統 SHALL 在同 transaction 內刪除這 3 個品項中對應的 `ShopMenuItemSugar` row，並回 `affected_menu_items_count=3`

#### Scenario: option_id 不存在於全域 pool
- **WHEN** 管理員 PUT 內 sugar_id 對應的 Sugar 不存在
- **THEN** 系統 SHALL 回傳驗證錯誤，errors 包含 `{ "sugars": [...] }`，且不寫入任何 row

#### Scenario: option_id 重複
- **WHEN** 管理員 PUT 同一陣列內出現重複的 option_id
- **THEN** 系統 SHALL 回傳驗證錯誤

#### Scenario: 空陣列等於全停用
- **WHEN** 管理員 PUT 四個陣列皆為空
- **THEN** 系統 SHALL 刪除該店家所有啟用 row，並 cascade 清空所有 ShopMenuItemXxx 中對應 ids

#### Scenario: 並發更新影響數重新計算
- **WHEN** preview 與 PUT 之間有他人變更菜單，導致 PUT 時實際差集與 preview 不同
- **THEN** PUT 仍 SHALL 以 transaction 內重新計算的實際差集為準執行 cascade，並回真實 `affected_menu_items_count`

### Requirement: 啟用設定權限控制
所有啟用設定 API SHALL 透過 `[RequireRole(MenuConstants.ShopOptions, CrudAction.X)]` 控制存取，獨立於 `ShopList` 與 `ShopOverride`。

#### Scenario: 無 Read 權限存取 GET
- **WHEN** 管理員角色無 ShopOptions Read 權限呼叫 `GET /api/admin/shops/1/options`
- **THEN** 系統 SHALL 回傳 403

#### Scenario: 無 Update 權限存取 PUT
- **WHEN** 管理員角色無 ShopOptions Update 權限呼叫 `PUT /api/admin/shops/1/options`
- **THEN** 系統 SHALL 回傳 403

#### Scenario: 系統角色完整權限
- **WHEN** 系統角色（IsSystem=true）呼叫任一啟用設定 API
- **THEN** 系統 SHALL 正常處理請求

### Requirement: 店家內排序由啟用表負責
四種選項（Sugar / Ice / Topping / Size）的店家內排序 SHALL 統一由啟用表的 `Sort` 欄位決定。系統列出店家可用選項時 SHALL 以 `Sort ASC, Id ASC` 排序。

#### Scenario: 排序輸出
- **WHEN** 系統回傳店家啟用的糖度清單
- **THEN** 結果 SHALL 依 `Sort ASC, Id ASC` 排序

#### Scenario: 不同選項相同 sort 值
- **WHEN** 兩個啟用 row 的 Sort 相同
- **THEN** 系統 SHALL 以 Id ASC 作為次要排序維持穩定性

# admin-shop-menu

## Purpose

定義 Admin 後台店家菜單（分類、品項、尺寸價格）的管理契約：取得店家完整菜單、分類 CRUD（同店家分類名稱唯一）、品項 CRUD、品項所支援的甜度 / 冰塊 / 加料設定，以及與 `admin-shop-override` 的甜度 / 加料覆寫整合。

## Requirements

### Requirement: 取得店家完整菜單
系統 SHALL 提供 `GET /api/admin/shops/{shopId}/menu` 端點，回傳該店家所有分類、品項、品項的尺寸價格、支援的甜度/冰塊/加料 ID，以及店家的甜度/加料覆寫。

#### Scenario: 成功取得菜單
- **WHEN** 管理員請求 `GET /api/admin/shops/1/menu`
- **THEN** 系統回傳 categories 陣列（含 items）及 sugar_overrides / topping_overrides

#### Scenario: 店家不存在
- **WHEN** 管理員請求一個不存在的 shopId
- **THEN** 系統回傳 404

### Requirement: 新增分類
系統 SHALL 提供 `POST /api/admin/shops/{shopId}/categories` 端點，建立菜單分類。同店家內分類名稱 SHALL 唯一（不分大小寫）。

#### Scenario: 成功新增分類
- **WHEN** 管理員送出有效的分類名稱與排序
- **THEN** 系統建立分類並回傳 201

#### Scenario: 分類名稱重複
- **WHEN** 管理員送出該店家已存在的分類名稱
- **THEN** 系統回傳 409 CATEGORY_ALREADY_EXISTS，errors 包含 `{ "name": ["分類名稱已存在"] }`

### Requirement: 更新分類
系統 SHALL 提供 `PUT /api/admin/shops/{shopId}/categories/{categoryId}` 端點，更新分類名稱與排序。名稱唯一性檢查排除自身。

#### Scenario: 成功更新
- **WHEN** 管理員送出有效的更新資料
- **THEN** 系統更新分類並回傳 200

#### Scenario: categoryId 不屬於該 shopId
- **WHEN** 管理員更新的 categoryId 不屬於該 shopId
- **THEN** 系統回傳 404

### Requirement: 刪除分類
系統 SHALL 提供 `DELETE /api/admin/shops/{shopId}/categories/{categoryId}` 端點。刪除分類時底下所有品項 SHALL 一併 soft delete。每個受影響品項 (ShopId, DrinkItemId) 的 ShopImage SHALL 隨之孤兒化（行為與「刪除品項」一致）。

#### Scenario: 成功刪除分類
- **WHEN** 管理員刪除一個存在的分類
- **THEN** 系統刪除該分類，底下品項標記為 soft deleted

#### Scenario: 刪除分類時連帶圖片孤兒化
- **WHEN** 管理員刪除一個含多個品項的分類，部分品項有圖
- **THEN** 系統在同一 transaction 將所有受影響 (ShopId, DrinkItemId) 的 ShopImage 孤兒化

### Requirement: 分類批次排序
系統 SHALL 提供 `PUT /api/admin/shops/{shopId}/categories/sort` 端點，批次更新分類排序。

#### Scenario: 成功批次排序
- **WHEN** 管理員送出分類排序陣列
- **THEN** 系統更新對應分類的 Sort 值

### Requirement: 新增品項
系統 SHALL 提供 `POST /api/admin/shops/{shopId}/categories/{categoryId}/items` 端點。支援 `drink_item_id`（引用既有 DrinkItem）或 `drink_item_name`（自動建立新 DrinkItem）二擇一，同時提供以 drink_item_id 優先。sizes SHALL 至少一個且 Price > 0。sugar_ids、ice_ids、topping_ids 預設全勾。

#### Scenario: 使用 drink_item_id 新增
- **WHEN** 管理員送出含 drink_item_id 的品項資料
- **THEN** 系統建立品項並關聯到既有 DrinkItem

#### Scenario: 使用 drink_item_name 自動建立 DrinkItem
- **WHEN** 管理員送出含 drink_item_name 且該名稱不存在於 DrinkItem
- **THEN** 系統自動新增該名稱到全域 DrinkItem（Sort 設為最大值 + 1），並建立品項

#### Scenario: drink_item_name 已存在
- **WHEN** 管理員送出的 drink_item_name 已存在於 DrinkItem（不分大小寫）
- **THEN** 系統使用既有的 DrinkItem 建立品項

#### Scenario: drink_item_id 不存在
- **WHEN** 管理員送出的 drink_item_id 不存在
- **THEN** 系統回傳 400 DRINK_ITEM_NOT_FOUND

#### Scenario: sizes 為空
- **WHEN** 管理員未提供任何尺寸
- **THEN** 系統回傳驗證錯誤

#### Scenario: size price <= 0
- **WHEN** 管理員提供的尺寸價格 <= 0
- **THEN** 系統回傳驗證錯誤

### Requirement: 更新品項
系統 SHALL 提供 `PUT /api/admin/shops/{shopId}/categories/{categoryId}/items/{itemId}` 端點。sizes、sugar_ids、ice_ids、topping_ids SHALL 整批覆蓋（delete-then-insert）。

#### Scenario: 成功更新品項
- **WHEN** 管理員送出有效的更新資料
- **THEN** 系統更新品項及其所有子關聯

#### Scenario: itemId 不屬於該 categoryId
- **WHEN** 管理員更新的 itemId 不屬於該 categoryId
- **THEN** 系統回傳 404

### Requirement: 刪除品項
系統 SHALL 提供 `DELETE /api/admin/shops/{shopId}/categories/{categoryId}/items/{itemId}` 端點，執行 soft delete。同一 transaction 內，該 (ShopId, DrinkItemId) 的所有 ShopImage SHALL 孤兒化（DrinkItemId=null、Sort=0、IsCover=false）。

#### Scenario: 成功刪除品項
- **WHEN** 管理員刪除一個存在的品項
- **THEN** 系統將品項標記為 IsDeleted=true, DeletedAt=now

#### Scenario: 刪除品項時圖片孤兒化
- **WHEN** 管理員刪除一個存在的品項，且該 (ShopId, DrinkItemId) 已綁有 ShopImage
- **THEN** 系統在同一 transaction 將所有受影響 ShopImage DrinkItemId 設為 null、Sort 設為 0、IsCover 設為 false

### Requirement: 品項批次排序
系統 SHALL 提供 `PUT /api/admin/shops/{shopId}/categories/{categoryId}/items/sort` 端點，批次更新品項排序。

#### Scenario: 成功批次排序
- **WHEN** 管理員送出品項排序陣列
- **THEN** 系統更新對應品項的 Sort 值

### Requirement: 菜單管理權限控制
所有菜單管理 API SHALL 透過 `[RequireRole(MenuConstants.ShopList, CrudAction.X)]` 控制存取（與店家共用權限）。

#### Scenario: 無權限存取
- **WHEN** 管理員角色無 ShopList 權限
- **THEN** 系統回傳 403

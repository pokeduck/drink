## ADDED Requirements

### Requirement: 店家列表查詢
系統 SHALL 提供 `GET /api/admin/shops` 端點，支援分頁、排序、keyword 搜尋（name）、status 篩選。預設排序為 Sort ASC, Id ASC, CreatedAt DESC。回傳欄位包含 id、name、phone、address、status、sort、category_count、menu_item_count、created_at。

#### Scenario: 無篩選條件的列表查詢
- **WHEN** 管理員請求 `GET /api/admin/shops?page=1&page_size=20`
- **THEN** 系統回傳所有未刪除店家（IsDeleted=false），依 Sort ASC 排序，含分頁資訊

#### Scenario: keyword 搜尋
- **WHEN** 管理員請求 `GET /api/admin/shops?keyword=50嵐`
- **THEN** 系統回傳 name 包含「50嵐」的店家

#### Scenario: status 篩選
- **WHEN** 管理員請求 `GET /api/admin/shops?status=1`
- **THEN** 系統僅回傳 Status=Active 的店家

### Requirement: 取得單一店家
系統 SHALL 提供 `GET /api/admin/shops/{shopId}` 端點，回傳店家詳細資訊，包含 id、name、phone、address、note、status、sort、max_topping_per_item、max_topping_count、created_at、updated_at。

#### Scenario: 店家存在
- **WHEN** 管理員請求 `GET /api/admin/shops/1`
- **THEN** 系統回傳該店家完整資訊

#### Scenario: 店家不存在
- **WHEN** 管理員請求一個不存在的 shopId
- **THEN** 系統回傳 404

### Requirement: 新增店家
系統 SHALL 提供 `POST /api/admin/shops` 端點，建立新店家。name 為必填且唯一（不分大小寫，僅在未刪除資料中檢查）。MaxToppingCount 預設值 SHALL 從系統設定（AdminSystemSetting）帶入。

#### Scenario: 成功新增
- **WHEN** 管理員送出有效的店家資料
- **THEN** 系統建立店家並回傳 201

#### Scenario: 名稱重複
- **WHEN** 管理員送出已存在的店家名稱
- **THEN** 系統回傳 409 SHOP_ALREADY_EXISTS，errors 包含 `{ "name": ["店家名稱已存在"] }`

### Requirement: 更新店家
系統 SHALL 提供 `PUT /api/admin/shops/{shopId}` 端點，更新店家資訊。name 唯一性檢查排除自身。

#### Scenario: 成功更新
- **WHEN** 管理員送出有效的更新資料
- **THEN** 系統更新店家資訊並回傳 200

#### Scenario: 名稱重複（排除自身）
- **WHEN** 管理員將名稱改為其他店家已使用的名稱
- **THEN** 系統回傳 409 SHOP_ALREADY_EXISTS

### Requirement: 刪除店家
系統 SHALL 提供 `DELETE /api/admin/shops/{shopId}` 端點，執行 soft delete（IsDeleted=true, DeletedAt=now）。底下所有 ShopMenuItem SHALL 一併 soft delete。

#### Scenario: 成功刪除
- **WHEN** 管理員刪除一個存在的店家
- **THEN** 系統將該店家及其底下品項標記為已刪除

#### Scenario: 店家不存在
- **WHEN** 管理員刪除一個不存在的 shopId
- **THEN** 系統回傳 404

### Requirement: 批次排序
系統 SHALL 提供 `PUT /api/admin/shops/sort` 端點，接受 `items: [{ id, sort }]` 陣列，批次更新 Sort 欄位。

#### Scenario: 成功批次排序
- **WHEN** 管理員送出排序陣列
- **THEN** 系統更新對應店家的 Sort 值

### Requirement: 批次刪除
系統 SHALL 提供 `DELETE /api/admin/shops/batch` 端點，接受 `ids: [1, 2, 3]`，批次 soft delete 店家及其底下品項。

#### Scenario: 成功批次刪除
- **WHEN** 管理員送出有效的 ID 陣列
- **THEN** 系統將所有指定店家及底下品項標記為已刪除

### Requirement: 店家權限控制
所有店家管理 API SHALL 透過 `[RequireRole(MenuConstants.ShopList, CrudAction.X)]` 控制存取，無權限回傳 403。

#### Scenario: 無權限存取
- **WHEN** 管理員角色無 ShopList 權限
- **THEN** 系統回傳 403

### Requirement: 加料限制欄位
Shop Entity SHALL 包含 `MaxToppingPerItem`（int, 預設 1）和 `MaxToppingCount`（int, 預設從系統設定帶入）欄位，控制該店家的加料規則。

#### Scenario: MaxToppingPerItem = 1
- **WHEN** 店家設定 MaxToppingPerItem = 1
- **THEN** 前台點餐時每種加料最多選 1 份

#### Scenario: MaxToppingPerItem = 2
- **WHEN** 店家設定 MaxToppingPerItem = 2
- **THEN** 前台點餐時每種加料最多選 2 份

#### Scenario: MaxToppingCount 限制總份數
- **WHEN** 店家設定 MaxToppingCount = 5
- **THEN** 前台點餐時單杯飲料加料總份數不得超過 5

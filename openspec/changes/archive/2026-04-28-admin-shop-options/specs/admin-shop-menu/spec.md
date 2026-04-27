## MODIFIED Requirements

### Requirement: 取得店家完整菜單
系統 SHALL 提供 `GET /api/admin/shops/{shopId}/menu` 端點，回傳該店家所有分類、品項、品項的尺寸價格、支援的甜度/冰塊/加料 ID，以及店家的甜度/加料覆寫。

回應中每個品項可選的「sugar / ice / topping / size 池」SHALL 由該店家的啟用清單（`ShopEnabledSugar` / `ShopEnabledIce` / `ShopEnabledTopping` / `ShopEnabledSize`）決定，**而非全域 pool**。未啟用的選項 SHALL 不出現在可選池中。

#### Scenario: 成功取得菜單
- **WHEN** 管理員請求 `GET /api/admin/shops/1/menu`
- **THEN** 系統回傳 categories 陣列（含 items）及 sugar_overrides / topping_overrides；可選的 sugar / ice / topping / size 池僅包含店家 1 啟用的選項

#### Scenario: 店家不存在
- **WHEN** 管理員請求一個不存在的 shopId
- **THEN** 系統回傳 404

#### Scenario: 店家未啟用任何選項
- **WHEN** 店家 1 的啟用清單全空，呼叫 `GET /api/admin/shops/1/menu`
- **THEN** 回應中可選 sugar / ice / topping / size 池 SHALL 皆為空陣列

### Requirement: 新增品項
系統 SHALL 提供 `POST /api/admin/shops/{shopId}/categories/{categoryId}/items` 端點。支援 `drink_item_id`（引用既有 DrinkItem）或 `drink_item_name`（自動建立新 DrinkItem）二擇一，同時提供以 drink_item_id 優先。sizes SHALL 至少一個且 Price > 0。

`sugar_ids`、`ice_ids`、`topping_ids` 與 `sizes` 中的 `size_id` SHALL 全部出現在該店家的啟用清單中，否則系統 SHALL 回傳 `OPTION_NOT_ENABLED_FOR_SHOP` 錯誤，errors 字典包含對應欄位（`sugar_ids` / `ice_ids` / `topping_ids` / `sizes`）的具體錯誤訊息。預設不再「全勾全域」，前端 SHALL 從店家啟用清單預設全勾。

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

#### Scenario: sugar_id 不在店家啟用清單
- **WHEN** 管理員送出 sugar_ids 中含有店家未啟用的 SugarId=99
- **THEN** 系統 SHALL 回傳 `OPTION_NOT_ENABLED_FOR_SHOP`，errors 含 `{ "sugar_ids": ["..."] }`，且不建立品項

#### Scenario: size_id 不在店家啟用清單
- **WHEN** 管理員送出的 sizes 含有店家未啟用的 SizeId=88
- **THEN** 系統 SHALL 回傳 `OPTION_NOT_ENABLED_FOR_SHOP`，errors 含 `{ "sizes": ["..."] }`

### Requirement: 更新品項
系統 SHALL 提供 `PUT /api/admin/shops/{shopId}/categories/{categoryId}/items/{itemId}` 端點。sizes、sugar_ids、ice_ids、topping_ids SHALL 整批覆蓋（delete-then-insert）。

`sugar_ids`、`ice_ids`、`topping_ids` 與 `sizes` 中的 `size_id` SHALL 全部出現在該店家的啟用清單中，否則系統 SHALL 回傳 `OPTION_NOT_ENABLED_FOR_SHOP` 錯誤，errors 字典包含對應欄位的錯誤訊息。

#### Scenario: 成功更新品項
- **WHEN** 管理員送出有效的更新資料
- **THEN** 系統更新品項及其所有子關聯

#### Scenario: itemId 不屬於該 categoryId
- **WHEN** 管理員更新的 itemId 不屬於該 categoryId
- **THEN** 系統回傳 404

#### Scenario: ice_id 不在店家啟用清單
- **WHEN** 管理員送出 ice_ids 中含有店家未啟用的 IceId=77
- **THEN** 系統 SHALL 回傳 `OPTION_NOT_ENABLED_FOR_SHOP`，errors 含 `{ "ice_ids": ["..."] }`，且不更新品項

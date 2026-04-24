## ADDED Requirements

### Requirement: 取得店家覆寫設定
系統 SHALL 提供 `GET /api/admin/shops/{shopId}/overrides` 端點，回傳該店家的甜度與加料覆寫設定，每筆包含全域預設值（default_price, default_sort）與覆寫值（override_price, override_sort）。

#### Scenario: 成功取得覆寫設定
- **WHEN** 管理員請求 `GET /api/admin/shops/1/overrides`
- **THEN** 系統回傳 sugar_overrides 和 topping_overrides 陣列，每筆含全域預設值與覆��值

#### Scenario: 無覆寫設定
- **WHEN** 店家未設定任何覆寫
- **THEN** 系統回傳空的 sugar_overrides 和 topping_overrides 陣列

### Requirement: 更新店家覆寫設定
系統 SHALL 提供 `PUT /api/admin/shops/{shopId}/overrides` 端點，整批覆蓋覆寫設定。未包含的 sugar/topping 表示移除覆寫（delete-then-insert）。Price 若有值 SHALL >= 0。Price 和 Sort 皆可為 null，表示使用全域預設值。

#### Scenario: 成功更新覆寫
- **WHEN** 管理員送出有效的覆寫設定
- **THEN** 系統整批覆蓋 ShopSugarOverride 和 ShopToppingOverride

#### Scenario: 移除所有覆寫
- **WHEN** 管理��送出空的 sugar_overrides 和 topping_overrides 陣列
- **THEN** 系統刪除該店家所有覆寫記錄

#### Scenario: 覆寫 Price 為負數
- **WHEN** 管理員送出 price < 0 的覆���
- **THEN** ��統回傳驗證錯誤

### Requirement: 覆寫設定權限控制
覆寫設定 API SHALL 透過 `[RequireRole(MenuConstants.ShopOverride, CrudAction.X)]` 控制存取，與店家管理使用不同的權限節點。

#### Scenario: 無權限存取
- **WHEN** 管理員角色無 ShopOverride 權限
- **THEN** 系統回傳 403

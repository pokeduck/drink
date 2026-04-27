## MODIFIED Requirements

### Requirement: 取得店家覆寫設定
系統 SHALL 提供 `GET /api/admin/shops/{shopId}/overrides` 端點，回傳該店家的甜度與加料覆寫設定，每筆包含全域預設值（default_price）與覆寫值（override_price）。回應 SHALL 不再包含 `default_sort` / `override_sort` 欄位（店家內排序已移交 `admin-shop-options` 啟用表負責）。

#### Scenario: 成功取得覆寫設定
- **WHEN** 管理員請求 `GET /api/admin/shops/1/overrides`
- **THEN** 系統回傳 sugar_overrides 和 topping_overrides 陣列，每筆含 default_price 與 override_price

#### Scenario: 無覆寫設定
- **WHEN** 店家未設定任何覆寫
- **THEN** 系統回傳空的 sugar_overrides 和 topping_overrides 陣列

#### Scenario: 回應不含 sort 欄位
- **WHEN** 任何成功回應
- **THEN** 每筆覆寫物件 SHALL 不包含 `default_sort` 或 `override_sort` 欄位

### Requirement: 更新店家覆寫設定
系統 SHALL 提供 `PUT /api/admin/shops/{shopId}/overrides` 端點，整批覆蓋覆寫設定。未包含的 sugar/topping 表示移除覆寫（delete-then-insert）。Price 若有值 SHALL >= 0。Price 可為 null，表示使用全域預設值。

請求 body SHALL 不再接受 `sort` 欄位（即使送出也 SHALL 被忽略）；店家內排序已移交 `admin-shop-options` 負責。

#### Scenario: 成功更新覆寫
- **WHEN** 管理員送出有效的覆寫設定（僅含 price）
- **THEN** 系統整批覆蓋 ShopSugarOverride 和 ShopToppingOverride

#### Scenario: 移除所有覆寫
- **WHEN** 管理員送出空的 sugar_overrides 和 topping_overrides 陣列
- **THEN** 系統刪除該店家所有覆寫記錄

#### Scenario: 覆寫 Price 為負數
- **WHEN** 管理員送出 price < 0 的覆寫
- **THEN** 系統回傳驗證錯誤

#### Scenario: 請求含 sort 欄位
- **WHEN** 管理員 PUT 內含 sort 欄位
- **THEN** 系統 SHALL 忽略該欄位，不寫入 DB

### Requirement: 覆寫設定權限控制
覆寫設定 API SHALL 透過 `[RequireRole(MenuConstants.ShopOverride, CrudAction.X)]` 控制存取。覆寫設定的入口 SHALL 從原 sidemenu `/shop/override`（內建店家下拉）移至 hub layout 的子 tab `/shop/[id]/overrides`，shopId 由 route param 取得。對應 `AdminMenu` row（Id=17）的 `Endpoint` SHALL 更新為 `/shop/[id]/overrides`、`IsPermissionOnly` SHALL 設為 true（不渲染 sidemenu 但保留為權限載體）。

#### Scenario: 無權限存取 API
- **WHEN** 管理員角色無 `ShopOverride` 權限
- **THEN** 系統回傳 403

#### Scenario: 透過 hub URL 進入
- **WHEN** 管理員導向 `/shop/[id]/overrides`
- **THEN** 前端 route middleware SHALL 將該 URL 對應到 `MenuConstants.ShopOverride` 進行權限判定

#### Scenario: AdminMenu 不再渲染至 sidemenu
- **WHEN** 前端 sidemenu 渲染
- **THEN** 對應 ShopOverride 的 menu row SHALL 因 `is_permission_only=true` 不被渲染

## REMOVED Requirements

### Requirement: ~~Sort 欄位於 ShopSugarOverride / ShopToppingOverride~~
**Reason**: 店家內排序統一由 `admin-shop-options` 的啟用表 `Sort` 負責，避免 sort 散落兩處。Override 表職責收窄為「Price 覆寫」。

**Migration**:
- EF Migration 直接 drop `ShopSugarOverride.Sort` 與 `ShopToppingOverride.Sort` 欄位。
- 既有資料：目前只有 1 家店、Sort 重設成本可忽略；不做資料搬遷。
- 上線後管理員到 hub 內 `/shop/[id]/options` 設定排序。
- API DTO 同步移除 `default_sort` / `override_sort` 欄位；前端覆寫頁面移除 sort UI。

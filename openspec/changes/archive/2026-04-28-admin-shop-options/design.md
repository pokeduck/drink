## Context

兩件事在這次 change 同時發生：

1. **店家層級啟用過濾**：當前店家品項管理（`/shop/[id]/edit`）在編輯品項時把整個全域 DrinkOption pool 噴給管理員勾選。隨著系統營運，全域 pool 預期會長到每種上百筆，UI 已經無法工程化處理。
2. **店家層級設定的入口統一**：目前 `/shop/[id]/edit` 與 `/shop/[id]/images` 走「店家列表行內按鈕」入口，`/shop/override` 卻是 sidemenu + 內建店家下拉。再多一個「選項啟用」會把入口模式撕成 3 種。趁目前只有 1 家店、零遷移成本的時間點一起整理。

未來規模：1 → 50 家店；多人協作會發生並發。

現有資料模型：
- 全域 pool：`Sugar` / `Ice` / `Topping` / `Size` / `DrinkItem`
- 店家層級僅有 `ShopSugarOverride` / `ShopToppingOverride`，包含 `Price`（覆寫全域價）與 `Sort`（覆寫全域排序）；Ice/Size 無對應 override 表。
- 品項層級：`ShopMenuItemSugar` / `ShopMenuItemIce` / `ShopMenuItemTopping` / `ShopMenuItemSize`。

權限模型現況：
- `AdminMenu` row → `Endpoint != null` 視為葉節點 → `AdminMenuRoleSeeder` 給 system role 全 CRUD；前端 sidemenu 渲染所有有 Endpoint 的葉節點。
- `/shop/[id]/edit` 與 `/shop/[id]/images` 沒有對應 AdminMenu row，借用 `ShopList` 權限（route prefix → menuId 對應）。

## Goals / Non-Goals

**Goals:**
- 在店家層引入啟用過濾，讓品項編輯只看到店家啟用的選項子集，涵蓋全部四種選項。
- 把店家內排序集中到啟用表，避免 sort 散落多張表。
- 提供 dry-run preview，讓管理員在儲存前看到「我這次會連帶刪除幾個品項中的哪些選項」。
- 啟用 / 停用變更要 transactional：啟用表更新與 `ShopMenuItemXxx` cascade 同生共死。
- 把店家層級設定整合成 hub-and-spoke layout：所有 `/shop/[id]/*` 子設定有共同 navigation 殼，店家列表是唯一進入點。
- 引入「permission-only menu node」概念：menu row 用作權限載體，但不渲染 sidemenu。

**Non-Goals:**
- 「複製其他店家的啟用設定」批次工具（50 家店時可能想要，留作未來）。
- 把全域 `DrinkOption` 池本身（CRUD）搬到店家層。
- User.API 相關的菜單呈現邏輯改動（本次只動 Admin）。
- 既有店家自動 backfill 啟用清單（決議：上線即全空）。
- 把菜單品項 (`/shop/[id]/menu`) 整合進 hub（既有 edit 頁面內就含菜單；hub 升級後菜單仍留在 edit tab 內，不另開 tab）。

## Decisions

### D1：選 Option C — 啟用獨立成 4 張新表，與 override 解耦

替代方案：A. 復用 override 表加 `IsEnabled` 並補 Ice/Size override 表；B. row 存在 = 啟用（隱含語意，不加欄位）。

選 C 的理由：啟用（access control 概念）與覆寫（pricing concept）職責不同；耦合在同表會混淆「我為什麼有這筆 row」。Ice/Size 不需要 price 覆寫但需要啟用過濾，走 A 會出現只用得到 Sort 的 override 表很尷尬。B 會把現有 override 表「沒 row = 用全域」語意翻轉成「沒 row = 停用」，砸掉 `admin-shop-override` 既有契約。

### D2：啟用表的 Sort 為店家內排序唯一來源；移除 Override.Sort

替代方案：保留 `ShopSugarOverride.Sort` / `ShopToppingOverride.Sort`，啟用表不帶 Sort；僅 Sugar/Topping 可以排序、Ice/Size 沿用全域。

選擇移除 Override.Sort 的理由：4 種選項對「店家內排序」需求一致，獨厚 2 種會造成 UI 不對稱；Sort 散落兩處需要決定誰優先；統一到啟用表後 override 職責收窄為「Price 覆寫」。1 家店、Sort 重設成本接近零。

### D3：啟用表沿用 BaseDataEntity 慣例（Id PK + Unique(ShopId, OptionId)）

替代方案：複合 PK `(ShopId, OptionId)`。沿用慣例的理由：與 `ShopMenuItem*` 一致，避免 Repository pattern 與 Audit trail 出現特例；多一欄成本可忽略。

### D4：Cascade 採 dry-run preview endpoint（Method A）

替代方案：前端自己 diff 估算受影響品項數，PUT 時直接寫並回傳實際數字。選 dry-run 的理由：50 家店規模下並發誤差會出現，前端估算的「影響 N 個品項」可能在按確認的瞬間因他人改了菜單而失準；preview 也讓 MessageBox 列出「具體哪些品項會被連帶移除」而不只是一個數字。

### D5：上線即全空（不 backfill 既有店家）

替代方案：Migration 將既有 `ShopMenuItemXxx` 用過的 option ids 反推寫入啟用表。選全空的理由：目前只有 1 家既有店家，手動補設成本可承受；全空避免「自動 backfill 寫錯」的風險，第一次操作即由管理員親手把關。

### D6：權限新增 `MenuConstants.ShopOptions` 節點，與 `ShopList` / `ShopOverride` 並列

替代方案：共用 `ShopOverride` 權限節點。選新節點的理由：「啟用 / 停用一個選項」的影響面比「調整覆寫價格」大（會 cascade 刪品項中的選項）；組織內可能由不同層級角色負責；沿用既有「每葉節點獨立權限」設計慣例。

### D7：選 Shop Hub Pattern（X 路線）

替代方案：
- C1（Y 路線）：選項管理走 `/shop/[id]/options` 獨立頁，從店家列表行內按鈕進入；override 維持 sidemenu。
- D：選項管理比照 override 走 sidemenu 模式，內建店家下拉。

選 X（hub）的理由：
- 任務歸屬感最強：「我在管這家店」，子設定全在身邊。
- 店家層級設定的入口統一（基本 / 圖片 / 選項 / 覆寫），不再有 3 種入口模式。
- 業界 admin（Shopify / Stripe / Linear）通用模式，使用者直覺。
- URL 仍細粒度（`/shop/[id]/edit`、`/shop/[id]/options` 等），可單獨書籤、權限細粒度。
- 每個 tab 各自儲存、各自 useUnsavedGuard，不互相牽動。
- 目前只有 1 家店、零遷移成本，是改架構最便宜的時間點。

代價：override 從 sidemenu 撤離是 BREAKING（但既有用戶極少）。

### D8：Hub layout 實作走 Nuxt 子 route + 共用 layout component

實作方式：
- `web/apps/admin/app/pages/shop/[id].vue` 作為 layout container（含 sub-tab navigation、AppBreadcrumb、card header），透過 `<NuxtPage />` 渲染當前子 tab。
- 子頁面：`shop/[id]/edit.vue`、`shop/[id]/images.vue`、`shop/[id]/options.vue`、`shop/[id]/overrides.vue`。
- Sub-tab 元件以 `el-tabs` + `router.push` 切換，當前 active tab 由 route name 推導。
- 每個子 tab 元件內仍可獨立 useFormLayout、useUnsavedGuard、v-loading。

替代方案：用 `<el-tabs>` 加 `v-show` / `v-if` 切換 tab，但會犧牲 URL 細粒度與 deep link 能力，不採用。

### D9：引入 `AdminMenu.IsPermissionOnly` 欄位（取代 hack）

問題背景：`/shop/[id]/overrides` 與 `/shop/[id]/options` 兩個 hub tab 需要：
- 在 `menus/me` 出現（前端 route middleware 用以判定權限）。
- 系統 role 的 `AdminMenuRole` 自動含 CRUD（既有 seeder 用 `Endpoint != null` 判葉節點）。
- 不渲染為 sidemenu entry。

替代方案：
- (a) 把 `Endpoint = null` → 不會被 seeder 判為葉節點，system role 就拿不到權限，行不通。
- (b) 用約定路徑（如含 `[id]` 字樣）讓前端 sidemenu skip → 隱晦、難維護。
- (c) 拿掉 ShopOverride 權限節點併入 ShopList → 失去既有設計意圖（不同層級角色負責）。
- (d) 加 `AdminMenu.IsPermissionOnly` bool 欄位（採用）：語意清楚、Seeder 邏輯改一行、前端 sidemenu 過濾改一行、route middleware 對應表照舊，所有 menus/me consumer 行為可預測。

採 (d)。`AdminMenuRoleSeeder` 葉節點判斷由 `Endpoint != null` 改為 `Endpoint != null || IsPermissionOnly`；前端 sidemenu 過濾 `is_permission_only=true`；menus/me Response DTO 新增 `is_permission_only` 欄位。

### D10：API 路由形狀

最終決定：
- `GET    /api/admin/shops/{shopId}/options`：全域 pool + 啟用狀態 + 店家內 sort。
- `POST   /api/admin/shops/{shopId}/options/preview`：dry-run。
- `PUT    /api/admin/shops/{shopId}/options`：整批 delete-then-insert + cascade。

`PUT` body（snake_case）：
```json
{
  "sugars":   [{ "sugar_id": 1,   "sort": 10 }, ...],
  "ices":     [{ "ice_id":   2,   "sort": 20 }, ...],
  "toppings": [{ "topping_id": 3, "sort": 30 }, ...],
  "sizes":    [{ "size_id":  4,   "sort": 40 }, ...]
}
```

`POST /preview` 回應：
```json
{
  "newly_disabled": {
    "sugar_ids":  [5, 6],
    "ice_ids":    [],
    "topping_ids":[7],
    "size_ids":   []
  },
  "affected_menu_items": [
    { "id": 101, "name": "珍珠奶茶", "removed_options": { "sugars": [5], "toppings": [7] } }
  ],
  "affected_menu_items_count": 12
}
```

`PUT` 回應：`{ affected_menu_items_count: N }`。

## Risks / Trade-offs

- **[既有 1 家店上線後品項 GET 子選項全空]** → 上線在維護視窗執行，管理員上線即手動到 `/shop/[id]/options` 完成首次啟用設定；release note 明列此步驟。
- **[Cascade 刪除 ShopMenuItemXxx 不可回滾]** → dry-run preview 強制顯示影響清單；UI 文案明示「會連帶移除 N 個品項中的選項」；管理員需顯式確認才送 PUT。
- **[並發更新導致 PUT 實際影響數與 preview 不同]** → PUT 仍在 transaction 內重新計算實際差集，回傳真實影響數；UI 在 PUT 結果提示「實際移除 N 筆」。
- **[Hub 重構導致既有 `/shop/override` 死連結]** → 既有書籤或外部連結會 404。Mitigation：在 `web/apps/admin/app/pages/shop/override.vue` 改為 redirect → 引導使用者到 `/shop/list`，並提示「覆寫設定已併入店家編輯」。
- **[`IsPermissionOnly` 欄位行為未來可能誤用]** → 在 spec 中明示語意（「menu row 作為權限載體，不渲染 sidemenu，仍出現在 menus/me 供前端 route middleware 判定」），對應 Entity property 加 doc comment。
- **[Fresh install 時 `RestructureShopMenusForHub` 會炸]** → Migrator 流程是「先 migration、再 seeder」，所以資料變動的 migration 在 fresh install 時 admin_menu 是空表，UPDATE 會 0 row、INSERT id=23 會撞 FK（parent_id=9 不存在）或之後與 seeder 撞 PK。Mitigation：migration Up/Down 用 `IF EXISTS (SELECT 1 FROM admin_menu)` guard 包住所有資料變更，fresh install 跳過、改由 AdminMenuSeeder 處理；既有環境照常執行。同時加上 `ON CONFLICT DO NOTHING` 雙保險。
- **[權限新節點上線 → 既有角色無法存取]** → system role (id=1) 由 migration 補 admin_menu_role row 自動取得；其他角色需手動賦予。

## Migration Plan

1. **EF Migration `AddAdminMenuIsPermissionOnly`**：
   - `ALTER TABLE admin_menu ADD COLUMN is_permission_only boolean NOT NULL DEFAULT false`。
2. **EF Migration `AddShopEnabledOptionTables`**：
   - 新增 4 張 `ShopEnabled*` 表（Id PK、Unique(ShopId, OptionId)、Sort、audit columns）。
   - FK 到 `Shops` / 各 option pool 表，配 `OnDelete(Cascade)`。
3. **EF Migration `DropShopOverrideSort`**：
   - drop `ShopSugarOverride.Sort`、`ShopToppingOverride.Sort` 兩欄。
4. **EF Migration `RestructureShopMenusForHub`**：
   - `UPDATE admin_menu SET endpoint='/shop/[id]/overrides', is_permission_only=true WHERE id=17`。
   - `INSERT admin_menu` 新 row Id=23（選項管理，Endpoint='/shop/[id]/options', IsPermissionOnly=true, Sort=3, ParentId=9）。
   - 為 system role (id=1) `INSERT admin_menu_role` (1, 23, true, true, true, true)，使用 `ON CONFLICT DO NOTHING` 避免重跑爆掉。
   - 重設 `admin_menu` sequence。
5. **Seeder 同步**：
   - `AdminMenuSeeder.cs` 修改 Id=17 row、新增 Id=23 row、兩者 IsPermissionOnly=true。
   - `AdminMenuRoleSeeder.cs` 葉節點 Where 改為 `m.Endpoint != null || m.IsPermissionOnly`。
6. **Migrator 套用後驗證**：
   - `admin_menu` 表含 Id=23、Id=17 endpoint 為 `/shop/[id]/overrides`、`is_permission_only` 欄位存在。
   - 4 張啟用表為空。
   - 兩張 override 表 Sort 欄位已不存在。
   - System role 對 Id=23 有全 CRUD。
7. **上線手動步驟**：
   - 為唯一一家既有店家走訪 hub 的「選項啟用」tab，逐 tab 啟用所需選項並儲存。
   - 走訪「基本資訊」「圖片」「覆寫設定」tab 確認 hub layout 行為正常。

**Rollback**：
- 程式碼回滾即可（4 支 migration 都可 down）。
- 若已開始為既有店家設定啟用清單，down migration 會直接遺失該設定；上線視窗內回滾風險可接受。

## Open Questions

- 啟用表的 `Sort` 是否要在 PUT 時做唯一性檢查？目前傾向不檢查（允許重複 sort 值，UI 用 `Sort ASC, Id ASC` 排序穩定）。
- Hub sub-tab 是否需要記憶上次造訪的 tab？目前傾向不記憶（每次點店家進去都從 `/shop/[id]/edit` 開始），apply 時驗證使用體感再決定。
- 既有 `web/apps/admin/app/pages/shop/override.vue` 的處理：redirect → `/shop/list` 還是直接刪除？傾向 redirect + flash message 一段時間，下個版本移除。

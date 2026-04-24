## 1. Entity + Enum + DbContext

- [x] 1.1 建立 ShopStatus enum
- [x] 1.2 建立 Shop Entity（含 MaxToppingPerItem、MaxToppingCount）
- [x] 1.3 建立 ShopCategory Entity
- [x] 1.4 建立 ShopMenuItem Entity
- [x] 1.5 建立 ShopMenuItemSize、ShopMenuItemSugar、ShopMenuItemIce、ShopMenuItemTopping Entity
- [x] 1.6 建立 ShopSugarOverride、ShopToppingOverride Entity
- [x] 1.7 設定 DbContext OnModelCreating 關聯（FK、唯一索引）

## 2. Mapper + DTO

- [x] 2.1 建立 ShopMapper（Mapperly，List/Detail/Menu 各 Response 的映射）
- [x] 2.2 建立 Shop Response DTO（ShopListResponse、ShopDetailResponse）
- [x] 2.3 建立 Shop Menu Response DTO（AdminShopMenuResponse 含 categories/items/overrides）
- [x] 2.4 建立 Shop Override Response DTO（ShopOverrideResponse）
- [x] 2.5 建立 Shop Request DTO（Create/Update ShopRequest、BatchSortRequest、BatchDeleteRequest）
- [x] 2.6 建立 Category Request DTO（Create/Update CategoryRequest）
- [x] 2.7 建立 MenuItem Request DTO（Create/Update MenuItemRequest）
- [x] 2.8 建立 Override Request DTO（UpdateShopOverrideRequest）

## 3. Service — 後台店家 CRUD

- [x] 3.1 建立 AdminShopService：GetList（分頁、排序、keyword、status 篩選）
- [x] 3.2 AdminShopService：GetById
- [x] 3.3 AdminShopService：Create（name 唯一、MaxToppingCount 從系統設定帶入預設值）
- [x] 3.4 AdminShopService：Update（name 唯一排除自身）
- [x] 3.5 AdminShopService：Delete（soft delete 店家 + 底下品項）
- [x] 3.6 AdminShopService：BatchSort、BatchDelete

## 4. Service — 後台菜單管理

- [x] 4.1 AdminShopService：GetMenu（完整菜單含分類 + 品項 + 子關聯 + 覆寫）
- [x] 4.2 AdminShopService：CreateCategory、UpdateCategory、DeleteCategory、BatchSortCategories
- [x] 4.3 AdminShopService：CreateMenuItem（支援 drink_item_id / drink_item_name）
- [x] 4.4 AdminShopService：UpdateMenuItem（子關聯整批覆蓋）
- [x] 4.5 AdminShopService：DeleteMenuItem、BatchSortMenuItems

## 5. Service — 覆寫設定

- [x] 5.1 AdminShopService：GetOverrides（含全域預設值對照）
- [x] 5.2 AdminShopService：UpdateOverrides（整批覆蓋 delete-then-insert）

## 6. Controller

- [x] 6.1 建立 Admin.API ShopsController（店家 CRUD 7 endpoints + RequireRole）
- [x] 6.2 ShopsController 增加菜單管理 endpoints（分類 + 品項共 9 endpoints）
- [x] 6.3 ShopsController 增加覆寫設定 endpoints（2 endpoints + RequireRole ShopOverride）

## 7. ErrorCodes

- [x] 7.1 新增 Shop 相關 ErrorCodes（SHOP_ALREADY_EXISTS、CATEGORY_ALREADY_EXISTS、DRINK_ITEM_NOT_FOUND 等）

## 8. Migration + DB

- [x] 8.1 執行 /migrate AddShop 產生 migration
- [x] 8.2 執行 Migrator 更新資料庫

## 9. OpenAPI TypeScript 型別產生

- [x] 9.1 啟動 Admin.API，執行 pnpm generate 更新 admin.d.ts

## 10. 前端 — 店家列表頁

- [x] 10.1 實作 /shop/list 頁面（el-table、keyword 搜尋、status 篩選、分頁、排序）
- [x] 10.2 實作單筆刪除、批次刪除功能
- [x] 10.3 實作拖拉排序功能

## 11. 前端 — 店家新增/編輯頁

- [x] 11.1 實作 /shop/list/create 頁面 — 店家基本資訊表單
- [x] 11.2 實作 /shop/list/:id/edit 頁面 — 載入既有資料 + 基本資訊表單
- [x] 11.3 實作菜單管理區塊 — 可收折分類 + 分類拖拉排序 + 分類 CRUD
- [x] 11.4 實作品項列表 — 分類內品項拖拉排序 + 品項 expand 顯示細項
- [x] 11.5 實作品項編輯 Dialog — drink item autocomplete + 尺寸價格 + 甜度/冰塊/加料 checkbox

## 12. 前端 — 覆寫設定頁

- [x] 12.1 實作 /shop/override 頁面 — 店家選擇 + 甜度/加料覆寫表格

## Why

平台需要店家與菜單管理功能，讓後台管理員可以建立店家、設定菜單分類與品項、調整各店的加料價格覆寫。這是訂飲料平台的核心功能，飲料選項（DrinkItem、Sugar、Ice、Topping、Size）已全部完成，店家模組是接下來所有業務流程（揪團、訂單）的前置依賴。

## What Changes

- 新增 Shop Entity（含 `MaxToppingPerItem`、`MaxToppingCount` 兩個加料限制欄位）及 ShopStatus enum
- 新增 ShopCategory（菜單分類）、ShopMenuItem（品項）及其子關聯 Entity（Size/Sugar/Ice/Topping）
- 新增 ShopSugarOverride / ShopToppingOverride（店家甜度/加料價格覆寫）
- 後台 API：店家 CRUD（7 endpoints）、菜單管理（9 endpoints）、覆寫設定（2 endpoints）
- 後台前端：店家列表頁、店家新增/編輯頁（含菜單管理 UI）、覆寫設定頁
- 菜單管理 UI 採用可收折分類 + 分類內品項拖拉排序 + 品項展開檢視細項的設計
- `MaxToppingCount` 預設值從系統設定（AdminSystemSetting）帶入

## Capabilities

### New Capabilities
- `admin-shop`: 後台店家 CRUD — 列表、新增、編輯、刪除、批次排序、批次刪除
- `admin-shop-menu`: 後台菜單管理 — 分類 CRUD + 排序、品項 CRUD + 排序，內嵌於店家編輯頁
- `admin-shop-override`: 後台店家覆寫設定 — 甜度/加料的價格與排序覆寫
### Modified Capabilities

（無既有 spec 需修改）

## Impact

- **Entity 層**：新增 11 個 Entity + 1 個 Enum，需 EF Core migration
- **API 層**：Admin.API 新增 18 個 endpoints
- **前端**：admin app 新增/改寫 3-4 個頁面 + 品項編輯 dialog
- **型別產生**：需重新執行 `pnpm generate` 更新 admin.d.ts
- **權限**：ShopList(10)、ShopOverride(17) 已定義在 MenuConstants
- **依賴**：引用已完成的 DrinkItem、Sugar、Ice、Topping、Size Entity

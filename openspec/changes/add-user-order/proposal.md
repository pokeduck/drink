## Why

前台揪團功能是平台核心,但目前只有後端 admin-order capability 完成揪團查看 / 改狀態 / 取消 / 匯出 / 通知,**前台使用者完全沒辦法發起揪團、加入別人的揪團下單、編輯自己的飲料**。Client Nuxt app(`@drink/client`)的所有訂單頁面目前都依賴 `useMockData()` 假資料,沒有後端 endpoint 對接,使用者連看到揪團牆都需要寫死假資料。本 change 把前台訂單路徑的後端 API 全部打通,client UI 對接留下一個 change(`user-order-ui`)。

## What Changes

- 新增 `User.API` 下的 `/api/user/orders` 系列 endpoint:列表(揪團牆)、詳情、發起揪團、編輯揪團、加入下單、編輯飲料、刪除飲料
- 沿用既有 `GroupOrder` / `OrderItem` / `OrderItemTopping` Entity(由 admin-order capability 建立)
- 寫入時做價格快照(由 `ShopMenuItem` / `ShopMenuItemSize` / `ShopMenuItemSugar` 取當下價格寫入 `ItemPrice` / `SugarPrice` / `ToppingPrice` / `TotalPrice`)
- 權限控制:
  - 列表 / 詳情:登入即可(訪客方案另一個 capability 處理)
  - 發起揪團:登入即可
  - 編輯揪團基本資訊(title / deadline / note):僅 initiator
  - 加入下單(新增 OrderItem):登入即可,需揪團為 `Active`
  - 編輯 / 刪除 OrderItem:可由原下單人 **或** 揪團 initiator 進行;需揪團為 `Active`
  - 取消揪團:可由 initiator **或** admin(admin 走既有 admin-order endpoint,user-side 新增 `PUT /api/user/orders/{id}/cancel`)
- 沿用既有 `ErrorCodes.cs` 模組 05(Order)區段已預留的錯誤碼:`ShopNotAvailable`、`InvalidDeadline`、`NotInitiator`、`OrderNotActive`、`OrderNotFound`、`CannotCancelOrder`

## Capabilities

### New Capabilities

- `user-order`:前台訂單管理 — 揪團發起 / 編輯 / 取消、加入下單、飲料 CRUD、價格快照、權限規則

### Modified Capabilities

(無 — admin-order 的 Entity 與 spec 不變動,本 change 純新增 user 端 endpoint;`ErrorCodes` 沿用既有預留位)

## Impact

**新增程式碼**

- `api/Application/Requests/User/Order/`:CreateGroupOrderRequest、UpdateGroupOrderRequest、CreateOrderItemRequest、UpdateOrderItemRequest、UserOrderListQuery
- `api/Application/Responses/User/Order/`:UserOrderListItemResponse、UserOrderDetailResponse、UserOrderItemResponse、UserOrderItemToppingResponse
- `api/Application/Mappings/UserOrderMapper.cs`
- `api/Application/Services/UserOrderService.cs`:列表 / 詳情 / 發起 / 編輯 / 取消 / 加入 / 編輯飲料 / 刪除飲料
- `api/User.API/Controllers/OrdersController.cs`:10 個 endpoint
- 沿用 `IEmailSender`(目前無 user 端通知需求)、無新增 DI

**不變動**

- Entity(`GroupOrder` / `OrderItem` / `OrderItemTopping`):欄位、FK、index 全部沿用 admin-order capability 建立的
- 後端 Admin.API / Upload.API:零變動
- DB migration:不需要(table 已建)
- ErrorCodes.cs:不新增,沿用 4-05-XX 既有定義

**前端**

- 完成後跑 `pnpm generate` 產出 `web/internal/api-types/src/user.d.ts` 新增的 endpoint 型別
- Client Nuxt app UI 對接是另一個 change(`user-order-ui`)

**不含**

- Client Nuxt app 任何 vue 檔(留給 user-order-ui change)
- 通知(user 自發的「揪團即將截止」「飲料到貨」push)(留給 notification capability)
- 訪客瀏覽訂單牆(留給 add-relax-guest-access change)
- 揪團牆推薦演算法 / 我的最愛 / 收藏(別的 capability)

## 1. Domain（Entity + Enum + Configuration）

- [x] 1.1 新增 `api/Domain/Enums/GroupOrderStatus.cs`（Active=1, Closed=2, Delivered=3, Completed=4, Cancelled=5）
- [x] 1.2 新增 `api/Domain/Entities/GroupOrder.cs`，實作 `ICreateEntity` / `IUpdateEntity`，含 navigation `Shop`、`Initiator (User)`、`OrderItems`
- [x] 1.3 新增 `api/Domain/Entities/OrderItem.cs`，實作 `ICreateEntity` / `IUpdateEntity`，含 navigation `GroupOrder`、`User`、`MenuItem`、`Size`、`Sugar`、`Ice`、`Toppings`
- [x] 1.4 新增 `api/Domain/Entities/OrderItemTopping.cs`，實作 `ICreateEntity` / `IUpdateEntity`，含 navigation `OrderItem`、`Topping`
- [x] 1.5 在 `User.cs` 與 `Shop.cs` 等既有 Entity 補上反向 navigation（若需要列表 API 走 navigation 投影）
- [x] 1.6 新增 `api/Infrastructure/Data/Configurations/GroupOrderConfiguration.cs`：FK 設定、index（`ShopId`、`InitiatorId`、`Status`、`Deadline`）、`Title` / `Note` 長度（直接寫入 DrinkDbContext.OnModelCreating）
- [x] 1.7 新增 `api/Infrastructure/Data/Configurations/OrderItemConfiguration.cs`：FK 設定、index（`GroupOrderId`）、`decimal(10,2)` precision、`RecipientName` / `Note` 長度（直接寫入 DrinkDbContext.OnModelCreating）
- [x] 1.8 新增 `api/Infrastructure/Data/Configurations/OrderItemToppingConfiguration.cs`：FK 設定、`Price decimal(10,2)`（直接寫入 DrinkDbContext.OnModelCreating）

## 2. 套件相依

- [x] 2.1 在 `api/Application/Drink.Application.csproj` 或 `api/Infrastructure/Drink.Infrastructure.csproj`（依匯出邏輯放置處）加入 `<PackageReference Include="ClosedXML" Version="0.105.*" />`
- [x] 2.2 `dotnet restore` 確認可下載且 build 通過

## 3. Migration

- [x] 3.1 執行 `dotnet ef migrations add AddGroupOrderAndItems --project api/Infrastructure --startup-project api/Migrator --output-dir Migrations`
- [x] 3.2 檢視產生的 migration 確認 table 名稱（`group_order` / `order_item` / `order_item_topping`）、index 與 FK 是否正確
- [x] 3.3 執行 `dotnet run --project api/Migrator` 套用 migration 至本機 DB

## 4. Email Sender 擴充

- [x] 4.1 在 `api/Application/Interfaces/IEmailSender.cs` 加入 `SendOrderNotificationAsync(string to, string recipientName, string groupTitle, string shopName, string? note)`
- [x] 4.2 在 `api/Infrastructure/Services/LogEmailSender.cs` 補上對應 mock 實作（log 即可）

## 5. Request / Response DTO

- [x] 5.1 新增 `api/Application/Requests/Admin/Order/AdminOrderListQuery.cs`（分頁 / 排序 / keyword / status / shop_id / created_from / created_to / deadline_from / deadline_to）
- [x] 5.2 新增 `api/Application/Requests/Admin/Order/UpdateGroupOrderStatusRequest.cs`（`GroupOrderStatus Status`）
- [x] 5.3 新增 `api/Application/Responses/Admin/Order/AdminOrderListItemResponse.cs`（含 `id` / `title` / `shop_id` / `shop_name` / `initiator_id` / `initiator_name` / `status` / `deadline` / `order_item_count` / `total_amount` / `created_at`）
- [x] 5.4 新增 `api/Application/Responses/Admin/Order/AdminOrderDetailResponse.cs`（揪團資訊 + `order_items` 扁平列表 + `summary`）
- [x] 5.5 新增 `api/Application/Responses/Admin/Order/AdminOrderItemResponse.cs`（含 size_name / sugar_name / ice_name / toppings array / 四段價格 / quantity / note / created_at）
- [x] 5.6 新增 `api/Application/Responses/Admin/Order/AdminOrderItemToppingResponse.cs`（`topping_name`、`price`）
- [x] 5.7 新增 `api/Application/Responses/Admin/Order/AdminOrderNotifyResponse.cs`（`total_recipients` / `email_sent` / `push_skipped` / `none_skipped` / `failed`）

## 6. Mapper（Mapperly）

- [x] 6.1 新增 `api/Application/Mappings/AdminOrderMapper.cs`：定義 `OrderItem → AdminOrderItemResponse`、`OrderItemTopping → AdminOrderItemToppingResponse`
- [x] 6.2 列表 item response 不走 Mapperly（透過 EF projection 直接投影到 anonymous + 手動 map 到 `AdminOrderListItemResponse`）

## 7. Service 層

- [x] 7.1 新增 `api/Application/Services/AdminOrderService.cs` 繼承 `BaseService`
- [x] 7.2 實作 `ListAsync(AdminOrderListQuery query)`：分頁、排序、keyword（ILike `Title` 或 `Initiator.Name`）、篩選；計算欄位 `order_item_count` / `total_amount` 走 EF projection；預設排序 `CreatedAt DESC, Id ASC`，所有排序末尾 ThenBy `Id`
- [x] 7.3 實作 `GetDetailAsync(int orderId)`：載入揪團 + `Include(OrderItems → Toppings、Size、Sugar、Ice、MenuItem.DrinkItem、User)`、組 summary；ID 不存在回 `OrderNotFound`
- [x] 7.4 實作 `UpdateStatusAsync(int orderId, GroupOrderStatus target)`：查表 → 套用狀態轉換白名單（內部常數矩陣）→ 違反回 `InvalidStatusTransition` → 寫入 `Updater = CurrentUserId`
- [x] 7.5 實作 `CancelAsync(int orderId)`：僅 Active / Closed 可取消，否則 `CannotCancelOrder`；改 status = Cancelled；保留所有 OrderItem
- [x] 7.6 新增 `api/Application/Services/AdminOrderExportService.cs`：使用 ClosedXML 產出 byte[]；單一 worksheet；含 15 欄表頭；排序 RecipientName ASC, CreatedAt ASC；title 內 OS-unsafe 字元（`\ / : * ? " < > |`）替換為 `_`
- [x] 7.7 新增 `api/Application/Services/AdminOrderNotificationService.cs`：依 NotificationType 分派；Email/Both → `IEmailSender.SendOrderNotificationAsync`；WebPush → skip + log；None → skip；per-recipient try-catch；回傳統計

## 8. Controller

- [x] 8.1 新增 `api/Admin.API/Controllers/OrdersController.cs` 繼承 `BaseController`
- [x] 8.2 `GET /api/admin/orders`：呼叫 `ListAsync`，回 `ApiOk(PaginationList<AdminOrderListItemResponse>)`
- [x] 8.3 `GET /api/admin/orders/{orderId}`：呼叫 `GetDetailAsync`，404 回 `ApiError(OrderNotFound, 404)`
- [x] 8.4 `PUT /api/admin/orders/{orderId}/status`：呼叫 `UpdateStatusAsync`，Updater 透過 `ICurrentUserContext` 在 service 層自動取得
- [x] 8.5 `PUT /api/admin/orders/{orderId}/cancel`：呼叫 `CancelAsync`
- [x] 8.6 `GET /api/admin/orders/{orderId}/export`：呼叫 `AdminOrderExportService`，回 `File(bytes, contentType, fileName)`
- [x] 8.7 `POST /api/admin/orders/{orderId}/notify`：呼叫 `AdminOrderNotificationService`，回 `ApiOk(AdminOrderNotifyResponse)`
- [x] 8.8 套用既有的 admin auth attribute / `RequireRoleAttribute`（依現有 controller 模式）

## 9. 編譯與啟動驗證

- [x] 9.1 `dotnet build api/Drink.sln` 通過，零警告（或不新增警告）
- [x] 9.2 啟動 Admin.API（port 5101），Swagger UI 看得到新 endpoint（6 個 `/api/admin/orders` 路由全部出現）
- [x] 9.3 用 Swagger 測 happy path：建立兩筆假資料（直接寫 SQL 或 seed）→ 列表 / 詳情 / 改狀態 / 取消 / 匯出 Excel / 發通知（觀察 log）

## 10. 前端型別產生

- [x] 10.1 確保 Admin.API（5101）、User.API（5102）、Upload.API（5103）三個 Swagger 都活著
- [x] 10.2 從專案根目錄執行 `pnpm generate`
- [x] 10.3 確認 `web/internal/api-types/src/admin.d.ts` 已包含 `/api/admin/orders` 系列 endpoint 的型別（6 個路由全部出現）

## 11. 自我檢查與 OpenSpec 驗證

- [x] 11.1 執行 `openspec validate add-admin-order --strict` 通過
- [x] 11.2 對照 spec 內 7 個 Requirement、每個 Scenario 逐項勾選實作覆蓋
- [x] 11.3 確認後台 API 完全沒有對 OrderItem / OrderItemTopping 的 POST/PUT/DELETE endpoint
- [x] 11.4 確認 `ErrorCodes.cs` 內 4-05-XX 區段沒有被新增、修改或刪除（沿用即可）

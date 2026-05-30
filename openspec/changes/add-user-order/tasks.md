## 1. Request / Response DTO

- [ ] 1.1 新增 `api/Application/Requests/User/Order/UserOrderListQuery.cs`:Page / PageSize / SortBy / SortOrder / Keyword / ShopId / Scope("public" | "mine",預設 "public")
- [ ] 1.2 新增 `api/Application/Requests/User/Order/CreateGroupOrderRequest.cs`:Title `[Required, StringLength(100)]`、ShopId、Deadline、Note `[StringLength(500)]`
- [ ] 1.3 新增 `api/Application/Requests/User/Order/UpdateGroupOrderRequest.cs`:Title / Deadline / Note(不含 ShopId / Status)
- [ ] 1.4 新增 `api/Application/Requests/User/Order/CreateOrderItemRequest.cs`:RecipientName `[Required, StringLength(100)]`、MenuItemId、SizeId、SugarId、IceId、ToppingIds:`List<int>`、Quantity `[Range(1, 99)]`、Note `[StringLength(200)]`
- [ ] 1.5 新增 `api/Application/Requests/User/Order/UpdateOrderItemRequest.cs`:同 Create(全欄位覆寫)
- [ ] 1.6 新增 `api/Application/Responses/User/Order/UserOrderListItemResponse.cs`:含 IsMine / IsJoined,不含 InitiatorId
- [ ] 1.7 新增 `api/Application/Responses/User/Order/UserOrderDetailResponse.cs` + `UserOrderSummary`
- [ ] 1.8 新增 `api/Application/Responses/User/Order/UserOrderItemResponse.cs`:含 IsMine,不含 UserId
- [ ] 1.9 新增 `api/Application/Responses/User/Order/UserOrderItemToppingResponse.cs`:ToppingName + Price

## 2. Mapper(Mapperly)

- [ ] 2.1 新增 `api/Application/Mappings/UserOrderMapper.cs`:`OrderItem → UserOrderItemResponse`(忽略 UserId、UserName 從 User.Name 取)、`OrderItemTopping → UserOrderItemToppingResponse`
- [ ] 2.2 列表 item 不走 Mapperly,在 service 內走 EF projection,內嵌 `g.OrderItems.Any(i => i.UserId == me)` 算 is_joined

## 3. Service

- [ ] 3.1 新增 `api/Application/Services/UserOrderService.cs` 繼承 `BaseService`,注入 `IGenericRepository<GroupOrder>`、`IGenericRepository<OrderItem>`、`IGenericRepository<OrderItemTopping>`、`IGenericRepository<ShopMenuItemSize>`、`IGenericRepository<ShopSugarOverride>`、`IGenericRepository<ShopToppingOverride>`、`IGenericRepository<Sugar>`、`IGenericRepository<Topping>`、`IGenericRepository<Shop>`
- [ ] 3.2 `ListAsync(UserOrderListQuery)`:依 scope 過濾(public→Status=Active;mine→InitiatorId=me OR exists OrderItem.UserId=me);keyword / shop_id 篩選;EF projection 投影 IsMine / IsJoined 子查詢;預設排序 `CreatedAt DESC, ThenBy(Id)`;支援 sort_by id/created_at/deadline
- [ ] 3.3 `GetDetailAsync(int orderId)`:Include OrderItems→User/MenuItem→DrinkItem/Size/Sugar/Ice/Toppings→Topping;每筆 item 套 mapper 後手動補 IsMine = (i.UserId == CurrentUserId);ID 不存在回 OrderNotFound
- [ ] 3.4 `CreateGroupOrderAsync(CreateGroupOrderRequest)`:驗 `deadline > UtcNow + 5min` 否則 INVALID_DEADLINE;驗 `Shop.Status == Active` 否則 SHOP_NOT_AVAILABLE;寫入 InitiatorId=me, Status=Active;回傳新 Id
- [ ] 3.5 `UpdateGroupOrderAsync(int id, UpdateGroupOrderRequest)`:驗存在 → 驗 InitiatorId == me 否則 NOT_INITIATOR → 驗 Status == Active 否則 ORDER_NOT_ACTIVE → 更新 title/deadline/note;允許 deadline 改成更早/晚但仍驗 > UtcNow + 5min
- [ ] 3.6 `CancelGroupOrderAsync(int id)`:驗存在 → 驗 InitiatorId == me 否則 NOT_INITIATOR → 驗 status ∈ { Active, Closed } 否則 CANNOT_CANCEL_ORDER → 設 Status=Cancelled
- [ ] 3.7 `CreateItemAsync(int groupOrderId, CreateOrderItemRequest)`:驗揪團存在 + Active → 呼叫私有 `SnapshotPricesAsync` 算四段價格 → 寫 OrderItem + 多筆 OrderItemTopping(同一 transaction,可用 GenericRepository 預設 SaveChanges 或顯式 BeginTransaction)
- [ ] 3.8 `UpdateItemAsync(int groupOrderId, int itemId, UpdateOrderItemRequest)`:驗揪團 + item 存在 → 驗權限(`CanModifyItem`)→ 驗 Active → 刪除原 OrderItemTopping → 重新 `SnapshotPricesAsync` → 更新 OrderItem 欄位 → 重建 OrderItemTopping
- [ ] 3.9 `DeleteItemAsync(int groupOrderId, int itemId)`:驗揪團 + item 存在 → 驗權限 → 驗 Active → repository.Delete(item)(FK Cascade 自動清子表)
- [ ] 3.10 私有 helper `SnapshotPricesAsync(menuItemId, sizeId, sugarId, toppingIds, shopId)`:回傳 `(decimal itemPrice, decimal sugarPrice, List<(int toppingId, decimal price)> toppings, decimal toppingTotal)`;若 ShopMenuItemSize / Sugar / Topping 任一查不到回傳 null 並讓 caller 回 SHOP_NOT_AVAILABLE
- [ ] 3.11 私有 helper `CanModifyItem(OrderItem item, GroupOrder group)`:回傳 `item.UserId == CurrentUserId || group.InitiatorId == CurrentUserId`

## 4. Controller

- [ ] 4.1 新增 `api/User.API/Controllers/OrdersController.cs` 繼承 `BaseController`,`[Authorize]`
- [ ] 4.2 `GET /api/user/orders`:`ListAsync`,200 + `PaginationList<UserOrderListItemResponse>`
- [ ] 4.3 `GET /api/user/orders/{id}`:`GetDetailAsync`,404 if not found
- [ ] 4.4 `POST /api/user/orders`:`CreateGroupOrderAsync`,200 + new id;400 for SHOP_NOT_AVAILABLE / INVALID_DEADLINE
- [ ] 4.5 `PUT /api/user/orders/{id}`:`UpdateGroupOrderAsync`;404 NOT_FOUND / 403 NOT_INITIATOR / 400 ORDER_NOT_ACTIVE / INVALID_DEADLINE
- [ ] 4.6 `PUT /api/user/orders/{id}/cancel`:`CancelGroupOrderAsync`;404 / 403 / 400 CANNOT_CANCEL_ORDER
- [ ] 4.7 `POST /api/user/orders/{groupOrderId}/items`:`CreateItemAsync`;404 / 400 ORDER_NOT_ACTIVE / 400 SHOP_NOT_AVAILABLE
- [ ] 4.8 `PUT /api/user/orders/{groupOrderId}/items/{itemId}`:`UpdateItemAsync`;404 / 403 / 400
- [ ] 4.9 `DELETE /api/user/orders/{groupOrderId}/items/{itemId}`:`DeleteItemAsync`;404 / 403 / 400
- [ ] 4.10 統一狀態碼 helper:在所有 endpoint 用 `result.Error == "ORDER_NOT_FOUND" ? 404 : (result.Error == "NOT_INITIATOR" ? 403 : 400)`

## 5. 編譯與啟動驗證

- [ ] 5.1 `dotnet build api/Drink.sln` 通過,零新增警告
- [ ] 5.2 啟動 User.API(port 5102),Swagger 看到 8 個 `/api/user/orders` 路由
- [ ] 5.3 手動測 happy path(curl / Swagger):
  - 註冊/登入 user → POST 發起揪團 → 看 detail → POST 加入飲料(自己 + 代下家人各一筆)→ 看 detail 確認 is_mine 標記 → PUT 編輯自己飲料 → DELETE 自己飲料 → PUT cancel
  - 用第二個 user 試:加入飲料 → 試 PUT 編輯他人飲料(應 403)→ 試 PUT 編輯揪團(應 403)
  - 試非 Active 揪團的編輯/加入(應 400)

## 6. 前端型別產生

- [ ] 6.1 確認 Admin.API(5101)、User.API(5102)、Upload.API(5103)三個 Swagger 都活著
- [ ] 6.2 從專案根目錄執行 `pnpm generate`
- [ ] 6.3 確認 `web/internal/api-types/src/user.d.ts` 已包含 8 個 endpoint 的型別

## 7. OpenSpec 驗證

- [ ] 7.1 `openspec validate add-user-order --strict` 通過
- [ ] 7.2 對照 spec 內 9 個 Requirement、所有 Scenario 逐項勾選實作覆蓋
- [ ] 7.3 確認沒有對 admin-order spec 的修改(本 change 純新增)
- [ ] 7.4 確認沒有新增 ErrorCodes(沿用 4-05-XX 既有定義)

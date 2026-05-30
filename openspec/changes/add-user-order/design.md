## Context

`admin-order` capability 已落地後端 Entity(`GroupOrder` / `OrderItem` / `OrderItemTopping`)與 admin 端 6 個 endpoint,DB schema、Mapper、ErrorCodes 模組 05、AdminMenu 都已就緒。本 change **不動 Entity**,純粹在 `User.API` 補上前台使用者的 10 個 endpoint。

關鍵既有設施:
- `BaseService.CurrentUserId`(從 JWT NameIdentifier 拿 UserId)
- `BaseController`(User.API)的 `ApiOk` / `ApiError(httpStatus=400)`
- `IGenericRepository<T>`(`.Query` IQueryable、`.GetById(id, tracking)`、`.Insert(entity)`、`.Update(entity)`、`.Delete(entity)`,皆自動 stamp CreatedAt/Creator/UpdatedAt/Updater)
- 價格 source of truth(寫入時要查的表):
  - `ShopMenuItemSize.Price`(品項 + 尺寸售價,非 null)
  - `ShopSugarOverride.Price`(店家覆寫甜度,nullable)+ fallback `Sugar.DefaultPrice`
  - `ShopToppingOverride.Price`(店家覆寫加料,nullable)+ fallback `Topping.DefaultPrice`

User.API 既有 controller:`AuthController`、`ProfileController`、`UploadController`,所有 endpoint 走 `[Authorize]`(JWT user audience)。

## Goals / Non-Goals

### Goals

- 完整的揪團 + 飲料 CRUD,涵蓋發起、編輯、取消、加入下單、編輯飲料、刪除飲料、代下(多 recipient)
- 寫入時做價格快照,後續店家改價不影響已建訂單(admin-order spec 已要求,這 change 落實寫入路徑)
- 權限規則明確:**自己下的飲料只有自己改 / 揪團 initiator 額外擁有改全部 + 取消揪團權限**
- 與 admin-order 共用同一份狀態白名單(這 change 不改狀態流轉,使用者只能取消)

### Non-Goals

- 不做 client Nuxt UI(留 `user-order-ui` change)
- 不做訪客瀏覽訂單牆(`add-relax-guest-access` 在做)
- 不做使用者自發通知(揪團將截止、飲料到貨)
- 不做收藏 / 我的最愛 / 推薦排序
- 不開放使用者改 status(只開放 cancel;status flow 是 admin 職權)

## Decisions

### 1. 路由命名:`/api/user/orders` 而非 `/api/user/group-orders`

**Why:** 對齊 `admin-order` 的 `/api/admin/orders`,讓 client 直觀對照;`group-order` 是後端內部 entity 名稱,不必透露給 API consumer。

### 2. 列表(揪團牆)範圍:全部 Active + 我參與的全狀態

**Why:** 揪團牆主要用途是「發現可加入的揪團」,所以預設只顯示 Active;但使用者也想看「我之前參與過的訂單」,所以加入 `scope=mine` 過濾改抓「我發起的 OR 我下過飲料的,所有狀態」。

**API:**
- `GET /api/user/orders?scope=public`(預設):Status=Active
- `GET /api/user/orders?scope=mine`:`initiator_id=me OR exists order_item where user_id=me`,所有狀態

支援 keyword / shop_id / 分頁,排序預設 `CreatedAt DESC, Id ASC`。**不**支援篩選 status(public 強制 Active、mine 不限);未來如需要再擴。

### 3. 詳情:同 admin 詳情但隱藏部分欄位

**Why:** 後台 admin detail 含 4 段價格細項。User 端詳情同樣顯示飲料明細與價格(讓使用者看自己付多少),但不應該洩露其他人的 user_id(只顯示 name)。

**結構:**
- `UserOrderDetailResponse`:全部使用者可看的揪團資訊 + summary + `order_items: List<UserOrderItemResponse>`
- `UserOrderItemResponse`:含 `id` / `recipient_name` / `user_name`(填單人姓名)/ 飲料各欄 / 4 段價格 / quantity / note,**不含** user_id(privacy)
- `is_mine: bool` 欄位:標示這一筆 OrderItem 是否為當前 user 所下,讓前端決定要顯示「編輯」按鈕

### 4. 寫入路徑 = service 層計算價格快照

**Why:** 價格快照是 admin-order spec 的硬性要求,前端**不該**送價格(會被偽造)。`CreateOrderItemRequest` 只傳 `menu_item_id` / `size_id` / `sugar_id` / `ice_id` / `topping_ids: int[]` / `quantity` / `recipient_name` / `note`,service 在 DB 內查當下價格寫入。

**價格查詢順序(每筆 OrderItem):**

```
ItemPrice    = ShopMenuItemSize.Price (menu_item_id + size_id)
SugarPrice   = ShopSugarOverride.Price ?? Sugar.DefaultPrice (sugar_id)
ToppingPrice = Σ (ShopToppingOverride.Price ?? Topping.DefaultPrice)  for each topping_id
TotalPrice   = ItemPrice + SugarPrice + ToppingPrice
```

每個 topping 的 `OrderItemTopping.Price` 用 override 或 default,同樣快照。

**錯誤情境:**
- `menu_item_id` 對應的 size 沒設定 → `ShopMenuItemSize` 查不到 → 400 `ITEM_SIZE_NOT_AVAILABLE`(複用 `ShopNotAvailable=40501`)
- 任一 topping_id 或 sugar_id 不存在/未啟用 → 400 `OPTION_NOT_AVAILABLE`(複用 `ShopNotAvailable=40501`)

### 5. 加入下單:單一 endpoint 一次塞一筆 OrderItem

**Why:** 簡化心智模型 — 一個 POST 等於買一杯飲料。若要買兩種不同的口味就送兩次。前端使用 `quantity` 處理「同口味多杯」、用 `recipient_name` 處理「幫家人代下」(再送一次 POST 改 recipient_name 即可)。

**避免 batch:** Batch endpoint 雖然能減少 HTTP 次數,但會讓「部分成功」變得複雜,且降低 client UX(無法 step-by-step 增刪)。

**API:** `POST /api/user/orders/{groupOrderId}/items`,body = `CreateOrderItemRequest`。

### 6. 編輯飲料:完整覆寫式 PUT

**Why:** OrderItem 的所有可變欄位(size / sugar / ice / toppings / quantity / recipient_name / note)在 UX 上會放一個編輯表單一次改,沒有 partial update 需求。價格在 service 層**重新快照**(因為 size/sugar/topping 改了,價格也要重算當下的)。

**API:** `PUT /api/user/orders/{groupOrderId}/items/{itemId}`。

**權限:** filler(原下單人,`OrderItem.UserId == CurrentUserId`)或 initiator(`GroupOrder.InitiatorId == CurrentUserId`),否則 403 `NOT_OWNER`(複用 `NotInitiator=40503` 的中性錯誤訊息,訊息文字描述「無權編輯此飲料」)。

**狀態守:** 揪團 status 必須是 Active,否則 400 `ORDER_NOT_ACTIVE`。

### 7. 刪除飲料:DELETE,權限同編輯

**API:** `DELETE /api/user/orders/{groupOrderId}/items/{itemId}`。

**權限 + 狀態守:** 同 #6。

**級聯:** 既有 EF cascade(`OrderItem` → `OrderItemTopping`)會自動清掉子表。

### 8. 發起揪團:`POST /api/user/orders`

**Body:** `CreateGroupOrderRequest`:`title` / `shop_id` / `deadline` / `note?`。

**Service 邏輯:**
1. 驗 `deadline > UtcNow + 5 min`(避免立刻過期);違反 → 400 `INVALID_DEADLINE=40502`
2. 驗 `Shop.Status == Active`;違反 → 400 `SHOP_NOT_AVAILABLE=40501`
3. 寫入 `GroupOrder`,`InitiatorId = CurrentUserId`、`Status = Active`

**回傳:** 新建的 `GroupOrder.Id`,讓前端跳轉。

### 9. 編輯揪團:`PUT /api/user/orders/{id}`

**可改欄位:** title / deadline / note(**不能改 shop**,改 shop 等同重建)。

**權限:** 僅 initiator,否則 403 `NOT_INITIATOR=40503`。

**狀態守:** 揪團必須 Active,否則 400 `ORDER_NOT_ACTIVE=40504`(後端已預留)。

### 10. 取消揪團:`PUT /api/user/orders/{id}/cancel`

**Why:** 使用者(initiator)能自取消揪團,不必等 admin。

**權限 + 狀態守:** 僅 initiator;狀態必須 Active 或 Closed(同 admin-order spec)。

**回傳:** 200 空 body。

### 11. 列表回傳欄位 vs admin 版差異

- User 列表多 `is_mine: bool`(是否為我發起)、`is_joined: bool`(我有沒有下過飲料)
- User 列表少 `initiator_id`(只給 `initiator_name`,避免 user id 散逸)

### 12. 價格快照不過早抽 helper

**Why:** 整套價格查詢只在 Create / UpdateOrderItem 兩處用,且兩處邏輯一樣(查同樣四張表)。先在 `UserOrderService` 內部以 private method `SnapshotPricesAsync(menuItemId, sizeId, sugarId, toppingIds, shopId)` 集中,等之後 user-order-ui 有 preview 需求(下單前先算價)再抽出共用 helper。

## Risks / Trade-offs

- **價格快照查詢成本:** Create/UpdateOrderItem 每次要查 4 張表(ShopMenuItemSize + ShopSugarOverride/Sugar + ShopToppingOverride/Topping × N)。對個別下單操作不是瓶頸(O(1)),但若未來 batch 下單需注意 N+1。
  **Mitigation:** 用單一 query 帶 IN 子句撈 topping 價,不要 loop。
- **權限重複定義:** 編輯/刪除 OrderItem 的「自己 OR initiator」邏輯在 7 處(每個 UserOrderService method)會用到。
  **Mitigation:** Service 內加 private helper `CanModifyItem(orderItem, groupOrder)`,集中守。
- **`scope=mine` 查詢成本:** `initiator_id=me OR exists order_item where user_id=me` 需要 join 或 subquery,對個別 user 的訂單數量不會爆(每人最多幾百筆),但要記得用 index(現有 `OrderItem.GroupOrderId` 已 index,`UserId` 在 admin-order capability 也提到要補,本 change 不擴 schema 但會利用 EF 規劃)。
  **Mitigation:** EF 查詢用 `_orderRepo.Query.Where(g => g.InitiatorId == me || g.OrderItems.Any(i => i.UserId == me))`;index `(initiator_id)` 已有,`order_item.user_id` 在 admin-order code review 找到沒 index 但留作獨立優化 change。
- **不開放使用者改 status:** 使用者只能 cancel,要等 admin 推進至 Closed/Delivered/Completed。若後續 product 想開放發起人也能 close,再開 change。
- **`is_mine`/`is_joined` 計算需 N+1 風險:** 列表頁每筆 GroupOrder 要 join 一次 OrderItem 看當前 user 有沒有下單。
  **Mitigation:** 用 EF projection 內嵌 `g.OrderItems.Any(i => i.UserId == me)` 子查詢,EF Core 10 可轉為 SQL EXISTS,單一 query。

## Migration Plan

- 本 change **不需 DB migration**(table 與 FK / index 都在 admin-order 已建)
- 不需 seed 變動
- 完成後在根目錄跑 `pnpm generate` 產出 `web/internal/api-types/src/user.d.ts`,client app 之後 import
- 既有 admin-order endpoint 不受影響,可獨立部署

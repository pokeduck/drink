## ADDED Requirements

### Requirement: 揪團列表 API

系統 SHALL 提供 `GET /api/user/orders` 列出揪團,支援:

- `scope=public`(預設):僅回傳 `Status=Active` 的揪團(公開揪團牆)
- `scope=mine`:回傳「我發起」OR「我下過飲料」的揪團(不限 status)
- 共同支援 `page`、`page_size`、`keyword`(比對 `Title`、`Initiator.Name`)、`shop_id`、`sort_by` ∈ { `id`, `created_at`, `deadline` }、`sort_order` ∈ { `asc`, `desc` }

預設排序:`CreatedAt DESC, Id ASC`,所有排序末尾 ThenBy `Id`。

回傳 list item SHALL 包含:`id`、`title`、`shop_id`、`shop_name`、`initiator_name`、`status`、`deadline`、`order_item_count`、`total_amount`、`is_mine`(boolean,當前 user 是否為 initiator)、`is_joined`(boolean,當前 user 是否已下過 OrderItem)、`created_at`。

#### Scenario: 預設僅看 Active
- **WHEN** 使用者呼叫 `GET /api/user/orders`
- **THEN** 回傳 status=Active 的揪團,依預設排序

#### Scenario: 切換到 mine
- **WHEN** 使用者呼叫 `GET /api/user/orders?scope=mine`
- **THEN** 回傳「`initiator_id=me` OR `exists order_item where user_id=me`」的所有揪團,不限 status

#### Scenario: is_mine / is_joined 標記
- **WHEN** 列表內某筆揪團由當前 user 發起且自己下過飲料
- **THEN** 對應 list item 的 `is_mine=true`、`is_joined=true`

### Requirement: 揪團詳情 API

系統 SHALL 提供 `GET /api/user/orders/{id}` 回傳揪團資訊、扁平 `order_items` 列表、`summary`(`total_items`、`total_amount`、`recipient_count`)。

`order_items` 預設排序 `CreatedAt ASC`;每筆 OrderItem 含 `id`、`user_name`(填單人姓名,**不**回傳 user_id)、`recipient_name`、`menu_item_name`、`size_name`、`sugar_name`、`ice_name`、`toppings`(陣列含 `topping_name` + `price`)、四段價格、`quantity`、`note`、`created_at`、`is_mine`(該筆是否為當前 user 所下)。

#### Scenario: 不存在的揪團
- **WHEN** 呼叫 `GET /api/user/orders/99999`
- **THEN** 回傳 404,`error = "ORDER_NOT_FOUND"`

#### Scenario: 我下的飲料標記
- **WHEN** 呼叫 `GET /api/user/orders/{id}` 對含當前 user 所下飲料的揪團
- **THEN** 對應 OrderItem 的 `is_mine=true`,其餘 `is_mine=false`

### Requirement: 發起揪團 API

系統 SHALL 提供 `POST /api/user/orders` 由登入 user 發起新揪團。Request body 含 `title (string 100, required)`、`shop_id (int, required)`、`deadline (DateTime, required)`、`note (string 500, nullable)`。

成功建立後 `InitiatorId = CurrentUserId`、`Status = Active`,回傳 `data.id` 為新建 GroupOrder 的 Id。

#### Scenario: 發起成功
- **WHEN** 登入 user 對 `Status=Active` 的店家 `shop_id=1` 呼叫 `POST /api/user/orders`,body 合法且 `deadline > UtcNow + 5 min`
- **THEN** 回傳 200,新揪團寫入 DB,`data.id` 為新 GroupOrder.Id

#### Scenario: 店家未上架
- **WHEN** `shop_id` 對應店家 `Status != Active`
- **THEN** 回傳 400,`error = "SHOP_NOT_AVAILABLE"`

#### Scenario: deadline 太近或過去
- **WHEN** `deadline <= UtcNow + 5 min`
- **THEN** 回傳 400,`error = "INVALID_DEADLINE"`

### Requirement: 編輯揪團 API

系統 SHALL 提供 `PUT /api/user/orders/{id}` 由 initiator 更新揪團 `title` / `deadline` / `note`;**禁止**變更 `shop_id` 與 `status`。

#### Scenario: 非 initiator 不可編輯
- **WHEN** 非 initiator 的登入 user 呼叫 `PUT /api/user/orders/{id}`
- **THEN** 回傳 403,`error = "NOT_INITIATOR"`

#### Scenario: 揪團非 Active 不可編輯
- **WHEN** initiator 對 `Status != Active` 的揪團呼叫 PUT
- **THEN** 回傳 400,`error = "ORDER_NOT_ACTIVE"`

#### Scenario: 編輯成功
- **WHEN** initiator 對 Active 揪團 PUT 合法欄位
- **THEN** 回傳 200,DB 內 title/deadline/note 更新,`UpdatedAt`/`Updater` 由 repository 自動 stamp

### Requirement: 取消揪團 API (user)

系統 SHALL 提供 `PUT /api/user/orders/{id}/cancel` 由 initiator 自取消揪團。狀態守同 admin-order:僅 `Active` 或 `Closed` 可取消,其餘狀態回 400 `CANNOT_CANCEL_ORDER`。

#### Scenario: initiator 取消 Active
- **WHEN** initiator 對 Active 揪團呼叫 `PUT /cancel`
- **THEN** 回傳 200,Status 寫為 `Cancelled`,OrderItem 仍保留

#### Scenario: 非 initiator 不可取消
- **WHEN** 非 initiator 呼叫 cancel
- **THEN** 回傳 403,`error = "NOT_INITIATOR"`

#### Scenario: Delivered 不可取消
- **WHEN** initiator 對 Delivered 揪團呼叫 cancel
- **THEN** 回傳 400,`error = "CANNOT_CANCEL_ORDER"`

### Requirement: 加入下單 API

系統 SHALL 提供 `POST /api/user/orders/{groupOrderId}/items` 由登入 user 在 Active 揪團內新增一筆 OrderItem。Request body 含 `recipient_name (string 100, required)`、`menu_item_id`、`size_id`、`sugar_id`、`ice_id`、`topping_ids: int[]`、`quantity (int, ≥ 1)`、`note (string 200, nullable)`。

Service SHALL 在寫入前自 DB 查當下價格,寫入快照欄位:

- `ItemPrice = ShopMenuItemSize.Price`
- `SugarPrice = ShopSugarOverride.Price ?? Sugar.DefaultPrice`
- 對每個 topping_id:`OrderItemTopping.Price = ShopToppingOverride.Price ?? Topping.DefaultPrice`
- `ToppingPrice = Σ topping prices`
- `TotalPrice = ItemPrice + SugarPrice + ToppingPrice`

`OrderItem.UserId = CurrentUserId`。

#### Scenario: 加入成功
- **WHEN** 登入 user 對 Active 揪團 `POST /items`,body 合法
- **THEN** 回傳 200,新 OrderItem + OrderItemTopping 寫入,四段價格皆為當下店家設定的快照

#### Scenario: 揪團非 Active 不可加入
- **WHEN** 對 `Status != Active` 的揪團 POST
- **THEN** 回傳 400,`error = "ORDER_NOT_ACTIVE"`

#### Scenario: 不存在的揪團
- **WHEN** 對不存在的 `groupOrderId` POST
- **THEN** 回傳 404,`error = "ORDER_NOT_FOUND"`

#### Scenario: 品項或選項不存在/未啟用
- **WHEN** `menu_item_id` / `size_id` 對應 `ShopMenuItemSize` 不存在,或任一 `topping_id` / `sugar_id` 在店家未啟用
- **THEN** 回傳 400,`error = "SHOP_NOT_AVAILABLE"`

### Requirement: 編輯 OrderItem API

系統 SHALL 提供 `PUT /api/user/orders/{groupOrderId}/items/{itemId}` 全欄位覆寫 OrderItem。Body 同 Create。

Service SHALL 重新快照價格(因 size/sugar/topping 可能改變),並刪除原 OrderItemTopping 後重建。

權限:OrderItem.UserId == CurrentUserId(原下單人) OR GroupOrder.InitiatorId == CurrentUserId(揪團發起人)。違反 → 403 `NOT_INITIATOR`(語意涵蓋「非 owner 也非 initiator」)。

狀態守:GroupOrder.Status == Active,否則 400 `ORDER_NOT_ACTIVE`。

#### Scenario: 原下單人編輯成功
- **WHEN** 飲料的下單 user 編輯自己的 OrderItem
- **THEN** 回傳 200,所有欄位 + 四段價格更新為當下快照,toppings 子表重建

#### Scenario: Initiator 編輯他人飲料
- **WHEN** GroupOrder 的 initiator 編輯非自己下的 OrderItem
- **THEN** 回傳 200(因 initiator 擁有改全部的權)

#### Scenario: 第三方使用者無權編輯
- **WHEN** 既非原下單人也非 initiator 的 user 編輯 OrderItem
- **THEN** 回傳 403,`error = "NOT_INITIATOR"`

#### Scenario: 揪團非 Active 不可編輯
- **WHEN** 對 `Status != Active` 的揪團內 OrderItem PUT
- **THEN** 回傳 400,`error = "ORDER_NOT_ACTIVE"`

### Requirement: 刪除 OrderItem API

系統 SHALL 提供 `DELETE /api/user/orders/{groupOrderId}/items/{itemId}`。權限與狀態守同 PUT(原下單人或 initiator;Active only)。

級聯:DB FK Cascade(OrderItem → OrderItemTopping)自動移除子表。

#### Scenario: 原下單人刪除成功
- **WHEN** 飲料的下單 user 刪除自己的 OrderItem
- **THEN** 回傳 200,OrderItem 與 OrderItemTopping 移除

#### Scenario: Initiator 刪除他人飲料
- **WHEN** Initiator 刪除非自己下的 OrderItem
- **THEN** 回傳 200

#### Scenario: 第三方使用者無權刪除
- **WHEN** 既非原下單人也非 initiator 的 user DELETE
- **THEN** 回傳 403,`error = "NOT_INITIATOR"`

### Requirement: 錯誤碼

系統 SHALL 沿用 `ErrorCodes.cs` 模組 05 區段定義的錯誤碼,本 capability 使用以下:

| Code | Error string | 使用情境 |
|------|--------------|---------|
| 40501 | `SHOP_NOT_AVAILABLE` | 發起揪團時店家未上架、或下單時品項/選項未啟用 |
| 40502 | `INVALID_DEADLINE` | 發起揪團 deadline ≤ UtcNow + 5min |
| 40503 | `NOT_INITIATOR` | 編輯揪團 / 取消揪團 / 編輯他人飲料時非 initiator(且非原下單人) |
| 40504 | `ORDER_NOT_ACTIVE` | 編輯揪團 / 加入下單 / 編輯飲料 / 刪除飲料時揪團非 Active |
| 40506 | `CANNOT_CANCEL_ORDER` | 取消揪團時 status ∉ { Active, Closed } |
| 40507 | `ORDER_NOT_FOUND` | 揪團 / OrderItem 不存在 |

回傳格式:404 表「資源不存在」;403 表「無權限」;400 表「狀態不對 / 商業規則違反」。

#### Scenario: 錯誤碼一致性
- **WHEN** 任何 user-order endpoint 回傳 4-05-XX 錯誤
- **THEN** Response 含 `code = 405XX`、`error = "<UPPER_SNAKE>"`、`message` 為人類可讀繁體中文

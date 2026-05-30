# admin-order

## Purpose

後台訂單管理 capability。定義前後台共用的訂單 Entity（`GroupOrder` / `OrderItem` / `OrderItemTopping` / `GroupOrderStatus` enum），以及後台 Admin Order API：列表（分頁/排序/搜尋/篩選）、詳情、狀態流轉、取消、Excel 匯出、Email 通知。前台揪團發起與飲料下單流程屬於另一個 capability（`user-order`），後台僅查看明細不提供編輯/刪除。

## Requirements

### Requirement: GroupOrder Entity Structure

系統 SHALL 維護 `GroupOrder`、`OrderItem`、`OrderItemTopping` 三個前後台共用 Entity，以及 `GroupOrderStatus` enum。

`GroupOrder` 欄位：`Id`、`Title (string 100, required)`、`ShopId (FK Shop)`、`InitiatorId (FK User)`、`Status (GroupOrderStatus)`、`Deadline (DateTime)`、`Note (string 500, nullable)`,並實作 `ICreateEntity` (`CreatedAt`, `Creator`) 與 `IUpdateEntity` (`UpdatedAt`, `Updater`)。

`OrderItem` 欄位:`Id`、`GroupOrderId (FK)`、`UserId (FK User)`、`RecipientName (string 100, required)`、`MenuItemId (FK ShopMenuItem)`、`SizeId`、`SugarId`、`IceId`、`ItemPrice / SugarPrice / ToppingPrice / TotalPrice (decimal(10,2))`、`Quantity (int, ≥ 1)`、`Note (string 200, nullable)`,並實作 `ICreateEntity` / `IUpdateEntity`。

`OrderItemTopping` 欄位:`Id`、`OrderItemId (FK)`、`ToppingId (FK Topping)`、`Price (decimal(10,2))`,並實作 `ICreateEntity` / `IUpdateEntity`。

`GroupOrderStatus` enum:`Active = 1`、`Closed = 2`、`Delivered = 3`、`Completed = 4`、`Cancelled = 5`。

#### Scenario: Entity 透過 RegisterAllEntities 自動掃描註冊
- **WHEN** 啟動 API 並掃描 `Drink.Domain.Entities` namespace
- **THEN** `GroupOrder` / `OrderItem` / `OrderItemTopping` 自動成為 `DbContext` 上可查詢的 entity,無需手動 `DbSet`

#### Scenario: TotalPrice 等於三段價格相加(單件小計)
- **WHEN** 建立或更新 `OrderItem`
- **THEN** `TotalPrice = ItemPrice + SugarPrice + ToppingPrice`,由 Service 層計算後寫入,不靠 DB computed column

### Requirement: 價格快照

系統 SHALL 在 `OrderItem` 與 `OrderItemTopping` 建立或更新時,將當下店家設定的品項尺寸價、甜度加價、加料加價快照到對應欄位,後續店家修改價格不影響已成立的訂單。

#### Scenario: 店家漲價不影響歷史訂單
- **WHEN** 訂單 A 建立後,店家將某品項尺寸價從 30 改為 35
- **THEN** 訂單 A 內該品項的 `ItemPrice` 仍為 30,列表 `total_amount` 與詳情 `total_price` 亦維持 30

### Requirement: 狀態流轉規則

系統 SHALL 強制以下狀態轉換白名單:

- `Active` → `Closed`、`Delivered`、`Cancelled`
- `Closed` → `Active`、`Delivered`、`Cancelled`
- `Delivered` → `Active`、`Closed`、`Completed`
- `Completed` → (終態,不可轉換)
- `Cancelled` → (終態,不可轉換)

違反白名單的 transition 回傳 `400` + `InvalidStatusTransition` 錯誤碼。

#### Scenario: Active 可推進至 Closed
- **WHEN** 後台 `PUT /api/admin/orders/{id}/status` body `{ "status": 2 }` 對 Active 揪團
- **THEN** 回傳 200,Entity 的 `Status` 寫為 `Closed`,`Updater` 寫為 AdminUserId

#### Scenario: Completed 不可回退
- **WHEN** 後台對 Completed 揪團呼叫 `PUT /status`,目標為 `Delivered`
- **THEN** 回傳 400,`error = "INVALID_STATUS_TRANSITION"`,Entity 狀態保持不變

#### Scenario: Cancelled 不可被改回 Active
- **WHEN** 後台對 Cancelled 揪團呼叫 `PUT /status`,目標為 `Active`
- **THEN** 回傳 400,`error = "INVALID_STATUS_TRANSITION"`

### Requirement: 取消揪團

系統 SHALL 提供 `PUT /api/admin/orders/{id}/cancel` 將揪團設為 `Cancelled`,且僅 `Active` 或 `Closed` 狀態允許取消,其餘狀態回 `400` + `CannotCancelOrder`。

取消後 `OrderItem` 與 `OrderItemTopping` 保留不刪除。

#### Scenario: Active 可取消
- **WHEN** 後台對 Active 揪團呼叫 `PUT /cancel`
- **THEN** 回傳 200,`Status = Cancelled`,飲料明細仍存在

#### Scenario: Delivered 不可取消
- **WHEN** 後台對 Delivered 揪團呼叫 `PUT /cancel`
- **THEN** 回傳 400,`error = "CANNOT_CANCEL_ORDER"`

#### Scenario: Completed 不可取消
- **WHEN** 後台對 Completed 揪團呼叫 `PUT /cancel`
- **THEN** 回傳 400,`error = "CANNOT_CANCEL_ORDER"`

### Requirement: 揪團列表 API

系統 SHALL 提供 `GET /api/admin/orders` 列出揪團,支援分頁(`page`、`page_size`)、排序(`sort_by`、`sort_order`,至少支援 `id`、`created_at`、`deadline`)、關鍵字搜尋(`keyword` 比對 `Title` 與 `Initiator.Name`,case-insensitive)、篩選(`status`、`shop_id`、`created_from`、`created_to`、`deadline_from`、`deadline_to`)。

回傳 list item 必須包含:`id`、`title`、`shop_id`、`shop_name`、`initiator_id`、`initiator_name`、`status`、`deadline`、`order_item_count`、`total_amount`、`created_at`。

預設排序:`CreatedAt DESC, Id ASC`。

#### Scenario: 預設分頁
- **WHEN** 呼叫 `GET /api/admin/orders` 不帶任何參數
- **THEN** 回傳第 1 頁、每頁 20 筆,依 `CreatedAt DESC, Id ASC` 排序

#### Scenario: keyword 搜尋揪團標題或發起人姓名
- **WHEN** 呼叫 `GET /api/admin/orders?keyword=wayne`
- **THEN** 回傳 Title 或 InitiatorName 包含 "wayne" 的揪團(不分大小寫)

#### Scenario: 過期排序穩定性
- **WHEN** 呼叫 `GET /api/admin/orders?sort_by=deadline&sort_order=asc`
- **THEN** 主排序 `Deadline ASC`,次要 `ThenBy(g => g.Id)` 確保結果穩定

### Requirement: 揪團詳情 API

系統 SHALL 提供 `GET /api/admin/orders/{id}` 回傳揪團資訊、扁平的 `order_items` 列表(包含填單人、收件人、品項名、size/sugar/ice 名稱、加料明細、四段價格、數量、備註、建立時間)、以及 `summary`(`total_items`、`total_amount`、`recipient_count`)。

`order_items` 預設排序:`CreatedAt ASC`。分組顯示(依 `recipient_name`)由前端處理,後端不做分組。

#### Scenario: 不存在的揪團
- **WHEN** 呼叫 `GET /api/admin/orders/99999` 不存在的 ID
- **THEN** 回傳 404,`error = "ORDER_NOT_FOUND"`

#### Scenario: 揪團含飲料明細
- **WHEN** 呼叫 `GET /api/admin/orders/{id}` 對存在且有飲料的揪團
- **THEN** 回傳 200,`data.order_items` 為扁平 array、`summary.total_items` 等於 array 長度、`summary.recipient_count` 等於 `recipient_name` distinct 數量

### Requirement: 匯出 Excel

系統 SHALL 提供 `GET /api/admin/orders/{id}/export` 匯出單張 Excel,Content-Type `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`,檔名 `{title}_{yyyyMMdd}.xlsx`(title 含 OS-unsafe 字元以底線替換)。

Excel 內容:單一工作表,欄位至少包含「填單人」、「收件人」、「品項」、「尺寸」、「甜度」、「冰塊」、「加料」、「品項價」、「甜度加價」、「加料加價」、「小計」、「數量」、「實付金額」、「備註」、「建立時間」。

排序:先 `RecipientName ASC`、再 `CreatedAt ASC`。

實作 SHALL 使用 ClosedXML 套件。

#### Scenario: 揪團不存在
- **WHEN** 呼叫 `GET /api/admin/orders/99999/export`
- **THEN** 回傳 404,`error = "ORDER_NOT_FOUND"`

#### Scenario: 揪團存在但無飲料明細
- **WHEN** 呼叫 `GET /api/admin/orders/{id}/export` 對空揪團
- **THEN** 回傳 200 + 一個只含表頭的 Excel 檔

#### Scenario: title 含特殊字元
- **WHEN** 揪團 title 為 `下午茶 / 五點 ?`
- **THEN** 下載檔名為 `下午茶_五點__yyyyMMdd.xlsx`(不安全字元以 `_` 替換)

### Requirement: 發送通知 API

系統 SHALL 提供 `POST /api/admin/orders/{id}/notify` 將通知發送給所有訂單參與者(去重後依 `UserId`),依各 User 的 `NotificationType` 決定通道:

- `Email`、`Both`:透過 `IEmailSender.SendOrderNotificationAsync` 發送 email
- `WebPush`:本期 skip,並 log `PUSH_NOT_IMPLEMENTED`
- `None`:skip 不發送

回傳統計:`total_recipients`、`email_sent`、`push_skipped`、`none_skipped`、`failed`。

單筆 email send 失敗時 SHALL 累計到 `failed` 計數並 log,整支 API 仍回 200,不因部分失敗而 throw。

#### Scenario: 揪團不存在
- **WHEN** 呼叫 `POST /api/admin/orders/99999/notify`
- **THEN** 回傳 404,`error = "ORDER_NOT_FOUND"`

#### Scenario: 三種 NotificationType 混合
- **WHEN** 揪團有 5 位參與者,3 位 `Email` / 1 位 `WebPush` / 1 位 `None`
- **THEN** 回傳 200,`data = { total_recipients: 5, email_sent: 3, push_skipped: 1, none_skipped: 1, failed: 0 }`

#### Scenario: Email 發送失敗不影響其他人
- **WHEN** 揪團有 3 位 `Email` 參與者,第 2 位的 `SendOrderNotificationAsync` throw exception
- **THEN** 回傳 200,`data.email_sent = 2, data.failed = 1`,其他兩位仍正常送出

### Requirement: 後台僅查看飲料明細

系統 SHALL NOT 在後台 API 提供新增、編輯或刪除 `OrderItem` 與 `OrderItemTopping` 的 endpoint。

修改飲料的權責在前台 user-order capability,後台僅負責管理揪團本身(status / cancel)與系統性操作(匯出、通知)。

#### Scenario: 後台無編輯飲料的路由
- **WHEN** 檢視 `/api/admin/orders/*` 的所有路由
- **THEN** 不存在任何對 `OrderItem` 的 POST/PUT/DELETE endpoint

### Requirement: 錯誤碼

系統 SHALL 沿用 `ErrorCodes.cs` 模組 05(Order)區段定義的錯誤碼,並至少使用以下:

| Code | Error string | 使用情境 |
|------|--------------|---------|
| 40505 | `INVALID_STATUS_TRANSITION` | `PUT /status` 違反白名單 |
| 40506 | `CANNOT_CANCEL_ORDER` | `PUT /cancel` 對不可取消狀態 |
| 40507 | `ORDER_NOT_FOUND` | 揪團 ID 不存在 |

`OrderNotActive`、`NotInitiator`、`InvalidDeadline`、`ShopNotAvailable` 等其他模組 05 錯誤碼留給前台 user-order capability 使用,本 capability 不涉及。

#### Scenario: 錯誤回傳格式
- **WHEN** API 回傳 4-05-XX 錯誤
- **THEN** Response 包含 `code = 405XX`、`error = "<UPPER_SNAKE>"`、`message` 為人類可讀中文訊息

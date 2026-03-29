# Spec: 後台訂單管理 (Admin Order)

## Objective
- 後台管理員查看與管理前台揪團訂單
- 提供揪團列表（分頁、搜尋、篩選）、查看詳情（含參與者點餐明細）、修改狀態、取消揪團、匯出 Excel
- 揪團與訂單明細為前後台共用資料表，Entity 定義於本 spec，前台 spec 引用

---

## Entities（前後台共用）

### GroupOrder（揪團）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| Title | string(100) | ✅ | 揪團標題（如「下午茶揪起來」） |
| ShopId | int | ✅ | FK → Shop |
| InitiatorId | int | ✅ | FK → User（發起人） |
| Status | GroupOrderStatus (enum) | ✅ | 揪團狀態 |
| Deadline | DateTime | ✅ | 截止時間 |
| Note | string(500) | ❌ | 揪團備註 |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity），= InitiatorId |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### GroupOrderStatus (Enum)
```csharp
public enum GroupOrderStatus
{
    Active = 1,       // 揪團進行中
    Closed = 2,       // 揪團截止
    Delivered = 3,    // 飲料已送達
    Completed = 4,    // 已結束
    Cancelled = 5     // 已取消
}
```

**狀態流轉規則：**
```
Active ↔ Closed ↔ Delivered → Completed
Active / Closed → Cancelled
```
- Active、Closed、Delivered 之間可來回切換
- Completed 不可回退（已結束）
- Cancelled 不可回退（已取消）
- Active 和 Closed 可取消
- Delivered 和 Completed 不可取消

### OrderItem（訂單明細）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| GroupOrderId | int | ✅ | FK → GroupOrder |
| UserId | int | ✅ | FK → User（點餐者） |
| RecipientName | string(100) | ✅ | 收件人名稱，未填則帶入點餐者 User.Name |
| MenuItemId | int | ✅ | FK → ShopMenuItem（品項） |
| SizeId | int | ✅ | FK → Size |
| SugarId | int | ✅ | FK → Sugar |
| IceId | int | ✅ | FK → Ice |
| ItemPrice | decimal | ✅ | 品項尺寸價格（下單時快照） |
| SugarPrice | decimal | ✅ | 甜度加價（下單時快照） |
| ToppingPrice | decimal | ✅ | 加料總加價（下單時快照） |
| TotalPrice | decimal | ✅ | 小計 = ItemPrice + SugarPrice + ToppingPrice |
| Quantity | int | ✅ | 數量，預設 1 |
| Note | string(200) | ❌ | 備註（如「少冰改去冰」） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity），= UserId |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### OrderItemTopping（訂單明細加料）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| OrderItemId | int | ✅ | FK → OrderItem |
| ToppingId | int | ✅ | FK → Topping |
| Price | decimal | ✅ | 加料價格（下單時快照） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

---

## Relationships
- `GroupOrder` → `Shop`：多對一
- `GroupOrder` → `User`（InitiatorId）：多對一
- `GroupOrder` → `OrderItem`：一對多
- `OrderItem` → `User`（UserId）：多對一
- `OrderItem` → `ShopMenuItem`：多對一
- `OrderItem` → `Size`：多對一
- `OrderItem` → `Sugar`：多對一
- `OrderItem` → `Ice`：多對一
- `OrderItem` → `OrderItemTopping`：一對多
- `OrderItemTopping` → `Topping`：多對一

---

## Business Rules

### 價格快照
- 訂單送出時，將當下的品項價格、甜度加價、加料加價快照到 OrderItem / OrderItemTopping
- 後續店家修改價格不影響已成立的訂單
- `TotalPrice = ItemPrice + SugarPrice + ToppingPrice`（單件小計）
- 實際金額 = `TotalPrice × Quantity`

### 飲料修改規則
- 揪團 Active 且未過截止時間：前台可編輯/刪除飲料（詳見 user-order.md）
- 揪團截止後（Closed / Delivered / Completed / Cancelled）：不可修改飲料
- 取消揪團後，飲料明細保留不刪除
- 後台不提供飲料編輯/刪除功能，僅查看

### 狀態管理
- 後台管理員可修改揪團狀態
- Active ↔ Closed ↔ Delivered 可來回切換
- Completed / Cancelled 不可回退
- Active / Closed 可取消，Delivered / Completed 不可取消
- 狀態修改時 `Updater` = AdminUserId

### 通知
- 後台管理員可手動對揪團發送通知給所有參與者
- 通知方式依各參與者的 NotificationType 設定，None 不發送
- 通知紀錄寫入通知資料表（後台通知管理 spec 另行定義）

### 截止時間
- 系統不自動改狀態，由發起人或後台管理員手動操作
- 前端可依截止時間顯示已過期提示

---

## Code Style

```csharp
public class GroupOrder : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(100)]
    public string Title { get; set; }

    public int ShopId { get; set; }
    public Shop Shop { get; set; }

    public int InitiatorId { get; set; }
    public User Initiator { get; set; }

    public GroupOrderStatus Status { get; set; }

    public DateTime Deadline { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }

    public ICollection<OrderItem> OrderItems { get; set; }
}

public class OrderItem : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int GroupOrderId { get; set; }
    public GroupOrder GroupOrder { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    [StringLength(100)]
    public string RecipientName { get; set; }

    public int MenuItemId { get; set; }
    public ShopMenuItem MenuItem { get; set; }

    public int SizeId { get; set; }
    public Size Size { get; set; }

    public int SugarId { get; set; }
    public Sugar Sugar { get; set; }

    public int IceId { get; set; }
    public Ice Ice { get; set; }

    public decimal ItemPrice { get; set; }
    public decimal SugarPrice { get; set; }
    public decimal ToppingPrice { get; set; }
    public decimal TotalPrice { get; set; }

    public int Quantity { get; set; }

    [StringLength(200)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }

    public ICollection<OrderItemTopping> Toppings { get; set; }
}

public class OrderItemTopping : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int OrderItemId { get; set; }
    public OrderItem OrderItem { get; set; }

    public int ToppingId { get; set; }
    public Topping Topping { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}
```

---

## API Endpoints

### 揪團列表
```
GET /api/admin/orders?page=1&page_size=20&sort_by=created_at&sort_order=desc&keyword=wayne&status=1&shop_id=1&created_from=2025-01-01&created_to=2025-03-31&deadline_from=2025-01-01&deadline_to=2025-03-31
```
- 遵循 SPEC.md 列表通用規範（分頁、排序、搜尋、篩選）
- keyword 搜尋欄位：title, initiator name
- 篩選條件：status, shop_id, created_from, created_to, deadline_from, deadline_to
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "title": "下午茶揪起來",
        "shop_id": 1,
        "shop_name": "50嵐",
        "initiator_id": 1,
        "initiator_name": "Wayne",
        "status": 1,
        "deadline": "2025-03-15T15:00:00Z",
        "order_item_count": 8,
        "total_amount": 520,
        "created_at": "2025-03-15T10:00:00Z"
      }
    ],
    "total": 50,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

### 取得揪團詳情
```
GET /api/admin/orders/{orderId}
```
- 回傳揪團資訊 + 所有飲料明細（扁平列表）
- 分組顯示由前端處理（按 recipient_name 分組）
- Response:
```json
{
  "data": {
    "id": 1,
    "title": "下午茶揪起來",
    "shop_id": 1,
    "shop_name": "50嵐",
    "initiator_id": 1,
    "initiator_name": "Wayne",
    "status": 1,
    "deadline": "2025-03-15T15:00:00Z",
    "note": "三點前要點完喔",
    "created_at": "2025-03-15T10:00:00Z",
    "updated_at": "2025-03-15T10:00:00Z",
    "order_items": [
      {
        "id": 1,
        "user_id": 2,
        "user_name": "Alice",
        "recipient_name": "Alice",
        "menu_item_name": "珍珠奶茶",
        "size_name": "大杯",
        "sugar_name": "半糖",
        "ice_name": "少冰",
        "toppings": [
          { "topping_name": "珍珠", "price": 10 },
          { "topping_name": "椰果", "price": 10 }
        ],
        "item_price": 40,
        "sugar_price": 0,
        "topping_price": 20,
        "total_price": 60,
        "quantity": 1,
        "note": null,
        "created_at": "2025-03-15T11:00:00Z"
      },
      {
        "id": 2,
        "user_id": 2,
        "user_name": "Alice",
        "recipient_name": "Bob",
        "menu_item_name": "綠茶",
        "size_name": "中杯",
        "sugar_name": "無糖",
        "ice_name": "去冰",
        "toppings": [],
        "item_price": 30,
        "sugar_price": 0,
        "topping_price": 0,
        "total_price": 30,
        "quantity": 1,
        "note": null,
        "created_at": "2025-03-15T11:05:00Z"
      }
    ],
    "summary": {
      "total_items": 2,
      "total_amount": 90,
      "recipient_count": 2
    }
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

### 修改揪團狀態
```
PUT /api/admin/orders/{orderId}/status
```
- Request Body:
```json
{
  "status": 2
}
```
- Active ↔ Closed ↔ Delivered 可來回切換
- Completed / Cancelled 不可回退，違反回傳 400（`INVALID_STATUS_TRANSITION`）
- Cancelled 只允許從 Active 或 Closed 轉換，其餘回傳 400
- `Updater` = AdminUserId

### 取消揪團
```
PUT /api/admin/orders/{orderId}/cancel
```
- 僅 Active 或 Closed 狀態可取消，否則回傳 400（`CANNOT_CANCEL_ORDER`）
- 訂單明細保留不刪除
- `Status` 設為 `Cancelled`
- `Updater` = AdminUserId

### 匯出 Excel
```
GET /api/admin/orders/{orderId}/export
```
- 回傳 Excel 檔案（Content-Type: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet）
- 內容包含：揪團資訊、所有飲料明細
- 排序：先依 recipient_name asc，再依 created_at asc
- 檔名格式：`{title}_{date}.xlsx`

### 發送通知
```
POST /api/admin/orders/{orderId}/notify
```
- 手動發送通知給所有參與者（依各自 NotificationType）
- NotificationType = None 的用戶不發送
- 通知紀錄寫入通知資料表

---

## Frontend（Admin）

### 頁面：訂單列表
- 路徑：`/order/list`
- el-table 顯示揪團列表（標題、店家、發起人、狀態、截止時間、品項數、總金額、建立時間）
- 操作欄：查看詳情、修改狀態、取消、匯出、發送通知
- 篩選：狀態、店家（el-select）、建立日期區間（el-date-picker range）、截止日期區間
- keyword 搜尋
- 狀態以 el-tag 顏色區分

### 頁面：訂單詳情
- 路徑：`/order/list/:orderId`
- 區塊一：揪團基本資訊（標題、店家、發起人、狀態、截止時間、備註）
- 區塊二：統計摘要（總杯數、總金額、收件人數）
- 區塊三：飲料明細，支援版面切換：
  - **列表模式（List）**：el-table 顯示所有飲料（填單人、收件人、品項、尺寸、甜度、冰塊、加料、金額、備註）
  - **分組模式（Group by Recipient）**：以 recipient_name 分組顯示，每組顯示收件人名稱、該組小計
  - 分組邏輯由前端處理，以 recipient_name 純文字比對，同名歸為同一組
- 操作：修改狀態、取消、匯出 Excel、發送通知

---

## Success Criteria

### 揪團列表
- [ ] `GET /api/admin/orders` 支援分頁、排序、keyword 搜 title / initiator name
- [ ] 篩選支援 status, shop_id, created_from/to, deadline_from/to

### 揪團詳情
- [ ] `GET /api/admin/orders/{orderId}` 回傳揪團資訊 + 所有飲料明細（扁平列表）+ 統計摘要
- [ ] 前端支援列表 / 收件人分組兩種版面切換，分組邏輯由前端處理

### 狀態管理
- [ ] `PUT /api/admin/orders/{orderId}/status` Active ↔ Closed ↔ Delivered 可來回切換，Completed / Cancelled 不可回退
- [ ] `PUT /api/admin/orders/{orderId}/cancel` 僅 Active / Closed 可取消，否則回傳 400
- [ ] 取消揪團後飲料明細保留不刪除
- [ ] 狀態修改時 Updater = AdminUserId

### 匯出
- [ ] `GET /api/admin/orders/{orderId}/export` 匯出 Excel，包含揪團資訊與完整明細

### 通知
- [ ] `POST /api/admin/orders/{orderId}/notify` 手動發送通知給所有參與者
- [ ] 依各參與者 NotificationType 發送，None 不發送
- [ ] 通知紀錄寫入通知資料表

### Entity
- [ ] 訂單送出時價格快照（ItemPrice, SugarPrice, ToppingPrice），後續價格變動不影響
- [ ] `TotalPrice = ItemPrice + SugarPrice + ToppingPrice`
- [ ] 揪團 Active 且未過截止時可編輯/刪除飲料，截止後不可修改
- [ ] GroupOrder、OrderItem、OrderItemTopping 皆實作 ICreateEntity / IUpdateEntity

---

## Boundaries

✅ Always:
- 訂單價格為下單時快照，不受後續價格變動影響
- 揪團截止後不可修改飲料內容
- Active ↔ Closed ↔ Delivered 可來回切換
- Completed / Cancelled 不可回退
- 取消揪團後飲料明細保留
- 通知由後台管理員手動發送
- Updater 記錄操作者 ID（後台 = AdminUserId）

⚠️ Ask First:
- 修改 GroupOrder / OrderItem Entity 結構
- 修改狀態流轉規則
- 新增 GroupOrderStatus enum 值

🚫 Never:
- 允許 Completed / Cancelled 狀態回退
- 允許揪團截止後修改飲料
- 刪除飲料明細（後台僅查看）
- 允許 Delivered / Completed 狀態的揪團被取消

# Spec: 前台揪團與訂單 (User Order)

## Objective
- 前台用戶可建立揪團，選擇店家，設定截止時間，分享給其他人加入
- 參與者登入後可加入揪團，一次送出多杯飲料，每杯填寫收件人
- 揪團 Active 且未過截止時間時，參與者可編輯/刪除自己的飲料，發起人可編輯所有參與者的飲料
- 揪團截止後不可修改飲料內容
- 發起人可手動推進揪團狀態、取消揪團、匯出 Excel
- 訂單狀態變更為「已送達」時，系統自動通知所有參與者

> Entity 定義詳見 [admin-order.md](./admin-order.md)，本 spec 不重複定義

---

## Business Rules

### 建立揪團
- 需登入
- 選擇店家（僅上架且未刪除的店家）
- 填寫標題、截止時間、備註（選填）
- 截止時間必須在未來
- 建立後 Status = Active
- `InitiatorId` = 當前 UserId

### 加入揪團（點餐）
- 需登入
- 透過揪團連結 `/group/{id}` 進入
- 揪團必須為 Active 狀態且未過截止時間，否則不可點餐
- 一次可送出多杯飲料
- 每杯飲料需選擇：品項、尺寸、甜度、冰塊、加料（可多選）、數量、收件人、備註（選填）
- 收件人（RecipientName）未填則自動帶入當前用戶 Name
- 價格於送出時快照（ItemPrice、SugarPrice、ToppingPrice）
- `TotalPrice = ItemPrice + SugarPrice + ToppingPrice`
- 同一用戶可多次送出

### 編輯飲料
- 揪團 Active 且未過截止時間才可編輯
- 參與者只能編輯自己送出的飲料（UserId = 自己）
- 發起人可編輯揪團內所有人的飲料
- 編輯時價格重新快照

### 刪除飲料
- 揪團 Active 且未過截止時間才可刪除
- 參與者只能刪除自己送出的飲料
- 發起人可刪除揪團內所有人的飲料
- 硬刪除（OrderItem + OrderItemTopping）

### 揪團狀態管理（僅發起人）
- 只有發起人可操作
- 狀態流轉：
  ```
  Active ↔ Closed ↔ Delivered → Completed
  Active / Closed → Cancelled
  ```
- Active、Closed、Delivered 之間可來回切換
- Completed 不可回退（已結束）
- Cancelled 不可回退（已取消）
- 發起人手動推進狀態，系統不自動流轉
- 取消僅限 Active / Closed 狀態
- 回退至 Active 時，若未過截止時間，參與者可繼續點餐/編輯/刪除

### 編輯揪團資訊（僅發起人）
- 可修改標題、截止時間、備註
- 截止時間修改後必須仍在未來
- 僅 Active 狀態可編輯揪團資訊

### 截止時間
- 過了截止時間，不可再加入或修改飲料
- 系統不自動改狀態，由發起人手動將 Active → Closed
- 前端可依截止時間顯示倒數或已過期提示

### 通知
- 發起人可手動發送通知給所有參與者（不綁定特定狀態）
- 通知方式依各參與者的 NotificationType 設定發送
- NotificationType = None 的用戶不發送通知
- 通知內容：「{揪團標題} 的飲料已送達」
- 通知紀錄寫入通知資料表（後台通知管理 spec 另行定義）
- 後台管理員也可從後台手動對該揪團發送通知（詳見 admin-order.md）

### 匯出 Excel（僅發起人）
- 任何狀態皆可匯出
- 排序：recipient_name asc → created_at asc
- 內容包含：揪團資訊、所有飲料明細

---

## API Endpoints

### 揪團

#### 建立揪團
```
POST /api/user/group-orders
```
- Request Body:
```json
{
  "title": "下午茶揪起來",
  "shop_id": 1,
  "deadline": "2025-03-15T15:00:00Z",
  "note": "三點前要點完喔"
}
```
- shop_id 必須為上架且未刪除的店家，否則回傳 400（`SHOP_NOT_AVAILABLE`）
- deadline 必須在未來，否則回傳 400（`INVALID_DEADLINE`）

#### 取得揪團詳情（含菜單 + 飲料明細）
```
GET /api/user/group-orders/{id}
```
- 任何登入用戶皆可查看
- 回傳揪團資訊、店家菜單（含實際生效價格）、已加入的飲料明細
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
    "is_initiator": true,
    "can_order": true,
    "created_at": "2025-03-15T10:00:00Z",
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
        "can_edit": false,
        "can_delete": false,
        "created_at": "2025-03-15T11:00:00Z"
      }
    ],
    "summary": {
      "total_items": 8,
      "total_amount": 520,
      "recipient_count": 5
    }
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```
- `is_initiator`：當前用戶是否為發起人
- `can_order`：是否可點餐（Active 且未過截止）
- `can_edit` / `can_delete`：該筆飲料當前用戶是否可操作（依身份 + 狀態判斷）

#### 編輯揪團資訊（僅發起人）
```
PUT /api/user/group-orders/{id}
```
- Request Body:
```json
{
  "title": "下午茶揪起來（更新）",
  "deadline": "2025-03-15T16:00:00Z",
  "note": "四點前要點完"
}
```
- 非發起人回傳 403（`NOT_INITIATOR`）
- 非 Active 狀態回傳 400（`ORDER_NOT_ACTIVE`）
- deadline 必須在未來

#### 修改揪團狀態（僅發起人）
```
PUT /api/user/group-orders/{id}/status
```
- Request Body:
```json
{
  "status": 2
}
```
- 非發起人回傳 403（`NOT_INITIATOR`）
- Active ↔ Closed ↔ Delivered 可來回切換
- Completed / Cancelled 不可回退，回傳 400（`INVALID_STATUS_TRANSITION`）
- Active / Closed → Cancelled 允許
- Delivered / Completed → Cancelled 不允許

#### 取消揪團（僅發起人）
```
PUT /api/user/group-orders/{id}/cancel
```
- 非發起人回傳 403（`NOT_INITIATOR`）
- 僅 Active / Closed 可取消，否則回傳 400（`CANNOT_CANCEL_ORDER`）
- 飲料明細保留不刪除

#### 匯出 Excel（僅發起人）
```
GET /api/user/group-orders/{id}/export
```
- 非發起人回傳 403（`NOT_INITIATOR`）
- 排序：recipient_name asc → created_at asc
- 檔名格式：`{title}_{date}.xlsx`

#### 發送通知（僅發起人）
```
POST /api/user/group-orders/{id}/notify
```
- 非發起人回傳 403（`NOT_INITIATOR`）
- 手動發送通知給所有參與者（依各自 NotificationType）
- NotificationType = None 的用戶不發送
- 通知紀錄寫入通知資料表

---

### 我的揪團列表

#### 我發起的揪團
```
GET /api/user/group-orders/initiated?page=1&page_size=20&sort_by=created_at&sort_order=desc&status=1
```
- 篩選：status
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "title": "下午茶揪起來",
        "shop_name": "50嵐",
        "status": 1,
        "deadline": "2025-03-15T15:00:00Z",
        "order_item_count": 8,
        "total_amount": 520,
        "created_at": "2025-03-15T10:00:00Z"
      }
    ],
    "total": 10,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

#### 我參與的揪團
```
GET /api/user/group-orders/joined?page=1&page_size=20&sort_by=created_at&sort_order=desc&status=1
```
- 列出當前用戶有送出飲料的揪團（不含自己發起的）
- 篩選：status
- Response 同上格式

---

### 飲料（點餐）

#### 送出飲料（加入揪團）
```
POST /api/user/group-orders/{id}/items
```
- Request Body（支援多杯）:
```json
{
  "items": [
    {
      "menu_item_id": 1,
      "size_id": 3,
      "sugar_id": 3,
      "ice_id": 2,
      "topping_ids": [1, 2],
      "quantity": 1,
      "recipient_name": "Alice",
      "note": null
    },
    {
      "menu_item_id": 2,
      "size_id": 2,
      "sugar_id": 5,
      "ice_id": 4,
      "topping_ids": [],
      "quantity": 1,
      "recipient_name": "",
      "note": "不要吸管"
    }
  ]
}
```
- 揪團必須 Active 且未過截止，否則回傳 400（`ORDER_NOT_ACTIVE`）
- `recipient_name` 為空字串或未提供時，自動帶入當前用戶 Name
- 價格於送出時快照
- 各 ID 必須存在且屬於該店家菜單，否則回傳 400

#### 編輯飲料
```
PUT /api/user/group-orders/{id}/items/{itemId}
```
- Request Body:
```json
{
  "menu_item_id": 1,
  "size_id": 3,
  "sugar_id": 3,
  "ice_id": 2,
  "topping_ids": [1, 2],
  "quantity": 1,
  "recipient_name": "Alice",
  "note": "改半糖"
}
```
- 揪團必須 Active 且未過截止，否則回傳 400（`ORDER_NOT_ACTIVE`）
- 非本人送出且非發起人，回傳 403（`FORBIDDEN`）
- 編輯時價格重新快照

#### 刪除飲料
```
DELETE /api/user/group-orders/{id}/items/{itemId}
```
- 揪團必須 Active 且未過截止，否則回傳 400（`ORDER_NOT_ACTIVE`）
- 非本人送出且非發起人，回傳 403（`FORBIDDEN`）
- 硬刪除 OrderItem + OrderItemTopping

---

## Frontend（Client）

### 頁面：建立揪團
- 路徑：`/group/create`
- 選擇店家（搜尋 + 選擇）
- 填寫標題、截止時間（date-time picker）、備註
- 建立成功後跳轉至揪團頁面

### 頁面：揪團詳情 / 點餐
- 路徑：`/group/{id}`
- 區塊一：揪團資訊（標題、店家、發起人、狀態、截止時間倒數、備註）
- 區塊二：點餐區（僅 can_order = true 時顯示）
  - 瀏覽店家菜單，選擇品項
  - 填寫尺寸、甜度、冰塊、加料、數量、收件人、備註
  - 可一次加多杯，送出前可預覽
- 區塊三：飲料總覽，支援版面切換：
  - **列表模式**：顯示所有飲料
  - **收件人分組模式**：以 recipient_name 分組，每組顯示小計
  - 自己送出的飲料可編輯/刪除（發起人可操作所有）
- 區塊四：統計摘要（總杯數、總金額、收件人數）
- 發起人專屬操作：編輯揪團、推進狀態、取消、匯出 Excel、發送通知

### 頁面：我的揪團
- 路徑：`/group/my`
- Tab 切換：我發起的 / 我參與的
- 列表顯示揪團（標題、店家、狀態、截止時間、杯數、總金額）
- 點擊進入揪團詳情

---

## Success Criteria

### 建立揪團
- [ ] `POST /api/user/group-orders` 建立揪團，Status = Active
- [ ] 店家必須上架且未刪除，否則回傳 400
- [ ] 截止時間必須在未來，否則回傳 400

### 揪團詳情
- [ ] `GET /api/user/group-orders/{id}` 回傳揪團資訊 + 飲料明細 + 統計摘要
- [ ] 回傳 `is_initiator`、`can_order`、每筆飲料的 `can_edit` / `can_delete`

### 揪團管理（發起人）
- [ ] `PUT /api/user/group-orders/{id}` 僅發起人可編輯揪團資訊，僅 Active 狀態可操作
- [ ] `PUT /api/user/group-orders/{id}/status` Active ↔ Closed ↔ Delivered 可來回切換，Completed / Cancelled 不可回退
- [ ] `PUT /api/user/group-orders/{id}/cancel` 僅 Active / Closed 可取消
- [ ] 非發起人操作回傳 403

### 點餐
- [ ] `POST /api/user/group-orders/{id}/items` 支援一次送出多杯
- [ ] 揪團非 Active 或已過截止時間，回傳 400
- [ ] recipient_name 未填自動帶入當前用戶 Name
- [ ] 價格於送出時快照，不受後續價格變動影響

### 編輯 / 刪除飲料
- [ ] `PUT .../items/{itemId}` 可編輯飲料，價格重新快照
- [ ] `DELETE .../items/{itemId}` 硬刪除飲料
- [ ] 參與者只能操作自己的飲料，發起人可操作所有
- [ ] 揪團截止後不可編輯/刪除飲料

### 匯出
- [ ] `GET /api/user/group-orders/{id}/export` 僅發起人可匯出
- [ ] 排序：recipient_name asc → created_at asc

### 我的揪團
- [ ] `GET /api/user/group-orders/initiated` 回傳我發起的揪團，支援分頁 + 篩選 status
- [ ] `GET /api/user/group-orders/joined` 回傳我參與的揪團（不含自己發起的）

### 通知
- [ ] `POST /api/user/group-orders/{id}/notify` 僅發起人可手動發送通知
- [ ] 通知依各參與者的 NotificationType 發送，None 不發送
- [ ] 通知紀錄寫入通知資料表

---

## Boundaries

✅ Always:
- 需登入才能建立/加入揪團
- 揪團 Active 且未過截止才可點餐/編輯/刪除飲料
- 參與者只能操作自己的飲料，發起人可操作所有
- 價格於送出/編輯時快照
- Active ↔ Closed ↔ Delivered 可來回切換
- Completed / Cancelled 不可回退
- recipient_name 未填自動帶入用戶 Name
- 通知由發起人或後台管理員手動發送

⚠️ Ask First:
- 修改狀態流轉規則
- 新增 GroupOrderStatus enum 值

🚫 Never:
- 允許未登入用戶加入揪團
- 允許揪團截止後修改飲料
- 允許非發起人推進狀態/取消/匯出/發送通知
- 允許 Completed / Cancelled 狀態回退
- 刪除已取消揪團的飲料明細（保留）

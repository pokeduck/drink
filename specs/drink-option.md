# Spec: 飲料選項 (Drink Option)

## Objective
- 後台管理全域飲料選項：通用品名、甜度、冰塊、加料、容量
- 這些選項為平台統一定義，店家建立菜單時引用
- 甜度、加料帶有預設價格，店家可於菜單編輯時覆寫（未覆寫則採用預設價格）
- 冰塊無價格概念
- 容量無預設價格，品項以絕對價格制設定各尺寸售價
- 通用品名僅供顯示用途，無價格
- 五種選項彼此完全獨立，無關聯

---

## Entities

### DrinkItem（通用品名）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| Name | string(100) | ✅ | 品名名稱（如「珍珠奶茶」、「綠茶」） |
| Sort | int | ✅ | 排序（由小到大） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### Sugar（甜度）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| Name | string(50) | ✅ | 甜度名稱（如「正常糖」、「加蜂蜜」） |
| DefaultPrice | decimal | ✅ | 預設價格，通常為 0，特殊甜度（如蜂蜜）有加價 |
| Sort | int | ✅ | 排序（由小到大） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### Ice（冰塊）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| Name | string(50) | ✅ | 冰塊名稱（如「正常冰」、「少冰」、「去冰」） |
| Sort | int | ✅ | 排序（由小到大） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### Topping（加料）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| Name | string(100) | ✅ | 加料名稱（如「珍珠」、「椰果」、「仙草」） |
| DefaultPrice | decimal | ✅ | 預設價格（如 10 元） |
| Sort | int | ✅ | 排序（由小到大） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### Size（容量）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| Name | string(50) | ✅ | 容量名稱（如「中杯」、「大杯」、「M」、「L」） |
| Sort | int | ✅ | 排序（由小到大） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

---

## Code Style

```csharp
public class DrinkItem : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(100)]
    public string Name { get; set; }

    public int Sort { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public class Sugar : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(50)]
    public string Name { get; set; }

    public decimal DefaultPrice { get; set; }

    public int Sort { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public class Ice : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(50)]
    public string Name { get; set; }

    public int Sort { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public class Topping : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(100)]
    public string Name { get; set; }

    public decimal DefaultPrice { get; set; }

    public int Sort { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public class Size : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    [StringLength(50)]
    public string Name { get; set; }

    public int Sort { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}
```

---

## API Endpoints

### 通用品名（DrinkItem）

#### 列表
```
GET /api/admin/drink-items?page=1&page_size=20&sort_by=sort&sort_order=asc&keyword=奶茶
```
- 遵循 SPEC.md 列表通用規範（分頁、排序、搜尋、篩選）
- keyword 搜尋欄位：name
- 預設排序：sort asc
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "name": "珍珠奶茶",
        "sort": 1,
        "created_at": "2025-01-01T00:00:00Z"
      }
    ],
    "total": 50,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 取得單一品名
```
GET /api/admin/drink-items/{id}
```
- Response:
```json
{
  "data": {
    "id": 1,
    "name": "珍珠奶茶",
    "sort": 1,
    "created_at": "2025-01-01T00:00:00Z",
    "updated_at": "2025-01-01T00:00:00Z"
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 新增品名
```
POST /api/admin/drink-items
```
- Request Body:
```json
{
  "name": "珍珠奶茶",
  "sort": 1
}
```
- name 唯一，重複回傳 409（`DRINK_ITEM_ALREADY_EXISTS`）

#### 更新品名
```
PUT /api/admin/drink-items/{id}
```
- Request Body:
```json
{
  "name": "珍珠奶茶（更新）",
  "sort": 2
}
```
- name 唯一（排除自身），重複回傳 409（`DRINK_ITEM_ALREADY_EXISTS`）

#### 刪除品名
```
DELETE /api/admin/drink-items/{id}
```
- 若有店家菜單引用該品名，回傳 400（`DRINK_ITEM_IN_USE`）

#### 批次排序
```
PUT /api/admin/drink-items/sort
```
- Request Body:
```json
{
  "items": [
    { "id": 1, "sort": 3 },
    { "id": 2, "sort": 1 },
    { "id": 3, "sort": 2 }
  ]
}
```
- 一次更新多筆排序
- items 中的 id 必須存在，否則回傳 400（`DRINK_ITEM_NOT_FOUND`）

#### 批次刪除
```
DELETE /api/admin/drink-items/batch
```
- Request Body:
```json
{
  "ids": [1, 2, 3]
}
```
- 若任一筆被店家菜單引用，整批失敗，回傳 400（`DRINK_ITEM_IN_USE`），並列出被引用的 id
- Response（失敗時）:
```json
{
  "data": null,
  "message": "部分品名已被店家引用，無法刪除",
  "code": 40702,
  "error": "DRINK_ITEM_IN_USE",
  "errors": {
    "ids": ["id 1, 3 已被店家引用"]
  }
}
```

---

### 甜度（Sugar）

#### 列表
```
GET /api/admin/sugars?page=1&page_size=20&sort_by=sort&sort_order=asc&keyword=蜂蜜
```
- keyword 搜尋欄位：name
- 預設排序：sort asc
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "name": "正常糖",
        "default_price": 0,
        "sort": 1,
        "created_at": "2025-01-01T00:00:00Z"
      }
    ],
    "total": 10,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 取得單一甜度
```
GET /api/admin/sugars/{id}
```
- Response:
```json
{
  "data": {
    "id": 1,
    "name": "正常糖",
    "default_price": 0,
    "sort": 1,
    "created_at": "2025-01-01T00:00:00Z",
    "updated_at": "2025-01-01T00:00:00Z"
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 新增甜度
```
POST /api/admin/sugars
```
- Request Body:
```json
{
  "name": "加蜂蜜",
  "default_price": 5,
  "sort": 6
}
```
- name 唯一，重複回傳 409（`SUGAR_ALREADY_EXISTS`）

#### 更新甜度
```
PUT /api/admin/sugars/{id}
```
- Request Body:
```json
{
  "name": "加蜂蜜",
  "default_price": 10,
  "sort": 6
}
```
- name 唯一（排除自身），重複回傳 409（`SUGAR_ALREADY_EXISTS`）

#### 刪除甜度
```
DELETE /api/admin/sugars/{id}
```
- 若有店家菜單引用該甜度，回傳 400（`SUGAR_IN_USE`）

#### 批次排序
```
PUT /api/admin/sugars/sort
```
- Request Body:
```json
{
  "items": [
    { "id": 1, "sort": 3 },
    { "id": 2, "sort": 1 }
  ]
}
```
- id 必須存在，否則回傳 400（`SUGAR_NOT_FOUND`）

#### 批次刪除
```
DELETE /api/admin/sugars/batch
```
- Request Body:
```json
{
  "ids": [1, 2, 3]
}
```
- 若任一筆被店家菜單引用，整批失敗，回傳 400（`SUGAR_IN_USE`），並列出被引用的 id

---

### 冰塊（Ice）

#### 列表
```
GET /api/admin/ices?page=1&page_size=20&sort_by=sort&sort_order=asc&keyword=少冰
```
- keyword 搜尋欄位：name
- 預設排序：sort asc
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "name": "正常冰",
        "sort": 1,
        "created_at": "2025-01-01T00:00:00Z"
      }
    ],
    "total": 5,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 取得單一冰塊
```
GET /api/admin/ices/{id}
```
- Response:
```json
{
  "data": {
    "id": 1,
    "name": "正常冰",
    "sort": 1,
    "created_at": "2025-01-01T00:00:00Z",
    "updated_at": "2025-01-01T00:00:00Z"
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 新增冰塊
```
POST /api/admin/ices
```
- Request Body:
```json
{
  "name": "溫的",
  "sort": 6
}
```
- name 唯一，重複回傳 409（`ICE_ALREADY_EXISTS`）

#### 更新冰塊
```
PUT /api/admin/ices/{id}
```
- Request Body:
```json
{
  "name": "溫的",
  "sort": 6
}
```
- name 唯一（排除自身），重複回傳 409（`ICE_ALREADY_EXISTS`）

#### 刪除冰塊
```
DELETE /api/admin/ices/{id}
```
- 若有店家菜單引用該冰塊，回傳 400（`ICE_IN_USE`）

#### 批次排序
```
PUT /api/admin/ices/sort
```
- Request Body:
```json
{
  "items": [
    { "id": 1, "sort": 3 },
    { "id": 2, "sort": 1 }
  ]
}
```
- id 必須存在，否則回傳 400（`ICE_NOT_FOUND`）

#### 批次刪除
```
DELETE /api/admin/ices/batch
```
- Request Body:
```json
{
  "ids": [1, 2, 3]
}
```
- 若任一筆被店家菜單引用，整批失敗，回傳 400（`ICE_IN_USE`），並列出被引用的 id

---

### 加料（Topping）

#### 列表
```
GET /api/admin/toppings?page=1&page_size=20&sort_by=sort&sort_order=asc&keyword=珍珠
```
- keyword 搜尋欄位：name
- 預設排序：sort asc
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "name": "珍珠",
        "default_price": 10,
        "sort": 1,
        "created_at": "2025-01-01T00:00:00Z"
      }
    ],
    "total": 15,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 取得單一加料
```
GET /api/admin/toppings/{id}
```
- Response:
```json
{
  "data": {
    "id": 1,
    "name": "珍珠",
    "default_price": 10,
    "sort": 1,
    "created_at": "2025-01-01T00:00:00Z",
    "updated_at": "2025-01-01T00:00:00Z"
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 新增加料
```
POST /api/admin/toppings
```
- Request Body:
```json
{
  "name": "珍珠",
  "default_price": 10,
  "sort": 1
}
```
- name 唯一，重複回傳 409（`TOPPING_ALREADY_EXISTS`）

#### 更新加料
```
PUT /api/admin/toppings/{id}
```
- Request Body:
```json
{
  "name": "珍珠",
  "default_price": 15,
  "sort": 1
}
```
- name 唯一（排除自身），重複回傳 409（`TOPPING_ALREADY_EXISTS`）

#### 刪除加料
```
DELETE /api/admin/toppings/{id}
```
- 若有店家菜單引用該加料，回傳 400（`TOPPING_IN_USE`）

#### 批次排序
```
PUT /api/admin/toppings/sort
```
- Request Body:
```json
{
  "items": [
    { "id": 1, "sort": 3 },
    { "id": 2, "sort": 1 }
  ]
}
```
- id 必須存在，否則回傳 400（`TOPPING_NOT_FOUND`）

#### 批次刪除
```
DELETE /api/admin/toppings/batch
```
- Request Body:
```json
{
  "ids": [1, 2, 3]
}
```
- 若任一筆被店家菜單引用，整批失敗，回傳 400（`TOPPING_IN_USE`），並列出被引用的 id

---

### 容量（Size）

#### 列表
```
GET /api/admin/sizes?page=1&page_size=20&sort_by=sort&sort_order=asc&keyword=大杯
```
- keyword 搜尋欄位：name
- 預設排序：sort asc
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "name": "中杯",
        "sort": 1,
        "created_at": "2025-01-01T00:00:00Z"
      }
    ],
    "total": 10,
    "page": 1,
    "page_size": 20
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 取得單一容量
```
GET /api/admin/sizes/{id}
```
- Response:
```json
{
  "data": {
    "id": 1,
    "name": "中杯",
    "sort": 1,
    "created_at": "2025-01-01T00:00:00Z",
    "updated_at": "2025-01-01T00:00:00Z"
  },
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

#### 新增容量
```
POST /api/admin/sizes
```
- Request Body:
```json
{
  "name": "中杯",
  "sort": 1
}
```
- name 唯一，重複回傳 409（`SIZE_ALREADY_EXISTS`）

#### 更新容量
```
PUT /api/admin/sizes/{id}
```
- Request Body:
```json
{
  "name": "中杯",
  "sort": 1
}
```
- name 唯一（排除自身），重複回傳 409（`SIZE_ALREADY_EXISTS`）

#### 刪除容量
```
DELETE /api/admin/sizes/{id}
```
- 若有店家菜單引用該容量，回傳 400（`SIZE_IN_USE`）

#### 批次排序
```
PUT /api/admin/sizes/sort
```
- Request Body:
```json
{
  "items": [
    { "id": 1, "sort": 3 },
    { "id": 2, "sort": 1 }
  ]
}
```
- id 必須存在，否則回傳 400（`SIZE_NOT_FOUND`）

#### 批次刪除
```
DELETE /api/admin/sizes/batch
```
- Request Body:
```json
{
  "ids": [1, 2, 3]
}
```
- 若任一筆被店家菜單引用，整批失敗，回傳 400（`SIZE_IN_USE`），並列出被引用的 id

---

## 前台 API（公開查詢）

前台用戶建立菜單時需要查詢全域選項，提供不分頁的完整列表。

### 取得所有通用品名
```
GET /api/user/drink-items
```
- Response:
```json
{
  "data": [
    { "id": 1, "name": "珍珠奶茶", "sort": 1 },
    { "id": 2, "name": "綠茶", "sort": 2 }
  ],
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 取得所有甜度
```
GET /api/user/sugars
```
- Response:
```json
{
  "data": [
    { "id": 1, "name": "正常糖", "default_price": 0, "sort": 1 },
    { "id": 2, "name": "加蜂蜜", "default_price": 5, "sort": 6 }
  ],
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 取得所有冰塊
```
GET /api/user/ices
```
- Response:
```json
{
  "data": [
    { "id": 1, "name": "正常冰", "sort": 1 },
    { "id": 2, "name": "少冰", "sort": 2 }
  ],
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 取得所有加料
```
GET /api/user/toppings
```
- Response:
```json
{
  "data": [
    { "id": 1, "name": "珍珠", "default_price": 10, "sort": 1 },
    { "id": 2, "name": "椰果", "default_price": 10, "sort": 2 }
  ],
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

### 取得所有容量
```
GET /api/user/sizes
```
- Response:
```json
{
  "data": [
    { "id": 1, "name": "中杯", "sort": 1 },
    { "id": 2, "name": "大杯", "sort": 2 }
  ],
  "message": null,
  "code": 0,
  "error": null,
  "errors": null
}
```

---

## Seed Data

```csharp
// 通用品名
new DrinkItem { Id = 1,  Name = "珍珠奶茶", Sort = 1 },
new DrinkItem { Id = 2,  Name = "紅茶",     Sort = 2 },
new DrinkItem { Id = 3,  Name = "綠茶",     Sort = 3 },
new DrinkItem { Id = 4,  Name = "烏龍茶",   Sort = 4 },
new DrinkItem { Id = 5,  Name = "青茶",     Sort = 5 },
new DrinkItem { Id = 6,  Name = "鮮奶茶",   Sort = 6 },
new DrinkItem { Id = 7,  Name = "冬瓜茶",   Sort = 7 },
new DrinkItem { Id = 8,  Name = "檸檬茶",   Sort = 8 },
new DrinkItem { Id = 9,  Name = "多多綠",   Sort = 9 },
new DrinkItem { Id = 10, Name = "果汁",     Sort = 10 },

// 甜度
new Sugar { Id = 1, Name = "正常糖", DefaultPrice = 0, Sort = 1 },
new Sugar { Id = 2, Name = "少糖",   DefaultPrice = 0, Sort = 2 },
new Sugar { Id = 3, Name = "半糖",   DefaultPrice = 0, Sort = 3 },
new Sugar { Id = 4, Name = "微糖",   DefaultPrice = 0, Sort = 4 },
new Sugar { Id = 5, Name = "無糖",   DefaultPrice = 0, Sort = 5 },
new Sugar { Id = 6, Name = "加蜂蜜", DefaultPrice = 5, Sort = 6 },

// 冰塊
new Ice { Id = 1, Name = "正常冰", Sort = 1 },
new Ice { Id = 2, Name = "少冰",   Sort = 2 },
new Ice { Id = 3, Name = "微冰",   Sort = 3 },
new Ice { Id = 4, Name = "去冰",   Sort = 4 },
new Ice { Id = 5, Name = "熱",     Sort = 5 },
new Ice { Id = 6, Name = "溫",     Sort = 6 },

// 加料
new Topping { Id = 1,  Name = "珍珠",   DefaultPrice = 10, Sort = 1 },
new Topping { Id = 2,  Name = "椰果",   DefaultPrice = 10, Sort = 2 },
new Topping { Id = 3,  Name = "仙草",   DefaultPrice = 10, Sort = 3 },
new Topping { Id = 4,  Name = "布丁",   DefaultPrice = 10, Sort = 4 },
new Topping { Id = 5,  Name = "蘆薈",   DefaultPrice = 10, Sort = 5 },
new Topping { Id = 6,  Name = "愛玉",   DefaultPrice = 10, Sort = 6 },
new Topping { Id = 7,  Name = "粉條",   DefaultPrice = 10, Sort = 7 },
new Topping { Id = 8,  Name = "芋圓",   DefaultPrice = 15, Sort = 8 },
new Topping { Id = 9,  Name = "白玉",   DefaultPrice = 10, Sort = 9 },
new Topping { Id = 10, Name = "奶蓋",   DefaultPrice = 15, Sort = 10 },

// 容量
new Size { Id = 1, Name = "小杯", Sort = 1 },
new Size { Id = 2, Name = "中杯", Sort = 2 },
new Size { Id = 3, Name = "大杯", Sort = 3 },
new Size { Id = 4, Name = "S",    Sort = 4 },
new Size { Id = 5, Name = "M",    Sort = 5 },
new Size { Id = 6, Name = "L",    Sort = 6 },
new Size { Id = 7, Name = "XL",   Sort = 7 },
```

---

## Business Rules

### 名稱唯一性
- 五種選項各自的 Name 在同類型內必須唯一（不分大小寫）
- 不同類型間名稱可重複（如品名和加料都可以有「珍珠」）

### 排序
- Sort 為全域預設排序，前端依 Sort 升冪顯示
- 店家可於菜單編輯時覆寫排序（店家 spec 處理）

### 價格
- Sugar.DefaultPrice 和 Topping.DefaultPrice 為全域預設價格
- 店家可於菜單編輯時覆寫價格，未覆寫則採用預設價格（店家 spec 處理）
- DefaultPrice 必須 >= 0

### 刪除保護
- 若有店家菜單引用該選項，不可刪除，回傳 400 + 對應錯誤碼
- 確認無引用後才可刪除

### 角色權限
- 後台 API（`/api/admin/*`）透過 RoleMiddleware 控制存取
- 對應 MenuConstants：DrinkItem = 12, Sugar = 13, Ice = 14, Topping = 15, Size = 16
- 前台 API（`/api/user/*`）菜單相關為公開存取，不需登入

---

## Frontend（Admin）

### 頁面：通用品名
- 路徑：`/drink-option/item`
- el-table 顯示品名列表（名稱、排序）
- 操作欄：編輯、刪除
- 新增按鈕
- 新增 / 編輯使用 el-dialog（名稱 + 排序）

### 頁面：甜度定義
- 路徑：`/drink-option/sugar`
- el-table 顯示甜度列表（名稱、預設價格、排序）
- 操作欄：編輯、刪除
- 新增按鈕
- 新增 / 編輯使用 el-dialog（名稱 + 預設價格 + 排序）

### 頁面：冰塊定義
- 路徑：`/drink-option/ice`
- el-table 顯示冰塊列表（名稱、排序）
- 操作欄：編輯、刪除
- 新增按鈕
- 新增 / 編輯使用 el-dialog（名稱 + 排序）

### 頁面：加料
- 路徑：`/drink-option/topping`
- el-table 顯示加料列表（名稱、預設價格、排序）
- 操作欄：編輯、刪除
- 新增按鈕
- 新增 / 編輯使用 el-dialog（名稱 + 預設價格 + 排序）

### 頁面：容量定義
- 路徑：`/drink-option/size`
- el-table 顯示容量列表（名稱、排序）
- 操作欄：編輯、刪除
- 新增按鈕
- 新增 / 編輯使用 el-dialog（名稱 + 排序）

### 共通行為
- 刪除前 el-dialog 確認
- 刪除時若有店家引用，顯示後端回傳的錯誤訊息
- 列表支援 keyword 搜尋
- 表格可排序
- 支援多選（el-table selection），可批次刪除
- 支援拖拉排序，拖拉後呼叫批次排序 API

---

## Success Criteria

### 通用品名
- [ ] `GET /api/admin/drink-items` 支援分頁、排序、keyword 搜 name
- [ ] `POST /api/admin/drink-items` name 唯一，重複回傳 409
- [ ] `PUT /api/admin/drink-items/{id}` name 唯一（排除自身），重複回傳 409
- [ ] `DELETE /api/admin/drink-items/{id}` 有店家引用時回傳 400
- [ ] `PUT /api/admin/drink-items/sort` 批次更新排序
- [ ] `DELETE /api/admin/drink-items/batch` 批次刪除，任一筆被引用則整批失敗

### 甜度
- [ ] `GET /api/admin/sugars` 支援分頁、排序、keyword 搜 name
- [ ] `POST /api/admin/sugars` name 唯一，重複回傳 409
- [ ] `PUT /api/admin/sugars/{id}` name 唯一（排除自身），重複回傳 409
- [ ] `DELETE /api/admin/sugars/{id}` 有店家引用時回傳 400
- [ ] `PUT /api/admin/sugars/sort` 批次更新排序
- [ ] `DELETE /api/admin/sugars/batch` 批次刪除，任一筆被引用則整批失敗
- [ ] DefaultPrice >= 0

### 冰塊
- [ ] `GET /api/admin/ices` 支援分頁、排序、keyword 搜 name
- [ ] `POST /api/admin/ices` name 唯一，重複回傳 409
- [ ] `PUT /api/admin/ices/{id}` name 唯一（排除自身），重複回傳 409
- [ ] `DELETE /api/admin/ices/{id}` 有店家引用時回傳 400
- [ ] `PUT /api/admin/ices/sort` 批次更新排序
- [ ] `DELETE /api/admin/ices/batch` 批次刪除，任一筆被引用則整批失敗

### 加料
- [ ] `GET /api/admin/toppings` 支援分頁、排序、keyword 搜 name
- [ ] `POST /api/admin/toppings` name 唯一，重複回傳 409
- [ ] `PUT /api/admin/toppings/{id}` name 唯一（排除自身），重複回傳 409
- [ ] `DELETE /api/admin/toppings/{id}` 有店家引用時回傳 400
- [ ] `PUT /api/admin/toppings/sort` 批次更新排序
- [ ] `DELETE /api/admin/toppings/batch` 批次刪除，任一筆被引用則整批失敗
- [ ] DefaultPrice >= 0

### 容量
- [ ] `GET /api/admin/sizes` 支援分頁、排序、keyword 搜 name
- [ ] `POST /api/admin/sizes` name 唯一，重複回傳 409
- [ ] `PUT /api/admin/sizes/{id}` name 唯一（排除自身），重複回傳 409
- [ ] `DELETE /api/admin/sizes/{id}` 有店家引用時回傳 400
- [ ] `PUT /api/admin/sizes/sort` 批次更新排序
- [ ] `DELETE /api/admin/sizes/batch` 批次刪除，任一筆被引用則整批失敗

### Seed Data
- [ ] 系統初始化時建立預設的通用品名、甜度、冰塊、加料、容量

### 前台 API
- [ ] `GET /api/user/drink-items` 回傳所有品名，依 sort 升冪
- [ ] `GET /api/user/sugars` 回傳所有甜度（含 default_price），依 sort 升冪
- [ ] `GET /api/user/ices` 回傳所有冰塊，依 sort 升冪
- [ ] `GET /api/user/toppings` 回傳所有加料（含 default_price），依 sort 升冪
- [ ] `GET /api/user/sizes` 回傳所有容量，依 sort 升冪

### 角色權限
- [ ] 後台 API 透過 `[RequireRole]` + RoleMiddleware 控制，無權回傳 403
- [ ] 前台 API 公開存取，不需登入

---

## Boundaries

✅ Always:
- 五種選項各自 Name 唯一（不分大小寫）
- DefaultPrice >= 0
- 有店家引用時不可刪除
- 後台 API 透過 RoleMiddleware 控制存取
- 前台 API 回傳依 Sort 升冪排序

⚠️ Ask First:
- 修改 DefaultPrice 欄位精度（影響金額計算）
- 新增選項類型

🚫 Never:
- 允許刪除被店家菜單引用的選項
- 允許 DefaultPrice 為負數
- 在前台 API 暴露 created_at / updated_at 等管理欄位

# Spec: 店家與菜單 (Shop)

## Objective
- 後台管理員建立與管理店家，包含店家資訊、上下架狀態
- 每間店家有多個菜單分類（一層），分類底下有多個品項
- 品項名稱可從全域 DrinkItem 選擇，也可在菜單編輯頁面直接新增（自動寫入全域 DrinkItem）
- 每個品項獨立設定支援的尺寸（絕對價格制）、甜度、冰塊、加料（勾選制，預設全勾）
- 店家可統一覆寫甜度、加料的價格與排序，未覆寫則採用全域預設值
- 前台公開查詢店家與菜單，不需登入

---

## Entities

### Shop（店家）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| Name | string(100) | ✅ | 店家名稱 |
| Phone | string(20) | ❌ | 聯絡電話 |
| Address | string(200) | ❌ | 地址 |
| Note | string(500) | ❌ | 備註 |
| Status | ShopStatus (enum) | ✅ | 上架/下架狀態 |
| Sort | int | ✅ | 排序（由小到大） |
| IsDeleted | bool | ✅ | 軟刪除標記，預設 false（ISoftDeleteEntity） |
| DeletedAt | DateTime? | ❌ | 刪除時間（ISoftDeleteEntity） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### ShopStatus (Enum)
```csharp
public enum ShopStatus
{
    Active = 1,     // 上架
    Inactive = 2    // 下架
}
```

### ShopCategory（菜單分類）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| ShopId | int | ✅ | FK → Shop |
| Name | string(100) | ✅ | 分類名稱（如「純茶類」、「奶茶類」） |
| Sort | int | ✅ | 排序（同店家內由小到大） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### ShopMenuItem（品項）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| CategoryId | int | ✅ | FK → ShopCategory |
| DrinkItemId | int | ✅ | FK → DrinkItem（通用品名） |
| Description | string(200) | ❌ | 品項描述/備註 |
| Sort | int | ✅ | 排序（同分類內由小到大） |
| IsDeleted | bool | ✅ | 軟刪除標記，預設 false（ISoftDeleteEntity） |
| DeletedAt | DateTime? | ❌ | 刪除時間（ISoftDeleteEntity） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### ShopMenuItemSize（品項尺寸價格）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| MenuItemId | int | ✅ | FK → ShopMenuItem |
| SizeId | int | ✅ | FK → Size |
| Price | decimal | ✅ | 絕對價格（如 30、40） |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### ShopMenuItemSugar（品項支援的甜度）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| MenuItemId | int | ✅ | FK → ShopMenuItem |
| SugarId | int | ✅ | FK → Sugar |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### ShopMenuItemIce（品項支援的冰塊）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| MenuItemId | int | ✅ | FK → ShopMenuItem |
| IceId | int | ✅ | FK → Ice |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### ShopMenuItemTopping（品項支援的加料）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| MenuItemId | int | ✅ | FK → ShopMenuItem |
| ToppingId | int | ✅ | FK → Topping |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### ShopSugarOverride（店家甜度覆寫）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| ShopId | int | ✅ | FK → Shop |
| SugarId | int | ✅ | FK → Sugar |
| Price | decimal | ❌ | 覆寫價格，null = 使用全域 DefaultPrice |
| Sort | int | ❌ | 覆寫排序，null = 使用全域 Sort |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

### ShopToppingOverride（店家加料覆寫）
| 欄位 | 型別 | 必填 | 說明 |
|------|------|------|------|
| Id | int | ✅ | PK，自動遞增 |
| ShopId | int | ✅ | FK → Shop |
| ToppingId | int | ✅ | FK → Topping |
| Price | decimal | ❌ | 覆寫價格，null = 使用全域 DefaultPrice |
| Sort | int | ❌ | 覆寫排序，null = 使用全域 Sort |
| CreatedAt | DateTime | ✅ | 建立時間（ICreateEntity） |
| Creator | int | ✅ | 建立者 ID（ICreateEntity） |
| UpdatedAt | DateTime | ✅ | 更新時間（IUpdateEntity） |
| Updater | int | ✅ | 更新者 ID（IUpdateEntity） |

---

## Relationships
- `Shop` → `ShopCategory`：一對多
- `ShopCategory` → `ShopMenuItem`：一對多
- `ShopMenuItem` → `DrinkItem`：多對一（品名引用）
- `ShopMenuItem` → `ShopMenuItemSize`：一對多（各尺寸價格）
- `ShopMenuItem` → `ShopMenuItemSugar`：一對多（支援的甜度）
- `ShopMenuItem` → `ShopMenuItemIce`：一對多（支援的冰塊）
- `ShopMenuItem` → `ShopMenuItemTopping`：一對多（支援的加料）
- `Shop` → `ShopSugarOverride`：一對多（店家甜度覆寫）
- `Shop` → `ShopToppingOverride`：一對多（店家加料覆寫）

---

## Code Style

```csharp
public class Shop : BaseDataEntity, ICreateEntity, IUpdateEntity, ISoftDeleteEntity
{
    [StringLength(100)]
    public string Name { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(200)]
    public string? Address { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public ShopStatus Status { get; set; }

    public int Sort { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }

    public ICollection<ShopCategory> Categories { get; set; }
    public ICollection<ShopSugarOverride> SugarOverrides { get; set; }
    public ICollection<ShopToppingOverride> ToppingOverrides { get; set; }
}

public class ShopCategory : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int ShopId { get; set; }
    public Shop Shop { get; set; }

    [StringLength(100)]
    public string Name { get; set; }

    public int Sort { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }

    public ICollection<ShopMenuItem> MenuItems { get; set; }
}

public class ShopMenuItem : BaseDataEntity, ICreateEntity, IUpdateEntity, ISoftDeleteEntity
{
    public int CategoryId { get; set; }
    public ShopCategory Category { get; set; }

    public int DrinkItemId { get; set; }
    public DrinkItem DrinkItem { get; set; }

    [StringLength(200)]
    public string? Description { get; set; }

    public int Sort { get; set; }

    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }

    public ICollection<ShopMenuItemSize> Sizes { get; set; }
    public ICollection<ShopMenuItemSugar> Sugars { get; set; }
    public ICollection<ShopMenuItemIce> Ices { get; set; }
    public ICollection<ShopMenuItemTopping> Toppings { get; set; }
}

public class ShopMenuItemSize : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int MenuItemId { get; set; }
    public ShopMenuItem MenuItem { get; set; }

    public int SizeId { get; set; }
    public Size Size { get; set; }

    public decimal Price { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public class ShopMenuItemSugar : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int MenuItemId { get; set; }
    public ShopMenuItem MenuItem { get; set; }

    public int SugarId { get; set; }
    public Sugar Sugar { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public class ShopMenuItemIce : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int MenuItemId { get; set; }
    public ShopMenuItem MenuItem { get; set; }

    public int IceId { get; set; }
    public Ice Ice { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public class ShopMenuItemTopping : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int MenuItemId { get; set; }
    public ShopMenuItem MenuItem { get; set; }

    public int ToppingId { get; set; }
    public Topping Topping { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public class ShopSugarOverride : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int ShopId { get; set; }
    public Shop Shop { get; set; }

    public int SugarId { get; set; }
    public Sugar Sugar { get; set; }

    public decimal? Price { get; set; }
    public int? Sort { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}

public class ShopToppingOverride : BaseDataEntity, ICreateEntity, IUpdateEntity
{
    public int ShopId { get; set; }
    public Shop Shop { get; set; }

    public int ToppingId { get; set; }
    public Topping Topping { get; set; }

    public decimal? Price { get; set; }
    public int? Sort { get; set; }

    public DateTime CreatedAt { get; set; }
    public int Creator { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Updater { get; set; }
}
```

---

## API Endpoints

### 店家管理（後台）

#### 店家列表
```
GET /api/admin/shops?page=1&page_size=20&sort_by=sort&sort_order=asc&keyword=50嵐&status=1
```
- 遵循 SPEC.md 列表通用規範（分頁、排序、搜尋、篩選）
- keyword 搜尋欄位：name
- 篩選條件：status
- 預設排序：sort asc
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "name": "50嵐",
        "phone": "02-12345678",
        "address": "台北市信義區...",
        "status": 1,
        "sort": 1,
        "category_count": 3,
        "menu_item_count": 25,
        "created_at": "2025-01-01T00:00:00Z"
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

#### 取得單一店家
```
GET /api/admin/shops/{shopId}
```
- Response:
```json
{
  "data": {
    "id": 1,
    "name": "50嵐",
    "phone": "02-12345678",
    "address": "台北市信義區...",
    "note": "週一公休",
    "status": 1,
    "sort": 1,
    "created_at": "2025-01-01T00:00:00Z",
    "updated_at": "2025-01-01T00:00:00Z"
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

#### 新增店家
```
POST /api/admin/shops
```
- Request Body:
```json
{
  "name": "50嵐",
  "phone": "02-12345678",
  "address": "台北市信義區...",
  "note": "週一公休",
  "status": 1,
  "sort": 1
}
```
- name 唯一，重複回傳 409（`SHOP_ALREADY_EXISTS`）

#### 更新店家
```
PUT /api/admin/shops/{shopId}
```
- Request Body:
```json
{
  "name": "50嵐（信義店）",
  "phone": "02-12345678",
  "address": "台北市信義區...",
  "note": "週一公休",
  "status": 2,
  "sort": 1
}
```
- name 唯一（排除自身），重複回傳 409（`SHOP_ALREADY_EXISTS`）

#### 刪除店家
```
DELETE /api/admin/shops/{shopId}
```
- Soft delete：設定 IsDeleted = true、DeletedAt = now，底下品項一併 soft delete
- 列表查詢自動過濾 IsDeleted = true 的資料

#### 批次排序
```
PUT /api/admin/shops/sort
```
- Request Body:
```json
{
  "items": [
    { "id": 1, "sort": 2 },
    { "id": 2, "sort": 1 }
  ]
}
```

#### 批次刪除
```
DELETE /api/admin/shops/batch
```
- Request Body:
```json
{
  "ids": [1, 2, 3]
}
```
- 批次 soft delete，同上邏輯

---

### 菜單管理（後台）

菜單管理在店家的編輯頁面中進行，以店家為上下文操作分類與品項。

#### 取得店家完整菜單（含分類 + 品項）
```
GET /api/admin/shops/{shopId}/menu
```
- 回傳該店家所有分類、品項、尺寸價格、支援的甜度/冰塊/加料
- Response:
```json
{
  "data": {
    "categories": [
      {
        "id": 1,
        "name": "純茶類",
        "sort": 1,
        "items": [
          {
            "id": 1,
            "drink_item_id": 2,
            "drink_item_name": "紅茶",
            "description": null,
            "sort": 1,
            "sizes": [
              { "size_id": 2, "size_name": "中杯", "price": 30 },
              { "size_id": 3, "size_name": "大杯", "price": 40 }
            ],
            "sugar_ids": [1, 2, 3, 4, 5],
            "ice_ids": [1, 2, 3, 4, 5, 6],
            "topping_ids": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
          }
        ]
      }
    ],
    "sugar_overrides": [
      { "sugar_id": 6, "sugar_name": "加蜂蜜", "price": 10, "sort": null }
    ],
    "topping_overrides": [
      { "topping_id": 1, "topping_name": "珍珠", "price": 15, "sort": null }
    ]
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

#### 新增分類
```
POST /api/admin/shops/{shopId}/categories
```
- Request Body:
```json
{
  "name": "純茶類",
  "sort": 1
}
```
- 同店家內分類名稱唯一，重複回傳 409（`CATEGORY_ALREADY_EXISTS`）

#### 更新分類
```
PUT /api/admin/shops/{shopId}/categories/{categoryId}
```
- Request Body:
```json
{
  "name": "純茶類（更新）",
  "sort": 2
}
```

#### 刪除分類
```
DELETE /api/admin/shops/{shopId}/categories/{categoryId}
```
- 刪除分類同時刪除底下所有品項及相關設定（cascade）

#### 分類批次排序
```
PUT /api/admin/shops/{shopId}/categories/sort
```
- Request Body:
```json
{
  "items": [
    { "id": 1, "sort": 2 },
    { "id": 2, "sort": 1 }
  ]
}
```

#### 新增品項
```
POST /api/admin/shops/{shopId}/categories/{categoryId}/items
```
- Request Body:
```json
{
  "drink_item_id": 2,
  "description": null,
  "sort": 1,
  "sizes": [
    { "size_id": 2, "price": 30 },
    { "size_id": 3, "price": 40 }
  ],
  "sugar_ids": [1, 2, 3, 4, 5],
  "ice_ids": [1, 2, 3, 4, 5, 6],
  "topping_ids": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
}
```
- `drink_item_id` 必須存在於 DrinkItem，否則回傳 400（`DRINK_ITEM_NOT_FOUND`）
- `sizes` 至少要有一個尺寸，Price > 0
- `sugar_ids`、`ice_ids`、`topping_ids` 預設全勾（前端送出所有全域選項 id）

#### 透過品項名稱新增（含自動建立 DrinkItem）
```
POST /api/admin/shops/{shopId}/categories/{categoryId}/items
```
- Request Body（使用 `drink_item_name` 替代 `drink_item_id`）:
```json
{
  "drink_item_name": "新品項名稱",
  "description": null,
  "sort": 1,
  "sizes": [
    { "size_id": 2, "price": 30 }
  ],
  "sugar_ids": [1, 2, 3, 4, 5],
  "ice_ids": [1, 2, 3, 4, 5, 6],
  "topping_ids": [1, 2, 3, 4, 5, 6, 7, 8, 9, 10]
}
```
- 若 `drink_item_name` 不存在於 DrinkItem，自動新增一筆到全域 DrinkItem（Sort 設為最大值 + 1）
- 若已存在（不分大小寫），使用既有的 DrinkItem
- `drink_item_id` 和 `drink_item_name` 二擇一，同時提供時以 `drink_item_id` 優先

#### 更新品項
```
PUT /api/admin/shops/{shopId}/categories/{categoryId}/items/{itemId}
```
- Request Body:
```json
{
  "drink_item_id": 2,
  "description": "招牌推薦",
  "sort": 1,
  "sizes": [
    { "size_id": 2, "price": 35 },
    { "size_id": 3, "price": 45 }
  ],
  "sugar_ids": [1, 2, 3, 4, 5],
  "ice_ids": [1, 2, 3, 4],
  "topping_ids": [1, 2, 3]
}
```
- 整批覆蓋 sizes、sugar_ids、ice_ids、topping_ids

#### 刪除品項
```
DELETE /api/admin/shops/{shopId}/categories/{categoryId}/items/{itemId}
```
- Soft delete：設定 IsDeleted = true、DeletedAt = now

#### 品項批次排序
```
PUT /api/admin/shops/{shopId}/categories/{categoryId}/items/sort
```
- Request Body:
```json
{
  "items": [
    { "id": 1, "sort": 2 },
    { "id": 2, "sort": 1 }
  ]
}
```

---

### 店家覆寫管理（後台）

#### 取得店家甜度/加料覆寫設定
```
GET /api/admin/shops/{shopId}/overrides
```
- Response:
```json
{
  "data": {
    "sugar_overrides": [
      {
        "sugar_id": 6,
        "sugar_name": "加蜂蜜",
        "default_price": 5,
        "override_price": 10,
        "default_sort": 6,
        "override_sort": null
      }
    ],
    "topping_overrides": [
      {
        "topping_id": 1,
        "topping_name": "珍珠",
        "default_price": 10,
        "override_price": 15,
        "default_sort": 1,
        "override_sort": null
      }
    ]
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

#### 更新店家覆寫設定（整批覆蓋）
```
PUT /api/admin/shops/{shopId}/overrides
```
- Request Body:
```json
{
  "sugar_overrides": [
    { "sugar_id": 6, "price": 10, "sort": null }
  ],
  "topping_overrides": [
    { "topping_id": 1, "price": 15, "sort": null }
  ]
}
```
- 整批覆蓋，未提供的 sugar/topping 表示無覆寫（刪除既有覆寫記錄）
- Price 和 Sort 皆可為 null，表示使用全域預設值
- Price 若有值，必須 >= 0

---

### 前台 API（公開查詢）

#### 取得上架店家列表
```
GET /api/user/shops?page=1&page_size=20&keyword=50嵐
```
- 僅回傳 Status = Active 且 IsDeleted = false 的店家
- 分頁，依 sort 升冪排序
- keyword 可選，模糊搜尋 name
- Response:
```json
{
  "data": {
    "items": [
      {
        "id": 1,
        "name": "50嵐",
        "phone": "02-12345678",
        "address": "台北市信義區...",
        "note": "週一公休"
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

#### 取得店家完整菜單（含實際價格）
```
GET /api/user/shops/{shopId}/menu?keyword=紅茶
```
- 僅回傳 Status = Active 且 IsDeleted = false 的店家，否則回傳 404
- keyword 可選，模糊搜尋品項名稱（DrinkItem.Name），僅回傳符合的品項
- 若有 keyword 且無符合品項，仍回傳店家資訊但 categories 內 items 為空
- 甜度/加料價格回傳**實際生效價格**（有覆寫用覆寫，無覆寫用全域預設）
- 甜度/加料排序同理（有覆寫用覆寫，無覆寫用全域預設）
- Response:
```json
{
  "data": {
    "shop": {
      "id": 1,
      "name": "50嵐",
      "phone": "02-12345678",
      "address": "台北市信義區...",
      "note": "週一公休"
    },
    "categories": [
      {
        "id": 1,
        "name": "純茶類",
        "sort": 1,
        "items": [
          {
            "id": 1,
            "name": "紅茶",
            "description": null,
            "sort": 1,
            "sizes": [
              { "size_id": 2, "name": "中杯", "price": 30 },
              { "size_id": 3, "name": "大杯", "price": 40 }
            ],
            "sugars": [
              { "sugar_id": 1, "name": "正常糖", "price": 0, "sort": 1 },
              { "sugar_id": 6, "name": "加蜂蜜", "price": 10, "sort": 6 }
            ],
            "ices": [
              { "ice_id": 1, "name": "正常冰", "sort": 1 },
              { "ice_id": 5, "name": "熱", "sort": 5 }
            ],
            "toppings": [
              { "topping_id": 1, "name": "珍珠", "price": 15, "sort": 1 },
              { "topping_id": 2, "name": "椰果", "price": 10, "sort": 2 }
            ]
          }
        ]
      }
    ]
  },
  "message": null,
  "code": "SUCCESS",
  "errors": null
}
```

---

## Business Rules

### 店家
- 店家名稱唯一（不分大小寫，僅在未刪除的資料中檢查）
- 下架店家（Status = Inactive）前台不顯示
- 刪除為 soft delete（IsDeleted = true），底下品項一併 soft delete
- 列表查詢自動過濾 IsDeleted = true 的資料

### 菜單分類
- 同店家內分類名稱唯一（不分大小寫）
- 刪除分類時，底下品項一併 soft delete

### 品項
- 品項透過 DrinkItemId 引用全域通用品名
- 新增品項時可用 `drink_item_name` 自動建立新的 DrinkItem
- 每個品項至少設定一個尺寸價格，Price > 0
- 甜度、冰塊、加料預設全勾，管理員可取消不支援的選項
- 更新品項時 sizes、sugar_ids、ice_ids、topping_ids 整批覆蓋

### 價格覆寫
- 店家可統一覆寫甜度、加料的價格與排序
- 覆寫為店家層級，該店所有品項共用
- 未覆寫則使用全域 DefaultPrice / Sort
- 前台 API 回傳實際生效價格（覆寫優先於全域）

### 角色權限
- 後台 API（`/api/admin/*`）透過 RoleMiddleware 控制存取
- 店家管理（店家 CRUD + 菜單管理）對應 MenuConstants：ShopList = 10
- 覆寫設定（甜度/加料覆寫）對應 MenuConstants：ShopOverride = 17
- 前台 API（`/api/user/*`）公開存取，不需登入

---

## Frontend（Admin）

### 頁面：店家列表
- 路徑：`/shop/list`
- el-table 顯示店家列表（名稱、電話、地址、狀態、分類數、品項數、排序）
- 操作欄：編輯、刪除
- 新增按鈕
- 篩選：狀態（上架/下架）
- keyword 搜尋
- 支援多選批次刪除
- 支援拖拉排序

### 頁面：店家編輯（含菜單管理）
- 路徑：`/shop/list/create`、`/shop/list/:shopId/edit`
- 分為多個區塊：

#### 區塊一：店家基本資訊
- 店家名稱、電話、地址、備註、狀態、排序

#### 區塊二：甜度/加料覆寫設定
- el-table 顯示所有全域甜度/加料
- 每筆顯示全域預設價格，可輸入覆寫價格和覆寫排序
- 留空表示使用全域預設

#### 區塊三：菜單管理
- 左側：分類列表（可拖拉排序、新增、編輯、刪除）
- 右側：選中分類的品項列表（el-table）
- 品項操作：新增、編輯、刪除、拖拉排序

#### 品項編輯 Dialog
- 品項名稱：el-autocomplete，輸入時搜尋全域 DrinkItem，可選擇現有或新增
- 描述：el-input
- 尺寸價格：el-table，勾選支援的尺寸 + 輸入絕對價格
- 甜度：checkbox group（預設全勾）
- 冰塊：checkbox group（預設全勾）
- 加料：checkbox group（預設全勾）

---

## Success Criteria

### 店家
- [ ] `GET /api/admin/shops` 支援分頁、排序、keyword 搜 name、篩選 status
- [ ] `POST /api/admin/shops` name 唯一，重複回傳 409
- [ ] `PUT /api/admin/shops/{shopId}` name 唯一（排除自身），重複回傳 409
- [ ] `DELETE /api/admin/shops/{shopId}` soft delete 店家及底下品項
- [ ] `PUT /api/admin/shops/sort` 批次更新排序
- [ ] `DELETE /api/admin/shops/batch` 批次刪除

### 菜單
- [ ] `GET /api/admin/shops/{shopId}/menu` 回傳完整菜單（分類 + 品項 + 尺寸 + 選項 + 覆寫）
- [ ] `POST /api/admin/shops/{shopId}/categories` 同店家內分類名稱唯一
- [ ] `PUT /api/admin/shops/{shopId}/categories/{categoryId}` 更新分類
- [ ] `DELETE /api/admin/shops/{shopId}/categories/{categoryId}` 刪除分類，底下品項一併 soft delete
- [ ] `PUT /api/admin/shops/{shopId}/categories/sort` 分類批次排序

### 品項
- [ ] `POST .../items` 支援 drink_item_id 或 drink_item_name（自動建立 DrinkItem）
- [ ] `PUT .../items/{itemId}` 整批覆蓋 sizes、sugar_ids、ice_ids、topping_ids
- [ ] `DELETE .../items/{itemId}` 刪除品項
- [ ] `PUT .../items/sort` 品項批次排序
- [ ] 每個品項至少一個尺寸，Price > 0
- [ ] 甜度、冰塊、加料預設全勾

### 覆寫
- [ ] `GET /api/admin/shops/{shopId}/overrides` 回傳覆寫設定（含全域預設值對照）
- [ ] `PUT /api/admin/shops/{shopId}/overrides` 整批覆蓋覆寫設定
- [ ] 覆寫 Price >= 0

### 前台 API
- [ ] `GET /api/user/shops` 分頁回傳上架店家，支援 keyword 模糊搜尋 name，公開存取
- [ ] `GET /api/user/shops/{shopId}/menu` 回傳實際生效價格（覆寫優先於全域），支援 keyword 模糊搜尋品項名稱
- [ ] 下架店家回傳 404

### 角色權限
- [ ] 店家管理 API 透過 `[RequireRole(MenuConstants.ShopList, ...)]` 控制，無權回傳 403
- [ ] 覆寫設定 API 透過 `[RequireRole(MenuConstants.ShopOverride, ...)]` 控制，無權回傳 403
- [ ] 前台 API 公開存取，不需登入

---

## Boundaries

✅ Always:
- 店家名稱唯一（不分大小寫）
- 同店家內分類名稱唯一（不分大小寫）
- 每個品項至少一個尺寸，Price > 0
- 品項的 sizes、sugar_ids、ice_ids、topping_ids 整批覆蓋
- 覆寫整批覆蓋，未提供表示無覆寫
- 前台 API 回傳實際生效價格
- 下架店家前台不可存取

⚠️ Ask First:
- 修改 ShopMenuItem 結構（影響訂單關聯）
- 修改覆寫策略（如從店家層級改為品項層級）
- 新增菜單分類層級

🚫 Never:
- 硬刪除店家或品項（一律 soft delete）
- 允許品項不設定任何尺寸
- 允許尺寸 Price <= 0
- 覆寫 Price 為負數
- 前台暴露下架店家

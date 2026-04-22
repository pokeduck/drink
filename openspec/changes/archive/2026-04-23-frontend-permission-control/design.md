## Context

後端 `AdminMenuRole` 表已有完整的 `CanRead / CanCreate / CanUpdate / CanDelete` 欄位，`RoleMiddleware` 也已實作權限檢查邏輯。但目前：
- 沒有任何 Controller 掛 `[RequireRole]` attribute
- `/menus/me` API 只回傳選單結構，不帶 CRUD 權限
- 前端沒有任何權限判斷

## Goals / Non-Goals

**Goals:**
- `/menus/me` 回傳每個 menu 的 CRUD 權限
- 前端提供 `usePermission()` composable，任何頁面可用 `can(menuId, 'create')` 判斷
- 按鈕級別控制：無權限的「新增」「編輯」「刪除」按鈕不顯示
- 路由級別控制：直接輸入 URL 進入無權限頁面時重導
- 後端所有 Controller endpoint 掛上 `[RequireRole]`，雙重保護

**Non-Goals:**
- 不做欄位級權限（例如某些欄位唯讀）
- 不做動態權限更新（權限變更需要重新登入或重新載入頁面）

## Decisions

### 1. `/menus/me` 回傳結構擴充

在 `MenuTreeResponse` 加上權限欄位：

```csharp
public class MenuTreeResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Icon { get; set; }
    public string? Endpoint { get; set; }
    public int Sort { get; set; }
    public bool CanRead { get; set; }
    public bool CanCreate { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public List<MenuTreeResponse> Children { get; set; }
}
```

`AdminMenuService.GetMyMenus()` 在建立樹時從 `AdminMenuRole` 查出權限並填入。

### 2. 前端權限儲存：permission map in menu store

menu store 在 `fetchMenuData` 時，從回傳的 menu tree 提取權限 map：

```typescript
// store 裡
const permissions = ref<Map<number, { read: boolean, create: boolean, update: boolean, delete: boolean }>>()
```

key 是 menuId，value 是 CRUD 權限。從 menu tree 遞迴提取。

### 3. usePermission() composable

```typescript
const { can } = usePermission()

// 用法
can(MenuConstants.DrinkItem, 'create')  // → boolean
can(MenuConstants.AdminAccountList, 'delete')  // → boolean
```

內部從 menu store 的 permission map 查詢。MenuConstants 放在 `@app/core` 供前後端共用。

### 4. 頁面按鈕控制

```vue
<!-- 新增按鈕 -->
<el-button v-if="can(MENU.DrinkItem, 'create')" icon="Plus" @click="openCreate">
  新增品名
</el-button>

<!-- 編輯按鈕 -->
<el-button v-if="can(MENU.DrinkItem, 'update')" @click="openEdit(row)">
  編輯
</el-button>

<!-- 刪除按鈕 -->
<el-button v-if="can(MENU.DrinkItem, 'delete')" type="danger" @click="handleDelete(row)">
  刪除
</el-button>
```

### 5. Route middleware 權限擋頁面

定義 route → menuId + 最低權限的對應表：

```typescript
// middleware/permission.global.ts
const routePermissions: Record<string, { menuId: number, action: CrudAction }> = {
  '/drink-option/item': { menuId: MENU.DrinkItem, action: 'read' },
  '/admin-account/create': { menuId: MENU.AdminAccountList, action: 'create' },
  // ...
}
```

進入頁面前檢查權限，無權限重導到首頁或 403 頁面。

### 6. 後端 Controller 掛 `[RequireRole]`

每個 endpoint 加上對應的 attribute：

```csharp
[HttpGet]
[RequireRole(MenuConstants.DrinkItem, CrudAction.Read)]
public async Task<IActionResult> GetList(...) { ... }

[HttpPost]
[RequireRole(MenuConstants.DrinkItem, CrudAction.Create)]
public async Task<IActionResult> Create(...) { ... }
```

### 7. MenuConstants 前後端共用

後端已有 `MenuConstants.cs`。前端在 `@app/core` 新增對應的 `menuConstants.ts`，值必須一致。
（未來可考慮從後端自動生成，但目前手動維護即可。）

## Risks / Trade-offs

- **[前後端 MenuConstants 不同步]** → 手動維護有風險。新增 menu 時要記得兩邊都改。可加 comment 提醒。
- **[權限快取不更新]** → 管理員權限被改後，需要重新載入頁面才生效。可接受，因為權限變更頻率極低。
- **[頁面路由對應表維護成本]** → 新增頁面時要記得加入 routePermissions map。但漏加的後果只是前端沒擋（後端仍然擋），不是安全漏洞。

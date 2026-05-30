## Why

訂單（揪團）是平台的核心業務，但目前後端尚未實作。後台需要可以查看所有揪團、推進狀態、必要時取消、匯出 Excel 給店家、通知參與者飲料到貨。這個 change 先把後台這條路徑打通，前台揪團流程（user-order）另外開 change 處理。

## What Changes

- 新增前後台共用的訂單 Entity：`GroupOrder`、`OrderItem`、`OrderItemTopping`、`GroupOrderStatus` enum
- 新增後台 Order API（`/api/admin/orders`）：列表（分頁/排序/搜尋/篩選）、詳情、改狀態、取消、匯出 Excel、發送通知
- 引入 ClosedXML（MIT，純 OSS）作為 Excel 產生套件
- 擴充 `IEmailSender` 加入訂單通知方法，並提供 mock log 實作（沿用現有 `LogEmailSender` 模式，等正式 SMTP/Resend 上線再替換）
- 通知本期只實作 email 路徑：`NotificationType` ∈ { `Email`, `Both` } 的使用者發送 email；`WebPush` 暫時 skip（Firebase 上線後另開 change 補上 push 通道）；`None` 不發送
- 沿用既有 `ErrorCodes.cs` 模組 05（Order）區段已預留的錯誤碼（`InvalidStatusTransition`、`CannotCancelOrder`、`OrderNotFound`、`OrderNotActive` 等）
- AdminMenu 訂單管理項目（Id=7 父節點、Id=8 `/order/list`）已存在於 `AdminMenuSeeder`，本 change 不再變動 seed
- 不含：前台 user-order API、Admin Nuxt UI 頁面、Firebase push、通知歷史紀錄資料表（通知歷史在後續 notification capability 處理）

## Capabilities

### New Capabilities

- `admin-order`：後台訂單管理 — Entity 定義（前後台共用）、Admin Order API（列表/詳情/狀態管理/取消/匯出/通知）、Excel 匯出、Email 通知整合

### Modified Capabilities

（無 — `admin-permission` 中的 AdminMenu seed 雖會新增一筆訂單管理 menu，但屬於資料變動而非 spec requirement 變動）

## Impact

**新增程式碼**
- `api/Domain/Entities/`：`GroupOrder.cs`、`OrderItem.cs`、`OrderItemTopping.cs`、`Enums/GroupOrderStatus.cs`
- `api/Domain/Interfaces/`（若需要）：order 相關介面
- `api/Infrastructure/Data/Configurations/`：`GroupOrderConfiguration.cs`、`OrderItemConfiguration.cs`、`OrderItemToppingConfiguration.cs`
- `api/Application/Requests/Admin/Order/`：狀態 / 篩選 request DTO
- `api/Application/Responses/Admin/Order/`：列表 / 詳情 response DTO
- `api/Application/Mappings/`：`AdminOrderMapper.cs`
- `api/Application/Services/`：`AdminOrderService.cs`、`OrderExportService.cs`、`OrderNotificationService.cs`
- `api/Admin.API/Controllers/`：`OrdersController.cs`
- `api/Application/Interfaces/IEmailSender.cs`：擴充 order notification 方法
- `api/Infrastructure/Services/LogEmailSender.cs`：補上 order notification mock 實作
- `api/Migrator/Migrations/`：新增 migration（GroupOrder / OrderItem / OrderItemTopping）

**新增套件依賴**
- `ClosedXML`（MIT license）

**前端**
- 完成後跑 `pnpm generate` 重新產出 `web/internal/api-types/src/admin.d.ts`
- Admin Nuxt UI 頁面延後到後續 change

**資料庫遷移**
- 新增三張資料表：`group_order`、`order_item`、`order_item_topping`（依現有 snake_case table naming convention）

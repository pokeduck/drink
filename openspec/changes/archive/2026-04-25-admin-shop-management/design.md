## Context

飲料選項模組（DrinkItem、Sugar、Ice、Topping、Size）已完成，遵循 Entity → DbContext → Mapper → Response → Request → Service → Controller 的開發流程。店家模組將引用這些既有 Entity 作為 FK，結構較飲料選項複雜（多層巢狀關聯），但開發模式一致。

現有基礎設施：
- `BaseService` 提供 `GetRepository<T>()`、`Success()`/`Fail()` helper
- `BaseController` 提供 `ApiOk()`/`ApiError()` 統一回應格式
- `IGenericRepository<T>` 提供 CRUD + 分頁查詢
- Mapperly compile-time mapper、Scrutor 自動 DI 註冊
- `@app/api-types` 透過 openapi-typescript 從 swagger.json 產生型別
- `/migrate` skill 產生 EF Core migration、Migrator project 執行 migration

## Goals / Non-Goals

**Goals:**
- 完成店家 CRUD + 菜單管理 + 覆寫設定的後台 API 與前端頁面
- 菜單管理 UI 採可收折分類 + 拖拉排序 + 品項展開細項的互動設計
- Shop Entity 新增 `MaxToppingPerItem`（每種料上限）和 `MaxToppingCount`（單杯總份數上限）
- `MaxToppingCount` 新增店家時從系統設定帶入預設值

**Non-Goals:**
- 訂單功能（依賴店家但不在此 change 範圍）
- 前台 API 與前台前端（留待後續 change）
- 拖拉排序的前端套件選型（如需引入新 dependency 在實作時決定）
- 飲料選項被 ShopMenuItem 引用時的刪除保護（TODO 已標記在 DrinkOptionService，可在此 change 一併處理但非必要）

## Decisions

### 1. Entity 一次全建、單一 migration

所有 11 個 Entity + ShopStatus enum 在同一個 migration 中建立，避免多次 migration 產生的碎片化問題。

替代方案：分批 migration（先 Shop → 再 Category/MenuItem → 再 Override）
不採用原因：Entity 之間 FK 依賴緊密，分批只增加複雜度。

### 2. 菜單管理 API 使用巢狀路由

```
/api/admin/shops/{shopId}/categories
/api/admin/shops/{shopId}/categories/{categoryId}/items
```

符合 REST 資源巢狀關係，路由本身表達歸屬，Controller 層驗證資源歸屬（categoryId 屬於 shopId）。

### 3. 品項子關聯（sizes/sugars/ices/toppings）整批覆蓋

更新品項時 delete-then-insert 子關聯，而非 diff patch。簡單且不易出錯，子關聯無獨立 ID 語意。

### 4. 加料限制欄位放在 Shop 層級

`MaxToppingPerItem` 和 `MaxToppingCount` 放在 Shop Entity，全店統一規則。

替代方案：放在 ShopMenuItem 層級（每品項獨立設定）
不採用原因：使用者在 explore 階段確認放店家層級即可，降低管理複雜度。

### 5. 覆寫設定整批覆蓋

`PUT /api/admin/shops/{shopId}/overrides` 送出完整覆寫清單，未包含的項目視為移除覆寫。與品項子關聯相同的 delete-then-insert 策略。

## Risks / Trade-offs

- **大量 Entity 關聯** → ShopMenuItem 有 4 個子關聯表，新增/更新品項時需在 transaction 內處理，確保一致性
- **菜單查詢效能** → `GET /api/admin/shops/{shopId}/menu` 需 eager load 多層關聯，可能產生 N+1 → 使用 `Include().ThenInclude()` 明確指定載入路徑
- **前台菜單 API 回傳量** → 單次回傳完整菜單可能資料量大 → 實務上一家店的菜單不會超過數百品項，可接受
- **拖拉排序套件** → 前端需要拖拉排序功能，目前專案未引入相關套件 → 實作時選型決定

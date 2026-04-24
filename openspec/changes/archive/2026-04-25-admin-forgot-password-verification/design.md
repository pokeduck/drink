## Context

後台已有完整的「註冊驗證信」管理頁面（`register.vue`），且忘記密碼的 Backend API 已完成，共用相同的 `VerificationListResponse` 資料結構。前端 `forgot-password.vue` 目前為 stub。

## Goals / Non-Goals

**Goals:**
- 實作與 `register.vue` 一致的忘記密碼驗證信管理頁面
- 使用現有 API endpoint 與型別定義，不需新增後端程式碼

**Non-Goals:**
- 修改 Backend API 或 Entity
- 實作實際寄信功能（已有 TODO，不在此次範圍）
- 前台會員忘記密碼的使用者端流程

## Decisions

### 1. 複製 register.vue 並調整為 forgot-password 版本

**理由**：兩頁功能完全相同（列表、篩選、排序、單筆/批次重發），僅 API path 與 menuId 不同。直接以 register.vue 為模板修改，確保 UX 一致性。

**替代方案**：抽取共用元件 → 過度抽象，目前只有兩頁，維護成本反而更高。

### 2. 權限使用 Menu Id = 22

**理由**：`AdminMenuSeeder` 已定義 Id=22 為「忘記密碼驗證信」，與 Id=6「註冊驗證信」獨立管理。

## Risks / Trade-offs

- [風險] register.vue 未來改版時需同步修改 forgot-password.vue → 兩頁結構簡單，同步成本低；若日後頁面增多再考慮抽元件

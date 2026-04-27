# admin-shop-hub

## Purpose

定義 Admin 後台「店家層級設定」的 hub-and-spoke UI 模式：所有 `/shop/[id]/*` 之下的子設定（基本資訊、圖片管理、選項啟用、覆寫設定…）共用一個 Nuxt 父 layout container，透過 sub-tab navigation 切換獨立的子 route，URL 與 tab 一一對應。Sidemenu 不再為任一店家層級設定預留入口；唯一進入點為 `/shop/list` 行內按鈕。原有 sidemenu 路徑保留 redirect 以相容舊書籤。

## Requirements

### Requirement: Hub layout 結構
Admin 後台 SHALL 將店家層級設定整合為 hub-and-spoke layout。`/shop/[id]/*` 之下的所有設定（基本資訊、圖片、選項啟用、覆寫設定等）SHALL 共用同一個 layout container，並透過 sub-tab navigation 切換。每個 sub-tab 對應一個獨立的 Nuxt 子 route（不使用 `v-if` / `v-show` 切換），URL 與 tab 一一對應。

#### Scenario: 進入店家編輯頁
- **WHEN** 管理員從店家列表點擊某店家的「編輯」按鈕
- **THEN** 系統 SHALL 導向 `/shop/[id]/edit`，並在 hub layout 中以「基本資訊」tab 為 active 狀態

#### Scenario: 切換 sub-tab
- **WHEN** 管理員在 hub 內點擊「選項啟用」tab
- **THEN** URL SHALL 變更為 `/shop/[id]/options`，並渲染對應內容；hub layout（breadcrumb / 標題 / 返回按鈕）SHALL 保持不變

#### Scenario: 直接以 URL 進入子 tab
- **WHEN** 管理員直接輸入 `/shop/[id]/overrides`
- **THEN** 系統 SHALL 渲染 hub layout 並以「覆寫設定」tab 為 active 狀態

### Requirement: Hub sub-tab 範圍
Hub layout SHALL 包含以下 sub-tabs：
- 基本資訊（`/shop/[id]/edit`）— 既有
- 圖片管理（`/shop/[id]/images`）— 既有
- 選項啟用（`/shop/[id]/options`）— 新增
- 覆寫設定（`/shop/[id]/overrides`）— 從原 sidemenu `/shop/override` 移入 hub

每個 sub-tab SHALL 根據使用者權限獨立顯示 / 隱藏（無權限的 tab 不渲染）。

#### Scenario: 無 ShopOverride 權限
- **WHEN** 管理員角色無 `ShopOverride` Read 權限
- **THEN** hub 內「覆寫設定」tab SHALL 不顯示

#### Scenario: 無 ShopOptions 權限
- **WHEN** 管理員角色無 `ShopOptions` Read 權限
- **THEN** hub 內「選項啟用」tab SHALL 不顯示

#### Scenario: 任一 hub sub-tab 無權限直接輸入 URL
- **WHEN** 管理員直接輸入 `/shop/[id]/options` 但角色無 ShopOptions Read 權限
- **THEN** route middleware SHALL 重導至首頁

### Requirement: Hub 內 sub-tab 各自管理表單狀態
每個 sub-tab SHALL 各自管理表單載入、儲存、未儲存保護（useUnsavedGuard），sub-tab 之間互不影響。切換 sub-tab 時，若當前 tab 有未儲存修改，SHALL 跳離開確認。

#### Scenario: 切 tab 時保留未儲存提示
- **WHEN** 管理員在「基本資訊」tab 修改了店名但未儲存，點擊「選項啟用」tab
- **THEN** 系統 SHALL 跳出未儲存修改確認提示

#### Scenario: 各 tab 獨立儲存
- **WHEN** 管理員儲存「選項啟用」tab
- **THEN** 「基本資訊」tab 的修改 SHALL 不受影響

### Requirement: 店家列表為唯一入口
Admin sidemenu SHALL 不顯示「店家編輯」「圖片管理」「選項啟用」「覆寫設定」任一葉節點。所有店家層級設定 SHALL 從 `/shop/list` 行內按鈕進入 hub。

#### Scenario: sidemenu 渲染
- **WHEN** 管理員載入 admin 後台 sidemenu
- **THEN** 「店家管理」群組底下 SHALL 僅顯示「店家列表」葉節點，覆寫設定 / 選項啟用 SHALL 不顯示

#### Scenario: 店家列表行內按鈕
- **WHEN** 管理員在 `/shop/list` 看到某一列店家
- **THEN** 該列 SHALL 提供「編輯」按鈕（即進入 hub 的入口），點擊後導向 `/shop/[id]/edit`

### Requirement: 既有 sidemenu 路徑相容
原有 sidemenu 路徑 `/shop/override` SHALL 在過渡期保留為 redirect 至 `/shop/list`，並可顯示一次性提示「覆寫設定已整合至店家編輯」。

#### Scenario: 訪問舊路徑
- **WHEN** 管理員透過舊書籤或外部連結進入 `/shop/override`
- **THEN** 系統 SHALL 重導至 `/shop/list`

## Context

`Shop` entity 目前只有基本資訊欄位（name / phone / address / note / status / sort / max_topping_per_item）。本次需要為店家加入兩個展示性質欄位：

- **Cover 圖片**：店家識別主視覺，每店至多一張
- **External URL**：店家外部連結（管理員實際用途是 Google Map URL，但語意保持通用）

既有的 `admin-shop-image` capability 處理「店家圖庫池 + 品項圖片綁定」，使用 `ShopImage` entity 並依賴「DrinkItemId IS NULL = 孤兒」這個語意。本次 cover 必須避免污染該語意。

## Goals / Non-Goals

**Goals:**
- 允許 Admin 在 create / edit 兩個頁面上傳、替換、移除 cover 圖
- 允許 Admin 設定店家外部連結（http / https only）
- Cover 上傳統一走 image-upload pipeline（與 `admin-shop-image` 同代理模式），檔案驗證與錯誤碼透傳
- 對 External URL 做格式驗證，攔下 `javascript:` / `mailto:` / `ftp:` 等非 http(s) scheme

**Non-Goals:**
- 不擴充 `ShopImage` entity / 不修改 `admin-shop-image` 既有契約
- 不在 cover 上傳 endpoint 處理排序、封面切換、孤兒化等邏輯（這些屬於品項圖庫範疇）
- 不在替換 cover 時刪除 image-upload server 上的舊檔案（content-addressed，可能仍被引用）
- 不在 `ShopListResponse` 加 cover 欄位（列表不需要顯示 cover，避免 payload 膨脹）
- 不為 External URL 做 reachability 檢查（不打 HEAD 請求驗證連結真實存活）

## Decisions

### 1. Cover 走 Shop entity 直接欄位，不沾邊 ShopImage

**選擇**：在 `Shop` entity 新增 `CoverImagePath string?` (nullable, max 500)。

**Why**：
- 店家 cover 永遠就一張、跟品項無關，跟「品項圖庫池（1:N、要排序、要綁品項、要孤兒化）」是不同語意
- `ShopImage` 把 `DrinkItemId IS NULL` 當作孤兒語意（可被一鍵清掉），如果 cover 也用 `DrinkItemId=null` 就會跟孤兒衝突
- Shop entity 直接欄位最簡單，跟 phone / address 同層級；前端表單也直接綁

**Alternative considered**：在 `ShopImage` 加 `IsShopCover` 旗標 → 否決，因為要再多一條 partial unique index、UI 還要分流，複雜度遠超收益。

### 2. Cover 上傳走獨立 endpoint（A1 方案）

**選擇**：新增 `POST /api/admin/shops/{shopId}/cover-image`，multipart upload，內部 forward 至 image-upload pipeline，成功後立即寫入 `Shop.CoverImagePath`。

**Why**：
- 「上傳即套用」UX 順，使用者上傳完馬上看到 cover 變更
- 兩階段（先傳檔再 PUT）會在使用者沒按儲存時留下孤兒檔
- 與 `admin-shop-image` 的 `POST /api/admin/shops/{shopId}/images` 代理模式一致，錯誤碼透傳邏輯可複用

**移除 cover 不另開 endpoint**：透過 `PUT /api/admin/shops/{shopId}` 把 `cover_image_path` 設為 null 隨其他欄位一起送。
- 移除 cover 不是獨立行為，就是「編輯時把它清掉」
- 跟其他 nullable 欄位（phone / address / note）的 null 化路徑一致

### 3. 替換 cover 不刪舊檔

**選擇**：替換 cover 只更新 `Shop.CoverImagePath`，舊 path 對應的 image-upload 檔案保留。

**Why**：
- image-upload pipeline 是 content-addressed（檔名為 hash），可能被其他 entity（其他 Shop / ShopImage）引用
- 跟 `admin-shop-image` 第 163-178 行「刪 row 不刪檔」一致
- 真正的孤兒檔清理應由 image-upload pipeline 自己的 GC job 處理（不在本變更範疇）

### 4. External URL 驗證：自製 `HttpUrlAttribute`

**選擇**：新增 `HttpUrlAttribute`，內部使用：

```csharp
Uri.TryCreate(value, UriKind.Absolute, out var uri)
  && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
```

放置位置：`api/Application/Validators/HttpUrlAttribute.cs`（與其他 Application 層 attribute 一起）。
驗證失敗訊息：「請輸入有效的連結」。

**Why C# 內建 `[Url]` 不夠**：
- `[Url]` 接受 ftp / mailto / 甚至更冷門的 scheme，沒辦法限制只 http / https
- 雖然顯示時前端會 escape，但攔在最前面對 admin 操作更安全（防誤貼）

**Alternative considered**：用 regex → 否決，URL regex 容易不完整或誤判，`Uri.TryCreate` 是 .NET 的 canonical 解析器。

### 5. 前端 cover 上傳用 useImageUploadQueue 但簡化為單張

**選擇**：複用既有 `useImageUploadQueue` composable，但每次只放一個檔案進佇列；上傳成功後把回傳 path 寫入表單欄位 `form.cover_image_path`，UI 顯示縮圖（透過 `useAssetHost.assetUrl(path)` 拼接）。

**Why**：
- 行為（progress / error 顯示）與 `admin-shop-image` 一致
- 元件層保持單純，不需要管理多檔佇列
- 上傳完直接由 endpoint 寫入 Shop row，前端只需更新本地 `form.cover_image_path` reactive

### 6. Spec 歸屬於 admin-shop capability（不另開 capability）

**選擇**：在 `openspec/specs/admin-shop/spec.md` 新增 ADDED Requirements。

**Why**：
- `cover_image_path` 與 `external_url` 是 Shop entity 直接欄位，跟 name / phone / address 同一層
- 開新 capability（如 admin-shop-cover）反而割裂語意：cover 是 shop 的一部分，不是獨立資源
- 不更動 `admin-shop-image` capability — cover 不屬於圖庫池

## Risks / Trade-offs

- **舊 cover 檔案累積** → image-upload pipeline 的 GC 由該 capability 自行處理；本變更維持「不刪檔」哲學一致即可，不需在此處加額外清理邏輯
- **HttpUrlAttribute 驗證失敗訊息與其他欄位風格不一致** → 用戶已指定為「請輸入有效的連結」，順從即可
- **Cover 上傳 endpoint 的權限**：對齊 admin-shop CRUD 的 `[RequireRole(MenuConstants.ShopList, CrudAction.Update)]`（替換 cover 屬於 update 行為）
- **Create 流程 UX 取捨** → Cover 上傳 endpoint 需要 `shopId`，但 create 階段尚未有 ID。取捨：create 頁不顯示 cover 上傳區，只在 edit 頁可上傳；create 頁的 external_url 仍可填。**已和用戶確認 create / edit 兩頁都要支援 → cover 部分採取「create 頁顯示但提示『建立後才能上傳』，建立完跳轉到 edit 頁可上傳」**；external_url 在 create 頁直接接受。
- **List 不顯示 cover** → 若日後產品想在 ShopList 顯示縮圖，再加欄位即可；現在不預先加，避免 payload 膨脹
- **檔案驗證錯誤碼透傳** → image-upload pipeline 的 `INVALID_FILE_TYPE` / `INVALID_IMAGE` / `FILE_TOO_LARGE` 直接 forward，前端 errors 字典 key 用 `file`

## Open Questions

無 — 所有決策已在 explore 階段與用戶敲定。

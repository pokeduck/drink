# Design: add-shop-drink-image

## Context

`image-upload` capability 已上線，提供：
- `POST /api/admin/upload` 單檔上傳（magic bytes + SkiaSharp + 長邊 4000 縮圖 + WebP@85 + SHA-256 去重）
- `GET /api/admin/upload/asset-host` 動態 base URL
- 回傳相對路徑 `/assets/images/{hash[0:6]}/{hash}.webp`
- 前端 composable `useImageUploadQueue` + `useAssetHost`

本 change 只處理「ShopImage 業務 Entity」與「店家／品項對圖片的綁定關係」，圖片本身的處理、儲存、CDN cache 都委外給 image-upload。

## Entity Model

```
ShopImage : BaseDataEntity, ICreateEntity, IUpdateEntity
  Id              int
  ShopId          int          (FK → Shop, NOT NULL)
  DrinkItemId     int?         (FK → DrinkItem, NULLABLE — null = 孤兒)
  Path            string       (image-upload 回傳的相對路徑，如 "/assets/images/ab/abc...webp")
  Hash            string       (SHA-256，64 chars；冗餘存以便日後查詢、不一定要做 unique)
  Sort            int          (僅在 DrinkItemId != null 時有意義；孤兒態為 0)
  IsCover         bool         (僅在 DrinkItemId != null 時可為 true)
  OriginalFileName string      (使用者上傳時的原檔名，給後台辨識用)
  FileSize        long         (處理後 webp bytes)
  Width           int          (處理後寬度)
  Height          int          (處理後高度)
  CreatedAt / Creator / UpdatedAt / Updater
```

**為什麼存 Path 而不是 Url？**

image-upload 的設計：DB 存相對路徑（`/assets/images/...`），前端透過 `useAssetHost` 拼上 base URL。這讓 dev/prod/CDN 切換只動 config 不動資料。所以 ShopImage 也跟進。

**為什麼存 Hash？**

冗餘儲存方便日後做：
1. 日後排程清理孤兒檔案時，可從 ShopImage table 查出仍被引用的 hash 集合
2. 跨 ShopImage 找重複（例如管理員想知道這張圖在哪幾家店都用了）

不做 `UNIQUE(Hash)`，因為：同一 hash 可以同時存在多筆 ShopImage（不同店家 / 不同品項都引用同一張圖）— 這正是 image-upload 去重的好處。

**約束**

| 約束 | 實作 |
|------|------|
| 一個 (Shop, DrinkItem) 最多 10 張 | 應用層 (Service) 驗證；達上限 API 回 400 + `IMAGE_LIMIT_REACHED` |
| 一個 (Shop, DrinkItem) 至多一張封面 | DB partial unique: `UNIQUE (ShopId, DrinkItemId) WHERE IsCover = true` |
| 孤兒（DrinkItemId IS NULL）無上限 | 不限 |
| 孤兒不參與排序與封面 | 應用層保證 Sort=0、IsCover=false |

**Index**
- `INDEX (ShopId, DrinkItemId, Sort)` 列表排序用
- `INDEX (ShopId) WHERE DrinkItemId IS NULL` 孤兒查詢用
- `INDEX (Hash)` 日後跨表查詢用（非 unique）

## State Machine

```
                ┌──────────┐
                │  不存在   │
                └────┬─────┘
                     │ POST /images (drink_item_id 選填)
       ┌─────────────┴─────────────┐
       │                           │
       ▼                           ▼
  ┌─────────┐  PATCH 改綁     ┌─────────┐
  │  孤兒   │ ─────────────►  │ 已綁品項 │
  │(null FK)│ ◄─────────────  │         │
  └────┬────┘  品項頁「移除」  └────┬────┘
       │                           │
       │                           │ ShopMenuItem 軟刪
       │                           │ (cascade orphan)
       │                           ▼
       │                      ┌─────────┐
       │                      │  孤兒   │
       │                      └────┬────┘
       │                           │
       │ Pool 批刪 / 一鍵清孤兒    │
       └───────────┬───────────────┘
                   ▼
            ┌──────────┐
            │ DB 真刪   │  (僅 ShopImage row；webp 檔保留)
            └──────────┘
```

## API Surface

| Method | Path | 用途 |
|--------|------|------|
| GET | `/api/admin/shops/{shopId}/images` | 列表（支援 `filter=all\|orphan\|assigned`、`drink_item_id` 篩選、分頁） |
| GET | `/api/admin/shops/{shopId}/drink-items/{drinkItemId}/images` | 單品項圖列表（已用於品項編輯頁） |
| POST | `/api/admin/shops/{shopId}/images` | 上傳（multipart；`drink_item_id` 選填，未填即孤兒） |
| PATCH | `/api/admin/shops/{shopId}/images/{id}` | 改綁品項 / 設為封面 / 改 Sort（部分更新） |
| PATCH | `/api/admin/shops/{shopId}/drink-items/{drinkItemId}/images/sort` | 批次更新該品項的圖片排序 |
| DELETE | `/api/admin/shops/{shopId}/images` | 批量刪 DB row（body: `ids: int[]`） |
| DELETE | `/api/admin/shops/{shopId}/images/orphans` | 一鍵清孤兒（DB row） |

回傳統一 `Dictionary<string, string[]>` errors 格式（與其他模組一致）。

## image-upload 整合

- 上傳流程：Admin frontend → `POST /api/admin/shops/{shopId}/images` → `AdminShopImageService` 內部呼叫 `POST /api/admin/upload` proxy → Upload.API
  - 不直接打 image-upload，要透過 ShopImage 業務層，因為要驗證：是否在同 shop 範圍、品項是否存在、是否達 10 張上限、處理 sort/cover
- image-upload 回傳的 `path`、`hash`、`size`、`width`、`height` 寫入 ShopImage
- **改綁品項只動 DB FK**：path 不變，因為 image-upload 是 content-addressed，路徑不含 shop/drinkItem 資訊
- **副檔名白名單**：完全跟隨 image-upload（jpg/jpeg/png/gif/webp）
- **單檔上限**：完全跟隨 image-upload（10MB）
- **不重複定義驗證**：所有 image 驗證錯誤直接 forward image-upload 的錯誤碼（`INVALID_FILE_TYPE` / `INVALID_IMAGE` / `FILE_TOO_LARGE`）

## 封面規則細節

- **預設封面**：第一張上傳的圖自動 IsCover=true
- **手動切換**：PATCH 把目標圖 IsCover=true，後端在 transaction 中：
  1. `UPDATE shop_images SET is_cover=false WHERE shop_id=X AND drink_item_id=Y AND is_cover=true`
  2. `UPDATE shop_images SET is_cover=true WHERE id=Z`
- **刪除封面 / 移為孤兒（含被孤兒化）**：自動把 sort 最小的剩餘圖補為封面；無剩餘則無封面
- **變孤兒時**：IsCover 強制 false、Sort 強制 0

## Cascade Orphan 觸發點

| 事件 | 行為 |
|------|------|
| ShopMenuItem 軟刪 | 該 (ShopId, DrinkItemId) 所有圖 DrinkItemId → null、Sort → 0、IsCover → false |
| ShopCategory 軟刪（連帶 items） | 同上，套用每個受影響 item |
| Shop 軟刪 | 圖**不**動（店家恢復則照常） |
| ShopMenuItem.DrinkItemId 變更 | 第一版**不支援**改 DrinkItemId（既有 spec 也沒明確開放） |
| DrinkItem 全域刪除（若有此操作） | 該 DrinkItem 在所有 Shop 的圖 → 孤兒（不自動真刪，留給 pool 清理） |

## 檔案刪除策略

ShopImage 的「刪除」操作 **只刪 DB row**，不刪 Upload.API 上的 webp 檔案。

理由：
- image-upload 是 content-addressed 去重設計：兩個 ShopImage 可能指向同一個 webp 檔（不同店家上傳了同一張圖）
- 直接刪檔會誤傷其他引用
- 引用計數機制 over-engineering（要跨 entity 查詢、要在多 transaction 邊界維護一致性）
- 磁碟成本相對低，孤兒檔留著等日後排程清

未來會另開 change `cleanup-orphan-image-files`：定期查 image-upload 上所有檔案 hash，去 ShopImage 與其他可能的引用 table（未來 client-page banner 等）查 hash 集合，沒被引用的檔才真刪。

## 前端：Admin Pool 頁布局

```
/shop/[id]/images
─────────────────────────────────────────────────────────────
[← 返回] 店家圖庫: XX 飲料店                 [+ 上傳圖片]

篩選 [全部 ▼] [品項 ▼ id+name]    搜尋 [...........]

□ 全選此頁  已選 3   [批量刪除] [一鍵清孤兒]

┌────────┬────────┬────────┬────────┐
│ ☑ ★    │ ☐      │ ☐      │ ☑      │
│ 圖     │ 圖     │ 圖     │ 圖     │
│珍奶 ★  │珍奶    │烏龍    │未使用  │
└────────┴────────┴────────┴────────┘
                        ▼ 點圖開 Drawer
                ┌─────────────────────┐
                │ 圖片詳情             │
                │ [大圖預覽]           │
                │ 檔名: ...            │
                │ 大小: 1.2 MB        │
                │ 尺寸: 4000×2666     │
                │ 上傳時間: ...        │
                │ 上傳者: ...          │
                │ ─────────           │
                │ 綁定品項:            │
                │ [#12 珍奶 ▼] [儲存]  │
                │ ☑ 設為封面           │
                │ [改為孤兒]           │
                │ [刪除]               │
                └─────────────────────┘
```

## 前端：Admin Shop Edit 頁的圖片區塊

ShopMenuItem 編輯時：
- 在表單下方加區塊「圖片（n/10）」
- 拖放上傳區（多檔，序列上傳；達 10 張禁用）— 用 `useImageUploadQueue`
- 已上傳縮圖網格：拖拉排序、星號設封面、點擊縮圖看大圖、按 X 移除（=孤兒化）
- 旁邊一顆「從孤兒池挑」按鈕 → 開 Modal 顯示該店孤兒圖，多選後加入此品項

## 前端 composable 整合

| Composable | 用途 |
|---|---|
| `useImageUploadQueue` | 多檔拖入後依序上傳；本 change 包一層 `useShopImageUpload` 把上傳結果 callback 進 ShopImage 建立 API |
| `useAssetHost` | 縮圖、Drawer 大圖預覽都用 `assetUrl(image.path)` 拼完整 URL |
| `useLoading` / `useApiFeedback` | 既有，沿用 |

## UI 元件

新增：
- `ShopImagePoolGrid.vue` — 縮圖網格（複用於 pool 頁與「從孤兒池挑」Modal）
- `ShopImageDrawer.vue` — 點圖後的右側詳情抽屜
- `ShopImageItemPicker.vue` — 「從孤兒池挑」Modal

不另寫上傳 UI 元件 — 直接用 `useImageUploadQueue` + 拖放 div。

## 不在此 change 範圍

- 前台 (Client) 圖片呈現（封面 / 輪播 / lightbox）→ 後續另開 change 修改 `client-pages`
- 圖片自動裁切多尺寸（thumb / medium / original）→ 由 Cloudflare Image Resizing 處理
- 圖片標籤 / 分類系統 → 不做
- EXIF / alt text → image-upload 已清掉 EXIF；alt text 暫不做
- 跨店家圖庫總覽 → 不做（pool 永遠 scoped to one shop）
- Upload.API 檔案級刪除 → 另開 change 做排程清孤兒

## Risks / Trade-offs

| Risk | Mitigation |
|---|---|
| 同 hash 圖被多店家引用後，UI 不會直接顯示這事 | OK — 對使用者無意義；管理員看不到「這張其實別店也在用」是預期行為 |
| 刪除 ShopImage row 後 webp 檔成孤兒檔 | 接受；日後排程清 |
| 改綁不動檔案，path 與 drink_item_id 沒關聯，看 path 看不出綁誰 | DB 才是 source of truth；維運查詢一律走 DB 而非 path |
| 一張圖 width/height 等 metadata 跟 DB 上的不一致（萬一 image-upload 改邏輯） | 接受；冗餘存 metadata 是 snapshot 性質，當下正確即可 |

## Migration Plan

1. 新建 ShopImage Entity + migration `AddShopImage`
2. 跑 Migrator
3. 實作 service / controller
4. `pnpm generate`
5. Admin frontend 整合 — pool 頁 + 品項區塊 + 元件
6. 端到端驗收

## Open Questions

無。

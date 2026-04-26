# Design: refactor-image-upload-pipeline

## Context

現行 Upload.API 是「最小可運作版本」：API Key 鑑權、IFormFile 落地、靜態檔案服務。隨著 shop 圖片功能、未來 SaaS 化、與 Linode + Cloudflare CDN 規劃，三項先天不足會放大成痛點：

1. **沒有去重** — 同樣的店家品項照片可能被不同管理員上傳多次，浪費磁碟與 CDN 流量
2. **EXIF 隱私** — 手機拍的圖直接夾帶 GPS / 拍攝時間 / 裝置型號，使用者裝置資訊外流
3. **檔名是 GUID 不是內容 hash** — CDN 無法用 `immutable` cache，URL 變動風險高

`IFileStorageService` 介面已存在，但目前只有 `LocalFileStorage` 實作，未來換 R2/S3 沒有準備好的接縫測試。

Frontend 目前還沒實際對接這個服務（grep 不到 vue 用法），重構成本極低，是改架構的最佳時機。

## Goals / Non-Goals

**Goals:**

- 內容相同的圖只存一份（content-addressed by SHA-256）
- 上傳後一律輸出 WebP，路徑與副檔名固定，DB 存相對路徑
- EXIF metadata 100% 清除
- 統一格式、單一 quality 設定，降低未來除錯複雜度
- 對 CDN 友善：`Cache-Control: immutable`、`ETag`、URL 內容變動 hash 自動變動
- Asset host 從 config 動態提供，切換 CDN 不動程式碼
- 為 Linode + Cloudflare Free Plan CDN 鋪路（Phase 2 文件記載）

**Non-Goals:**

- 多尺寸處理（thumb / medium / original）— 交給 Cloudflare Image Resizing，本服務只存原圖
- 批次/分塊/續傳上傳 — 永遠單檔 POST，前端用佇列處理多檔
- 防盜連、配額、rate limit
- 排程清理孤兒檔（DB 沒參照的檔）— 未來另開 change
- 雲端儲存實作（R2 / S3 / MinIO）— 介面留好，實作未來再做
- 舊上傳檔案搬遷 — 直接清空（無前端引用）

## Decisions

### D1. 影像處理選 SkiaSharp，非 ImageSharp

**選擇：** `SkiaSharp` + `SkiaSharp.NativeAssets.Linux`

**Why:**
- License 乾淨（MIT + BSD），未來 SaaS 商用無授權成本
- WebP 編碼器是 Google 同一套，品質與檔案大小最佳化
- GIF 多幀解碼穩定，抓第一幀容易
- 4K 縮圖 + 編碼速度比 ImageSharp 快約 1.5–3x

**Alternatives:**
- `ImageSharp`：純 .NET 部署最省心，但 v2 起 Six Labors Split License 商用要付費
- `Magick.NET`：功能強但體積大、原生相依重
- `System.Drawing.Common`：Linux 不支援已棄用

**Cost:** 部署需確保 Docker base image 含正確 native libs（`libSkiaSharp.so`），這個一次性配置即可。

### D2. 全部編碼為 WebP，丟棄原始格式

**選擇：** 不論輸入是 jpg/png/gif/webp，輸出固定 `.webp`，quality=85

**Why:**
- DB 與檔案路徑統一單一副檔名 → 邏輯最簡單
- WebP 比 JPEG 小 25–35%、比 PNG 小 50%+
- 全球瀏覽器支援率 97%+，IE11 已死、Safari 14+（2020 年起）支援
- Linode 磁碟 + CDN 回源頻寬都省

**Alternatives:**
- 保留原格式：實作分支多、儲存成本高，無實際好處
- 全部轉 AVIF：壓縮率更好但編碼慢 5–10x、瀏覽器支援度仍不及 WebP
- 同時存 webp + 原檔：浪費空間，去重邏輯破功

**Trade-off:** 動畫 GIF 會變成靜態第一幀。對「店家品項照片」場景無妨；若未來有需要動圖的場景，再開 change 處理。

### D3. Quality = 85（不可調）

**選擇：** WebP encoder quality 寫死 85

**Why:**
- 業界共識：80–90 為「肉眼難辨損失」甜蜜點
- 85 是 Google `cwebp` 預設、Cloudflare Polish 預設
- 寫死降低決策成本與測試矩陣

**Alternatives:**
- 動態參數：使用者/呼叫端決定 — 增加 API surface 與測試複雜度，無實際好處
- Lossless：檔案大很多，對照片場景沒意義（PNG 來源還是 lossy 損失，因為 PNG 像素已無 metadata 來重建）

### D4. 路徑採 content-addressed: `images/{hash[0:6]}/{hash}.webp`

**選擇：** SHA-256(處理後 webp bytes) 取前 6 字元當分桶目錄，完整 hash 當檔名

**Why:**
- 內容變了 hash 必變 → URL 變 → CDN 自動失效，可設 `immutable` 永久快取
- 前 6 字元 = 16^6 ≈ 1600 萬桶，避免單一目錄過多檔案影響檔案系統效能
- 同樣 hash 撞到 = 內容真的相同 = 直接回原 path（去重）
- 不需要 DB lookup 即可知道檔案位置

**Alternatives:**
- GUID 檔名：無法去重、CDN 無法 `immutable`
- Hash 全長當目錄結構（`a/b/c/...`）：路徑太深，沒額外好處
- 前 2 字元分桶：256 個桶，shop 圖規模下單桶可能塞太多檔

**Hash collision risk:** SHA-256 全寫 64 字元，碰撞機率天文等級低；現實中不會發生。

### D5. SHA-256 是「處理後」bytes，不是上傳原檔

**選擇：** 算 hash 在 Skia 解碼+縮圖+轉 WebP 之後

**Why:**
- 同一張原圖，不同 EXIF / 不同上傳 client（手機 vs 桌機壓縮）→ bytes 不同 → 原檔 hash 不同
- 處理後 bytes 才是「真正存進服務的內容」，hash 應該對應該內容
- 處理是 deterministic（同樣輸入 + 同樣 Skia 版本 + 同樣 quality → 同樣輸出 bytes）→ 可重現去重

**Trade-off:** 必須先處理才能算 hash，不能用「先 hash 看有沒有重複再決定要不要處理」省 CPU。可接受 — 上傳量目前不大，圖片處理本來就不是熱點。

### D6. 重複時直接回原 path（不報錯、不覆寫、不引用計數）

**選擇：** Hash 已存在 → server 不再寫檔，直接回該 path 給呼叫端

**Why:**
- 去重最自然的語意
- 「不刪檔」（你已決定）→ 引用計數的價值消失（反正不會真刪）
- 兩個呼叫端綁同一個 path 不是問題，反正內容相同

**Risk:** 萬一未來想做「刪除這張圖」就會有風險（其他人可能還在用）。但 proposal 已明確「不做刪除」，未來真要做時再加引用計數，現在 over-engineering。

### D7. 上限 10MB（原檔），長邊 > 4000 等比縮到 4000

**選擇：** 收檔時拒絕 > 10MB；解碼後若 max(width, height) > 4000 則 resize

**Why:**
- 10MB 對大多數現代手機照片（4–8MB）夠用
- 縮到 4000 之後，CDN + Cloudflare Image Resizing 動態縮成 thumb 綽綽有餘
- 限制 server 端記憶體峰值（Skia decode 一張 8K × 8K JPEG 可能吃 200MB+ RAM）

**Alternatives:**
- 不縮，全部存原圖：使用者上傳 12000×8000 的圖會把磁碟快速吃光、CDN 回源慢
- 縮到 2000：CDN resize 成 1600 大圖會看到模糊
- 4096：選 4000 好記，差 96px 沒差

### D8. asset-host 由 Admin.API / User.API 各自提供，不打 Upload.API

**選擇：** `GET /api/admin/upload/asset-host`、`GET /api/user/upload/asset-host` 各自從 appsettings 讀 `Upload.PublicBaseUrl` 後回傳

**Why:**
- 前端只跟自己的 API 對話 — 架構乾淨
- Upload.API 不變成前端常駐相依（萬一 Upload.API 某天只開內部 IP，不對外，這層 indirection 救命）
- 各 API 可以有不同的 PublicBaseUrl（萬一未來要做 admin / client 分流到不同 CDN）

**Trade-off:** 一份 config 要設兩處（Admin / User）。但 dev/prod 切換時本來就要動 config，沒額外麻煩。

### D9. 端點維持 `[Authorize]`

**選擇：** asset-host endpoint 走認證

**Why:**
- 結果只是個 URL prefix，但既然有認證牆就放裡面，沒必要破例
- 前端拿 token 後再撈 → 跟其他 endpoint 一致

### D10. 前端佇列上傳（不切片、不 resumable）

**選擇：** Server 永遠單檔 POST；前端 composable `useImageUploadQueue` 管理 File[]

**Why:**
- 圖片 ≤ 10MB，重傳成本可接受（4G 約 5–15 秒）
- 切片只解決「大檔斷線續傳」與「繞過代理大小限制」，10MB 兩者皆不適用
- Server 程式單純、無 session 管理
- 失敗只影響當前那張

**Frontend pattern:**
```typescript
function useImageUploadQueue(options?: { concurrency?: number })
  → uploads: Ref<UploadItem[]>
  → progress: ComputedRef<{ done, total, failed }>
  → enqueue(files: File[])
  → retry(item: UploadItem)
```
預設 `concurrency = 1`（一張一張慢慢傳）。

### D11. CDN-friendly headers

**選擇：** `/assets/*` 的 response 一律加：

```
Cache-Control: public, max-age=31536000, immutable
ETag: "{hash}"
```

**Why:**
- 不加的話 CDN 預設只快取幾分鐘，hash 路徑的設計就浪費了
- `immutable` 告訴瀏覽器「這個 URL 永遠不會變內容」，連 304 都省
- ETag 用 hash 作為 strong validator

**Implementation:** 用 ASP.NET Core `StaticFileOptions.OnPrepareResponse` callback 設 header。

### D12. CORS 維持公開讀取

**選擇：** `/assets/*` 允許 `localhost:8081`、`localhost:8082` 與 production frontend domain（從 config 讀）

**Why:**
- 前後台都直連 Upload.API 抓圖
- 上 CDN 後 CORS 設定一樣有效（CF 透傳 Origin header）

## Risks / Trade-offs

| Risk | Mitigation |
|---|---|
| Skia native libs 在不同 OS / arch 部署失敗 | Docker image 統一 base（Linode 上跑 linux-x64），CI 跑一次驗證 startup |
| 動畫 GIF 變靜態（resolution loss） | proposal 已明說此 trade-off；場景是飲料店菜單圖，無動圖需求 |
| 上傳完才能算 hash → 流式去重做不到 | 接受。上傳量小，CPU 不是瓶頸 |
| 同 hash 但實際是不同來源（極小機率） | SHA-256 碰撞天文機率，工程上忽略 |
| 重編碼 GIF/PNG 失敗（檔案損壞） | Skia decode 失敗 → 回 400 + `INVALID_IMAGE`；不嘗試「修圖」 |
| 4000×4000 + WebP encode 記憶體峰值 | 限制 10MB 原檔 + Skia stream decode；單請求峰值約 100–200MB，多 concurrent 上傳要監控 |
| 前端忘了拼 asset-host 直接用 path | path 開頭固定 `/assets/`，dev 環境跟同 origin 也能 work；上 prod 才會錯，會被 e2e 抓到 |
| 既有 Uploads/ 目錄殘留檔 | 直接清空（測試殘留），新 pipeline 從零開始 |

## Migration Plan

### Phase 1: 程式重構（this change）

```
1. 引入 SkiaSharp 套件
2. 重寫 FileUploadService：magic bytes → Skia → WebP → SHA-256 → dedup
3. Upload.API：移除 category 參數、改 content-addressed 路徑、加 cache headers
4. Admin.API / User.API UploadController：移除 category、加 asset-host endpoint
5. appsettings：新增 Upload.PublicBaseUrl
6. 清空 api/Upload.API/Uploads/，rebuild empty
7. pnpm generate 更新 api-types
8. 前端 composable useImageUploadQueue
9. dotnet build + admin/client lint/build
```

### Phase 2: Cloudflare Free Plan CDN（未來，文件記載）

```
前置：
  - 把 domain 移到 Cloudflare（NS 換 ns1/ns2.cloudflare.com）
  - Linode 上裝 Caddy（自動 Let's Encrypt HTTPS）
  - 確認 Upload.API 透過 Caddy 對 https 服務

DNS:
  - cdn.example.com  A  {Linode IP}  Proxied(orange cloud) ON

CF 後台設定:
  1. SSL/TLS mode: Full (strict)
  2. Caching → Cache Rules:
     Name:   Cache image assets
     When:   URI Path contains "/assets/images/"
     Then:   Cache eligibility: Eligible
             Edge TTL: 1 year (override origin)
             Browser TTL: Respect origin
     ※ 用 Cache Rule 強制覆蓋 .webp（CF Free 預設 .webp 不一定快取）
  3. Speed → Optimization → Brotli: ON
  4. Network → HTTP/3: ON

Linode 防火牆:
  - 80/443 入站只允許 Cloudflare IP range
    （CF 公開列表 https://www.cloudflare.com/ips/）
  - 防止使用者繞過 CDN 直接 hammer origin

切換 config（無 code change）:
  - Admin.API/appsettings.json   Upload.PublicBaseUrl=https://cdn.example.com/assets
  - User.API/appsettings.json    Upload.PublicBaseUrl=https://cdn.example.com/assets
  - Upload.API/appsettings.json  Cors.AllowedOrigins 加入 production frontend domain
  - 重啟服務，前端打 asset-host 拿到新 URL，自動切過去

驗證:
  - 第一次請求應 cf-cache-status: MISS
  - 第二次請求應 cf-cache-status: HIT
  - 開瀏覽器 DevTools 看 response header 有 cf-cache-status / age 欄位
```

### Rollback

- Phase 1: 還原 git commit、跑 dotnet build。Uploads/ 已清空，無資料遺失（圖片本來就會用新流程重新上傳）
- Phase 2: CF 後台關掉 proxy（橘雲變灰雲）→ 流量直回 Linode。config 改回 `localhost:5103/assets` 或 `upload.example.com/assets`

## Open Questions

無。決策都已經敲定，可進入 specs 與 tasks 階段。

## 未來鋪路（不在本 change 範圍）

- **R2 / S3 切換**：`IFileStorageService` 介面已存在，未來只需新增 `R2FileStorage : IFileStorageService` 並透過 DI 切換。`AddFileUpload` 擴充方法可加開關。
- **Cloudflare Image Resizing**：上 CF Pro 後 URL 改成 `cdn.example.com/cdn-cgi/image/width=400,quality=80/images/ab/abc.webp` 即可動態縮圖、轉 AVIF。本服務存的 4000px 原圖完美 cover 此需求。
- **多尺寸 fallback**：若不上 CF Pro，將來可加一個 `/api/upload/images/{hash}/{size}` endpoint 動態產生（lazy + cache）。
- **病毒掃描 / NSFW 偵測**：可加進 pipeline 第 4 步（Skia decode 後）。
- **引用計數 / 軟刪除**：若要做檔案刪除，需引入 `image_references` 表記錄被誰使用，計數歸零才真刪。

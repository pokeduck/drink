# Upload.API/

獨立的檔案上傳服務（port `5103`）。Admin.API / User.API 透過 HttpClient + `X-Api-Key` 內部 proxy 寫入；前端則直接以 `/assets/...` 讀檔（CORS 允許）。

## 啟動 / 指令

```bash
# 開發啟動（預設 Development，啟用 Swagger）
dotnet run --project api/Upload.API

# Swagger UI
http://localhost:5103/swagger
```

- 寫入端點：`POST /api/files`，需帶 header `X-Api-Key`（由 `ApiKeyMiddleware` 驗證，跳過 `/assets`）
- 讀取端點：`GET /assets/{path}` → 對應 `../../upload/` 落地檔案
- 路由 / Query / JSON 命名規則與其他 API 一致（kebab-case route、snake_case query、snake_case JSON）

## 主要檔案 / 子層

| 路徑 | 角色 |
|------|------|
| `Program.cs` | DI、CORS（`Cors:AllowedOrigins` 白名單）、靜態檔案、`ApiKeyMiddleware` pipeline |
| `Controllers/BaseController.cs` | 簡化版 base（不含 JWT；只負責 `ApiOk()` / `ApiError()`） |
| `Controllers/FilesController.cs` | 圖片上傳：3 層驗證 → SkiaSharp decode → 縮放 → webp → SHA-256 去重 |
| `Middleware/ApiKeyMiddleware.cs` | 驗 `X-Api-Key`，`/assets` 路徑直接放行 |
| `Uploads/` | 預設落地路徑（實際目錄為專案根的 `../../upload/`，依 `appsettings.json` 設定） |
| `appsettings*.json` | `ApiKey`、`UploadSettings`、`Cors:AllowedOrigins` |

## 3 層驗證

| 順序 | 檢查 |
|------|------|
| 1 | 副檔名白名單（`.jpg` / `.jpeg` / `.png` / `.webp`） |
| 2 | MIME type（`Content-Type`） |
| 3 | Magic bytes（檔案頭實際 binary signature） |

## 依賴與被依賴

- 依賴：`../Application/`（只用 `FileUploadService` 與 `ICurrentUserContext`）、`../Infrastructure/`（`HttpCurrentUserContext` + `AddFileUpload`）、`../Shared.Web/`
- 被依賴：`Admin.API` / `User.API` 透過 HttpClient proxy 呼叫
- **不**註冊 JWT、**不**註冊 `RoleMiddleware`、**不**自動掃 `BaseService`（只手動註冊 `FileUploadService`）

## 不要做的事

- 不要在這層加業務邏輯（會員、訂單、店家）— 只處理檔案儲存 + 3 層驗證
- 不要 skip 3 層驗證直接落地檔案
- 不要把 `X-Api-Key` 寫死在程式碼，必須讀 `appsettings.json` 的 `ApiKey`
- 不要允許 `/assets` 以外的 endpoint 不帶 API Key 通過 — 新加路由要確認 `ApiKeyMiddleware` 是否會擋
- 不要從前端直接 POST 上傳到這裡，前端只能透過 Admin/User API proxy；讀取才走 `/assets/...`
- 不要把 Upload 落地路徑寫死，使用 `UploadSettings` 配置

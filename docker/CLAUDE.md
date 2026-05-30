# docker/

本地開發環境支援檔。目前**只**用來跑 PostgreSQL，.NET API 與 Nuxt 前端皆在本機直接啟動，不進容器。

## 啟動 / 停止（在專案根目錄執行）

```bash
docker compose up -d      # 啟動 postgres（背景）
docker compose down       # 停止並移除容器（資料保留於 named volume）
docker compose logs -f    # 觀察 log
```

## docker-compose.yml 摘要

- service：`postgres`（image `postgres:17`，container name `drink-postgres`）
- port：`5432:5432`（host:container）
- 預設帳密：`postgres` / `yourpassword`（僅 local，不要用於正式環境）
- data volume：named volume `postgres_data` → `/var/lib/postgresql/data`
- init script：`./docker/postgres/init/` 掛載到 `/docker-entrypoint-initdb.d/`
  - 容器**首次**建立 volume 時才會執行；之後不再跑

## 內容物

```
docker/
└── postgres/
    └── init/
        └── 01-create-db.sql   # 建立 application 用 database / role
```

修改 init 腳本後，必須先 `docker compose down -v` 刪掉 volume 才會重跑（會清光資料）。

## 與其他層的關係

- API 連線字串走 `appsettings.Development.json`，host 用 `localhost:5432`
- Migration / Seeder 由 `api/Migrator` 套用，**不**透過 init script
- Upload.API 寫入的檔案落在 `../upload/`，與容器無關

## 不要做的事

- 不要把 .NET API 包進這份 compose；本機 `dotnet run` 即可
- 不要在 init 腳本內塞 schema 或 seed data（那是 EF Core migration / Seeder 的職責）
- 不要把 `yourpassword` 之類的測試密碼搬到 production

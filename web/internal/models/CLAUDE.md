# internal/models

`@app/models`：手寫共用型別、enum、商業邏輯常數。提供「跨 app 共用、且不適合靠 codegen 維護」的型別。

## 啟動 / 指令

此套件無 build / dev / test script，作為 source-only TS package 由其他套件直接 import 消費。

修改後 admin / client 透過 workspace 連結即時生效。

## 主要檔案 / 子層

```
internal/models/
├── package.json
└── src/
    └── index.ts    # 全部型別 / enum 統一從這裡 export
```

`main` / `types`：皆指向 `./src/index.ts`。新增型別請集中放 `src/index.ts` 或拆檔後從 `index.ts` re-export。

### 預期內容範圍

- 業務 enum（例：`DrinkStatus`、`OrderStatus`、`NotificationType` 等）
- 跨 app 共用的 UI 無關型別（例：通用回應結構、表單狀態 enum）
- 與後端 Entity 對應、但需在前端額外 mapping 的型別

## 與其他套件的關係

依賴：
- `@app/tsconfig`（dev）

被依賴：
- `@drink/admin`
- `@drink/client`
- `@app/core`

## 與 `@app/api-types` 的差別（關鍵）

| 維度 | `@app/api-types` | `@app/models` |
|------|-----------------|---------------|
| 來源 | 後端 Swagger codegen | 手寫 |
| 工具 | `openapi-typescript` | TS 本身 |
| 內容 | API 請求 / 回應 schema、path、operation | 業務 enum、跨 app 共用型別 |
| 是否手改 | 不可（會被覆蓋） | 是（唯一來源） |
| 觸發更新 | 後端 Swagger 變動 → `pnpm generate` | 業務邏輯變動 → 手動編輯 |

兩者**互不取代**：API 介面型別走 `api-types`，業務 enum / 共用型別走 `models`。

## 不要做的事

- 不要把 `api-types` 產出的型別 re-export 出來（兩者來源不同）
- 不要把 UI 框架相關型別（Element Plus props、Tailwind class 名）放進來
- 不要 import `vue` / `nuxt` runtime，models 必須能在純 TS 環境消費
- 不要在 models 內寫業務邏輯函式，純型別 / enum / 常數即可（函式類放 `@app/core`）
- 不要 export default，全部 named export

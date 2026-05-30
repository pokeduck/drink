# internal/core

`@app/core`：admin 與 client 兩支 Nuxt app 共用的核心工具層。放置與 UI 框架無關的 fetch wrapper、auth helper、token 管理、共用常數。

## 啟動 / 指令

此套件無 build / dev / test script，作為 source-only TS package 由其他套件直接 import 消費。

修改後不需重新 build，admin / client 透過 workspace 連結即時生效（Nuxt dev server 會自動 reload）。

## 主要檔案 / 子層

```
internal/core/
├── package.json
└── src/
    ├── index.ts                       # 統一 re-export
    └── constants/
        ├── errorCodes.ts              # 後端 4XXYY 錯誤碼對應表
        └── menuConstants.ts           # AdminMenu route / icon 常數
```

`main`：`./src/index.ts`（CJS 形式 entry）。新增模組請從 `src/index.ts` 統一 re-export。

### 預期內容範圍

- fetch wrapper（基於 `openapi-fetch`，整合 token 注入 / refresh 流程）
- auth helper：access token / refresh token 存取、登入登出
- token rotation 與 401 處理策略
- 共用常數：error code、menu route、業務 enum 對應
- 純函式 utility（不可依賴 Vue / Nuxt runtime）

## 與其他套件的關係

依賴：
- `@app/models`（共用 enum / 型別）
- `dayjs`

被依賴：
- `@drink/admin`
- `@drink/client`

> 同時被兩個設計系統不同的 app 引用，因此**必須保持框架無關**：不能 import Element Plus、Tailwind class、`lucide-vue-next`、`vue` runtime API。

## 不要做的事

- 不要在 core 內 import `element-plus` / `lucide-vue-next` / 任何 UI 套件
- 不要 import Vue 的 `ref` / `reactive` 等 reactivity runtime（純函式即可；如果真的要 reactive helper 改放各 app 的 composables）
- 不要直接 import `@app/api-types`（core 應與後端型別解耦；需要時由呼叫端傳入 generic type）
- 不要在 core 寫死 API base URL，由呼叫端（admin / client 的 `runtimeConfig`）注入
- 不要 export default，所有公開介面走 named export 並從 `src/index.ts` 統一聚合
- 不要新增依賴 Node runtime 的套件（fs / path 等），core 必須能跑在瀏覽器

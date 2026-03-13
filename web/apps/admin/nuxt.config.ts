import { createResolver } from "@nuxt/kit";

const { resolve } = createResolver(import.meta.url);

export default defineNuxtConfig({
  // 1. 讓 TypeScript 知道如何處理你的共享包
  typescript: {
    // 這裡相當於在 tsconfig.json 裡寫內容
    tsConfig: {
      extends: "../../internal/tsconfig/base.json", // 繼承你的基礎設定 (方法 4)
      compilerOptions: {
        paths: {
          "@app/models": ["../../internal/models/src"],
          "@app/core": ["../../internal/core/src"],
        },
      },
    },
  },

  // 2. 讓 Vite/Nitro 執行時能找到檔案 (這是給運行時用的)
  alias: {
    "@app/models": "../../internal/models/src",
    "@app/core": "../../internal/core/src",
  },

  // 3. 務必讓 Nuxt 轉譯這些外部包，否則遇到 .ts 檔案可能會報錯
  build: {
    transpile: ["@app/models", "@app/core"],
  },
  devServer: {
    port: 3002,
  },
});

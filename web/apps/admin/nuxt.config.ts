import { createResolver } from "@nuxt/kit";

const { resolve } = createResolver(import.meta.url);

export default defineNuxtConfig({
  modules: ["@element-plus/nuxt", "@pinia/nuxt"],

  typescript: {
    tsConfig: {
      extends: resolve("../../internal/tsconfig/base.json"),
      compilerOptions: {
        paths: {
          "@app/models": [resolve("../../internal/models/src")],
          "@app/core": [resolve("../../internal/core/src")],
        },
      },
    },
  },

  alias: {
    "@app/models": resolve("../../internal/models/src"),
    "@app/core": resolve("../../internal/core/src"),
  },

  build: {
    transpile: ["@app/models", "@app/core"],
  },
  devServer: {
    port: 8081,
  },

  runtimeConfig: {
    public: {
      apiBase: "http://localhost:5101/api",
    },
  },
});

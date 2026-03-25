import { createResolver } from '@nuxt/kit'

const { resolve } = createResolver(import.meta.url)

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/ui'
  ],

  devtools: {
    enabled: true
  },

  css: ['~/assets/css/main.css'],

  ssr: false,

  compatibilityDate: '2025-01-15',

  typescript: {
    tsConfig: {
      extends: resolve('../../internal/tsconfig/base.json'),
      compilerOptions: {
        paths: {
          '@app/models': [resolve('../../internal/models/src')],
          '@app/core': [resolve('../../internal/core/src')]
        }
      }
    }
  },

  alias: {
    '@app/models': resolve('../../internal/models/src'),
    '@app/core': resolve('../../internal/core/src')
  },

  build: {
    transpile: ['@app/models', '@app/core']
  },

  eslint: {
    config: {
      stylistic: {
        commaDangle: 'never',
        braceStyle: '1tbs'
      }
    }
  }
})

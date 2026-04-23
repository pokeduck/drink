import { createResolver } from '@nuxt/kit'
import tailwindcss from '@tailwindcss/vite'

const { resolve } = createResolver(import.meta.url)

// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxtjs/color-mode',
    '@nuxtjs/google-fonts'
  ],

  devtools: {
    enabled: true
  },

  css: ['~/assets/css/main.css'],

  vite: {
    plugins: [
      tailwindcss()
    ]
  },

  ssr: false,

  compatibilityDate: '2025-01-15',

  colorMode: {
    classSuffix: '',
    preference: 'system',
    fallback: 'light'
  },

  googleFonts: {
    families: {
      Inter: [400, 500, 600, 700],
      'Space Grotesk': [700, 800, 900]
    },
    display: 'swap'
  },

  app: {
    pageTransition: { name: 'page', mode: 'out-in' }
  },

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

  devServer: {
    port: 8082
  },

  runtimeConfig: {
    public: {
      apiBase: 'http://localhost:5102/api'
    }
  }
})

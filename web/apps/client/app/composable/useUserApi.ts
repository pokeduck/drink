import createClient from 'openapi-fetch'
import type { paths } from '@app/api-types/user'

let client: ReturnType<typeof createClient<paths>> | null = null

export const useUserApi = () => {
  if (client) return client

  const config = useRuntimeConfig()
  // swagger paths 已包含 /api 前綴，baseUrl 不需要帶 /api
  const baseUrl = (config.public.apiBase as string).replace(/\/api\/?$/, '')

  client = createClient<paths>({ baseUrl })

  // TODO: 前台 auth middleware（待前台會員認證功能實作時加入）

  // Error middleware — 非 401 錯誤 toast 通知
  client.use({
    async onResponse({ response }) {
      if (response.ok || response.status === 401) return

      let msg = '系統發生錯誤，請稍後再試'
      try {
        const body = await response.clone().json()
        if (body?.message) msg = body.message
      } catch {
        // 無法解析 body，使用預設訊息
      }

      // TODO: 前台改用 Nuxt UI 的 toast
      console.error('[API Error]', msg)
    },

    async onError({ error }) {
      const isNetworkError = error instanceof TypeError && (error as TypeError).message === 'Failed to fetch'
      console.error(isNetworkError ? '網路連線異常' : '系統發生錯誤')
    },
  })

  return client
}

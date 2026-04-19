import createClient from 'openapi-fetch'
import type { paths } from '@app/api-types/admin'
import { useAuthStore } from '~/stores/auth'

let client: ReturnType<typeof createClient<paths>> | null = null

export const useAdminApi = () => {
  if (client) return client

  const config = useRuntimeConfig()
  // swagger paths 已包含 /api 前綴，baseUrl 不需要帶 /api
  const baseUrl = (config.public.apiBase as string).replace(/\/api\/?$/, '')

  client = createClient<paths>({ baseUrl })

  const authStore = useAuthStore()
  let isRefreshing = false
  let refreshPromise: Promise<boolean> | null = null

  // Auth middleware — token 注入 + 401 refresh
  client.use({
    async onRequest({ request }) {
      if (authStore.accessToken) {
        request.headers.set('Authorization', `Bearer ${authStore.accessToken}`)
      }
    },

    async onResponse({ response, request: req }) {
      if (response.status !== 401) return

      // 401 → 嘗試 refresh
      if (authStore.refreshToken) {
        if (!isRefreshing) {
          isRefreshing = true
          refreshPromise = authStore.refresh()
        }

        const success = await refreshPromise
        isRefreshing = false
        refreshPromise = null

        if (success) {
          // 用新 token 重試原始請求
          const retryReq = new Request(req, {
            headers: new Headers(req.headers),
          })
          retryReq.headers.set('Authorization', `Bearer ${authStore.accessToken}`)
          return await fetch(retryReq)
        }
      }

      // refresh 失敗或無 refresh token → 重導登入
      authStore.clearTokens()
      await navigateTo('/login')
    },
  })

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

      ElMessage.error(msg)
    },

    async onError({ error }) {
      // 網路斷線或其他 fetch 層級錯誤
      const isNetworkError = error instanceof TypeError && (error as TypeError).message === 'Failed to fetch'
      ElMessage.error(isNetworkError ? '網路連線異常，請檢查網路狀態' : '系統發生錯誤，請稍後再試')
    },
  })

  return client
}

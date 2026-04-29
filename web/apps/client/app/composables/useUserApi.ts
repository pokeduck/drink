import createClient from 'openapi-fetch'
import type { paths } from '@app/api-types/user'
import { useAuthStore } from '~/stores/auth'

let client: ReturnType<typeof createClient<paths>> | null = null

export const useUserApi = () => {
  if (client) return client

  const config = useRuntimeConfig()
  const baseUrl = (config.public.apiBase as string).replace(/\/api\/?$/, '')

  client = createClient<paths>({ baseUrl })

  const authStore = useAuthStore()
  let isRefreshing = false
  let refreshPromise: Promise<boolean> | null = null

  client.use({
    async onRequest({ request }) {
      if (authStore.accessToken) {
        request.headers.set('Authorization', `Bearer ${authStore.accessToken}`)
      }
    },
    async onResponse({ response, request: req }) {
      if (response.status !== 401) return

      if (authStore.refreshToken) {
        if (!isRefreshing) {
          isRefreshing = true
          refreshPromise = authStore.refresh()
        }
        const ok = await refreshPromise
        isRefreshing = false
        refreshPromise = null

        if (ok) {
          const retryReq = new Request(req, { headers: new Headers(req.headers) })
          retryReq.headers.set('Authorization', `Bearer ${authStore.accessToken}`)
          return await fetch(retryReq)
        }
      }

      authStore.clearTokens()
      await navigateTo('/login')
    }
  })

  return client
}

import { useAuthStore } from '~/stores/auth'

// 全域 API pending 計數器
const pendingCount = ref(0)
export const useApiPending = () => computed(() => pendingCount.value > 0)

export const useApi = () => {
  const config = useRuntimeConfig()
  const authStore = useAuthStore()

  let isRefreshing = false
  let refreshPromise: Promise<boolean> | null = null

  const apiFetch = $fetch.create({
    baseURL: config.public.apiBase,

    async onRequest({ options }) {
      const headers = new Headers(options.headers)

      if (authStore.accessToken) {
        headers.set('Authorization', `Bearer ${authStore.accessToken}`)
      }

      options.headers = headers
    },

    async onResponseError({ response, request, options }) {
      // 401 → 嘗試 refresh token 後重試一次
      if (response.status === 401 && authStore.refreshToken) {
        if (!isRefreshing) {
          isRefreshing = true
          refreshPromise = authStore.refresh()
        }

        const success = await refreshPromise
        isRefreshing = false
        refreshPromise = null

        if (success) {
          // 重試原始請求
          const headers = new Headers(options.headers)
          headers.set('Authorization', `Bearer ${authStore.accessToken}`)
          return $fetch(request, { ...options, headers })
        }

        // refresh 失敗，導向登入
        authStore.clearTokens()
        await navigateTo('/login')
        return
      }

      // 其他 401 (無 refresh token)
      if (response.status === 401) {
        authStore.clearTokens()
        await navigateTo('/login')
        return
      }

      // 非 401 錯誤
      const msg = response._data?.message || response._data?.error || '未知錯誤'
      console.error('API Error:', msg)
    },
  })

  const tracked = <T>(promise: Promise<T>): Promise<T> => {
    pendingCount.value++
    return promise.finally(() => {
      pendingCount.value--
    })
  }

  return {
    get: <T>(url: string, opts?: any) => tracked(apiFetch<T>(url, { method: 'GET', ...opts })),
    post: <T>(url: string, body?: any, opts?: any) => tracked(apiFetch<T>(url, { method: 'POST', body, ...opts })),
    put: <T>(url: string, body?: any, opts?: any) => tracked(apiFetch<T>(url, { method: 'PUT', body, ...opts })),
    delete: <T>(url: string, opts?: any) => tracked(apiFetch<T>(url, { method: 'DELETE', ...opts })),
  }
}

import { defineStore } from 'pinia'

interface LoginPayload {
  username: string
  password: string
}

interface AuthTokens {
  access_token: string
  refresh_token: string
}

interface ApiResponse<T> {
  data: T
  code: number
  error?: string
  message?: string
}

export const useAuthStore = defineStore('auth', () => {
  const config = useRuntimeConfig()
  const accessToken = useCookie('auth_token')
  const refreshToken = useCookie('refresh_token')

  const isLoggedIn = computed(() => !!accessToken.value)

  async function login(payload: LoginPayload) {
    try {
      const res = await $fetch<ApiResponse<AuthTokens>>('/admin/auth/login', {
        baseURL: config.public.apiBase,
        method: 'POST',
        body: payload,
      })

      if (res.code !== 0) {
        throw new Error(res.message || '登入失敗')
      }

      accessToken.value = res.data.access_token
      refreshToken.value = res.data.refresh_token
    } catch (err: any) {
      // $fetch 在非 2xx 時拋 FetchError，從 response body 取 message
      const msg = err?.data?.message || err?.message || '登入失敗'
      throw new Error(msg)
    }
  }

  async function refresh(): Promise<boolean> {
    if (!refreshToken.value) return false

    try {
      const res = await $fetch<ApiResponse<AuthTokens>>('/admin/auth/refresh', {
        baseURL: config.public.apiBase,
        method: 'POST',
        body: { refresh_token: refreshToken.value },
      })

      if (res.code !== 0) {
        clearTokens()
        return false
      }

      accessToken.value = res.data.access_token
      refreshToken.value = res.data.refresh_token
      return true
    } catch {
      clearTokens()
      return false
    }
  }

  async function logout() {
    if (refreshToken.value) {
      try {
        await $fetch('/admin/auth/logout', {
          baseURL: config.public.apiBase,
          method: 'POST',
          headers: { Authorization: `Bearer ${accessToken.value}` },
          body: { refresh_token: refreshToken.value },
        })
      } catch {
        // ignore logout errors
      }
    }
    clearTokens()
  }

  function clearTokens() {
    accessToken.value = null
    refreshToken.value = null
  }

  return {
    accessToken,
    refreshToken,
    isLoggedIn,
    login,
    refresh,
    logout,
    clearTokens,
  }
})

import { defineStore } from 'pinia'
import createClient from 'openapi-fetch'
import type { paths } from '@app/api-types/admin'

export const useAuthStore = defineStore('auth', () => {
  const config = useRuntimeConfig()
  const accessToken = useCookie('auth_token')
  const refreshToken = useCookie('refresh_token')

  const isLoggedIn = computed(() => !!accessToken.value)

  // 不帶 auth middleware 的 client，專給 login/refresh/logout 用
  const baseUrl = (config.public.apiBase as string).replace(/\/api\/?$/, '')
  const authClient = createClient<paths>({ baseUrl })

  async function login(payload: { username: string; password: string }) {
    const { data: res, error } = await authClient.POST('/api/admin/auth/login', {
      body: payload,
    })

    if (error) {
      const msg = (error as any)?.message || '登入失敗'
      throw new Error(msg)
    }

    if (!res?.data) {
      throw new Error('登入失敗')
    }

    accessToken.value = res.data.access_token ?? null
    refreshToken.value = res.data.refresh_token ?? null
  }

  async function refresh(): Promise<boolean> {
    if (!refreshToken.value) return false

    try {
      const { data: res, error } = await authClient.POST('/api/admin/auth/refresh', {
        body: { refresh_token: refreshToken.value },
      })

      if (error || !res?.data) {
        clearTokens()
        return false
      }

      accessToken.value = res.data.access_token ?? null
      refreshToken.value = res.data.refresh_token ?? null
      return true
    } catch {
      clearTokens()
      return false
    }
  }

  async function logout() {
    if (refreshToken.value) {
      try {
        await authClient.POST('/api/admin/auth/logout', {
          body: { refresh_token: refreshToken.value },
          headers: { Authorization: `Bearer ${accessToken.value}` },
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

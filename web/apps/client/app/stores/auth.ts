import { defineStore } from 'pinia'
import createClient from 'openapi-fetch'
import type { paths, components } from '@app/api-types/user'

type UserProfileResponse = components['schemas']['UserProfileResponse']

export const useAuthStore = defineStore('auth', () => {
  const config = useRuntimeConfig()
  const accessToken = useCookie<string | null>('auth_token', { default: () => null })
  const refreshToken = useCookie<string | null>('refresh_token', { default: () => null })
  const currentUser = ref<UserProfileResponse | null>(null)

  const isLoggedIn = computed(() => !!accessToken.value)

  const baseUrl = (config.public.apiBase as string).replace(/\/api\/?$/, '')
  const authClient = createClient<paths>({ baseUrl })

  async function register(payload: { name: string; email: string; password: string }) {
    const { data: res, error } = await authClient.POST('/api/user/auth/register', { body: payload })
    if (error) throw error
    if (res?.code !== 0) throw res
  }

  async function verifyEmail(token: string) {
    const { data: res, error } = await authClient.POST('/api/user/auth/verify-email', { body: { token } })
    if (error) throw error
    if (res?.code !== 0) throw res
  }

  async function login(payload: { email: string; password: string }) {
    const { data: res, error } = await authClient.POST('/api/user/auth/login', { body: payload })
    if (error) throw error
    if (res?.code !== 0 || !res.data) throw res

    accessToken.value = res.data.access_token ?? null
    refreshToken.value = res.data.refresh_token ?? null
    await fetchProfile()
  }

  async function refresh(): Promise<boolean> {
    if (!refreshToken.value) return false
    try {
      const { data: res, error } = await authClient.POST('/api/user/auth/refresh', {
        body: { refresh_token: refreshToken.value }
      })
      if (error || res?.code !== 0 || !res.data) {
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
        await authClient.POST('/api/user/auth/logout', {
          body: { refresh_token: refreshToken.value },
          headers: { Authorization: `Bearer ${accessToken.value}` }
        })
      } catch {
        // ignore
      }
    }
    clearTokens()
  }

  async function changePassword(payload: { oldPassword: string; newPassword: string }) {
    const { data: res, error } = await authClient.PUT('/api/user/auth/password', {
      body: { old_password: payload.oldPassword, new_password: payload.newPassword },
      headers: { Authorization: `Bearer ${accessToken.value}` }
    })
    if (error) throw error
    if (res?.code !== 0) throw res
  }

  async function logoutAll() {
    try {
      await authClient.POST('/api/user/auth/logout-all', {
        headers: { Authorization: `Bearer ${accessToken.value}` }
      })
    } catch {
      // ignore
    }
    clearTokens()
  }

  async function fetchProfile(): Promise<void> {
    if (!accessToken.value) return
    const { data: res } = await authClient.GET('/api/user/profile', {
      headers: { Authorization: `Bearer ${accessToken.value}` }
    })
    if (res?.code === 0 && res.data) {
      currentUser.value = res.data
    }
  }

  function clearTokens() {
    accessToken.value = null
    refreshToken.value = null
    currentUser.value = null
  }

  return {
    accessToken,
    refreshToken,
    currentUser,
    isLoggedIn,
    register,
    verifyEmail,
    login,
    refresh,
    logout,
    logoutAll,
    changePassword,
    fetchProfile,
    clearTokens
  }
})

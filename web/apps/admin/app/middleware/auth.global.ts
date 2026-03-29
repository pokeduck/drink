import { useAuthStore } from '~/stores/auth'

export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()

  // 登入頁不需要驗證
  if (to.path === '/login') {
    // 已登入就導回首頁
    if (authStore.isLoggedIn) {
      return navigateTo('/')
    }
    return
  }

  // 未登入導向登入頁
  if (!authStore.isLoggedIn) {
    return navigateTo('/login')
  }
})

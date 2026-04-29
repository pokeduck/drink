import { useAuthStore } from '~/stores/auth'

const publicPaths = ['/login', '/register', '/verify-email']

export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()
  const isPublic = publicPaths.includes(to.path)

  if (isPublic) {
    if (authStore.isLoggedIn && (to.path === '/login' || to.path === '/register')) {
      return navigateTo('/')
    }
    return
  }

  if (!authStore.isLoggedIn) {
    return navigateTo('/login')
  }
})

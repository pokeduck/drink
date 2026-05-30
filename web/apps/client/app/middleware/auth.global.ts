import { useAuthStore } from '~/stores/auth'

const publicPaths = ['/', '/login', '/register', '/verify-email']
const publicPrefixes = ['/group/']

function isPublic(path: string) {
  return publicPaths.includes(path)
    || publicPrefixes.some((prefix) => path.startsWith(prefix))
}

export default defineNuxtRouteMiddleware((to) => {
  const authStore = useAuthStore()

  if (isPublic(to.path)) {
    if (authStore.isLoggedIn && (to.path === '/login' || to.path === '/register')) {
      return navigateTo('/')
    }
    return
  }

  if (!authStore.isLoggedIn) {
    return navigateTo(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
  }
})

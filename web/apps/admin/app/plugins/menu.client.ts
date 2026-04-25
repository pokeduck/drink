import { useAuthStore } from '~/stores/auth'
import { useMenuStore } from '~/stores/menu'

export default defineNuxtPlugin(async () => {
  const authStore = useAuthStore()
  const menuStore = useMenuStore()

  if (authStore.isLoggedIn && menuStore.permissions.size === 0) {
    await menuStore.fetchMenuData()
  }
})

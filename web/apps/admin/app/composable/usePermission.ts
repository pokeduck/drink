import { useMenuStore } from '~/stores/menu'
import type { CrudAction } from '@app/core'

export function usePermission() {
  const menuStore = useMenuStore()

  const can = (menuId: number, action: CrudAction): boolean => {
    const perm = menuStore.permissions.get(menuId)
    if (!perm) return false
    return perm[action]
  }

  return { can }
}

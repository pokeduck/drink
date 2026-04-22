import { defineStore } from 'pinia'
import { useAdminApi } from '~/composable/useAdminApi'
import type { components } from '@app/api-types/admin'

export interface MenuModel {
  index: string
  title: string
  icon?: string
  children?: MenuModel[]
}

type MenuTreeResponse = components['schemas']['MenuTreeResponse']

interface MenuPermission {
  read: boolean
  create: boolean
  update: boolean
  delete: boolean
}

/**
 * 將 API 回傳的 MenuTreeResponse 轉換為前端 MenuModel
 */
function toMenuModel(items: MenuTreeResponse[]): MenuModel[] {
  return items.map((item) => {
    const model: MenuModel = {
      index: item.endpoint ?? `/menu-${item.id}`,
      title: item.name!,
      icon: item.icon ?? undefined,
    }

    if (item.children && item.children.length > 0) {
      model.children = toMenuModel(item.children)
    }

    return model
  })
}

/**
 * 遞迴提取所有節點的權限資訊
 */
function extractPermissions(
  items: MenuTreeResponse[],
  map: Map<number, MenuPermission>,
) {
  for (const item of items) {
    if (item.id != null) {
      map.set(item.id, {
        read: item.can_read ?? false,
        create: item.can_create ?? false,
        update: item.can_update ?? false,
        delete: item.can_delete ?? false,
      })
    }

    if (item.children && item.children.length > 0) {
      extractPermissions(item.children, map)
    }
  }
}

export const useMenuStore = defineStore('menu', () => {
  const menuData = ref<MenuModel[]>([])
  const permissions = ref<Map<number, MenuPermission>>(new Map())
  const loading = ref(false)
  const isCollapsed = ref(false)

  const toggleCollapse = () => {
    isCollapsed.value = !isCollapsed.value
  }

  const fetchMenuData = async () => {
    loading.value = true
    try {
      const api = useAdminApi()
      const { data: res } = await api.GET('/api/admin/menus/me')
      const items = res?.data ?? []
      menuData.value = toMenuModel(items)
      const permMap = new Map<number, MenuPermission>()
      extractPermissions(items, permMap)
      permissions.value = permMap
    } catch (error) {
      console.error('Failed to fetch menu:', error)
    } finally {
      loading.value = false
    }
  }

  const clearMenu = () => {
    menuData.value = []
    permissions.value = new Map()
  }

  return {
    menuData,
    permissions,
    loading,
    isCollapsed,
    fetchMenuData,
    toggleCollapse,
    clearMenu,
  }
})

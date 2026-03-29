import { defineStore } from 'pinia'
import { useApi } from '~/composable/useApi'

export interface MenuModel {
  index: string
  title: string
  icon?: string
  children?: MenuModel[]
}

interface MenuTreeResponse {
  id: number
  name: string
  icon: string | null
  endpoint: string | null
  sort: number
  children: MenuTreeResponse[]
}

interface ApiResponse<T> {
  data: T
  code: number
  error?: string
  message?: string
}

/**
 * 將 API 回傳的 MenuTreeResponse 轉換為前端 MenuModel
 */
function toMenuModel(items: MenuTreeResponse[]): MenuModel[] {
  return items.map((item) => {
    const model: MenuModel = {
      index: item.endpoint ?? `/menu-${item.id}`,
      title: item.name,
      icon: item.icon ?? undefined,
    }

    if (item.children && item.children.length > 0) {
      model.children = toMenuModel(item.children)
    }

    return model
  })
}

export const useMenuStore = defineStore('menu', () => {
  const menuData = ref<MenuModel[]>([])
  const loading = ref(false)
  const isCollapsed = ref(false)

  const toggleCollapse = () => {
    isCollapsed.value = !isCollapsed.value
  }

  const fetchMenuData = async () => {
    loading.value = true
    try {
      const api = useApi()
      const res = await api.get<ApiResponse<MenuTreeResponse[]>>('/admin/menus/me')
      menuData.value = toMenuModel(res.data)
    } catch (error) {
      console.error('Failed to fetch menu:', error)
    } finally {
      loading.value = false
    }
  }

  const clearMenu = () => {
    menuData.value = []
  }

  return {
    menuData,
    loading,
    isCollapsed,
    fetchMenuData,
    toggleCollapse,
    clearMenu,
  }
})

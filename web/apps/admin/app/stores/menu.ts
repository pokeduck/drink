import { defineStore } from 'pinia'

export interface MenuModel {
  index: string
  title: string
  icon?: string
  children?: MenuModel[]
}

export const useMenuStore = defineStore('menu', () => {
  const menuData = ref<MenuModel[]>([])
  const loading = ref(false)
  const isCollapsed = ref(false)

  // 切換收合狀態
  const toggleCollapse = () => {
    isCollapsed.value = !isCollapsed.value
  }

  // 初始化 Mock Data
  const initMockData = () => {
    menuData.value = [
      {
        index: '/',
        title: '控制台',
        icon: 'House',
      },
      {
        index: '/products',
        title: '產品管理',
        icon: 'Goods',
        children: [
          { index: '/products/list', title: '商品列表' },
          {
            index: '/products/config',
            title: '分類配置',
            children: [
              { index: '/products/config/drinks', title: '飲品分類' },
              { index: '/products/config/foods', title: '熟食分類' },
            ],
          },
        ],
      },
      {
        index: '/orders',
        title: '訂單系統',
        icon: 'List',
        children: [
          { index: '/orders/active', title: '處理中訂單' },
          { index: '/orders/history', title: '歷史成交紀錄' },
        ],
      },
      {
        index: '/settings',
        title: '系統設定',
        icon: 'Setting',
      }
    ]
  }

  /**
   * 未來對接 API 的位置
   */
  const fetchMenuData = async () => {
    loading.value = true
    try {
      // TODO: const { data } = await useFetch('/api/menu')
      // menuData.value = data.value
      
      // 目前先直接使用 Mock Data
      initMockData()
    } catch (error) {
      console.error('Failed to fetch menu:', error)
    } finally {
      loading.value = false
    }
  }

  return {
    menuData,
    loading,
    isCollapsed,
    fetchMenuData,
    toggleCollapse
  }
})

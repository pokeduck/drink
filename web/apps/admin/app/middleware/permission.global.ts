import { useMenuStore } from '~/stores/menu'
import { MENU } from '@app/core'
import type { CrudAction } from '@app/core'

interface RoutePermission {
  menuId: number
  action: CrudAction
}

const routePermissions: { pattern: RegExp; perm: RoutePermission }[] = [
  // 後台帳號管理
  { pattern: /^\/admin-account\/list$/, perm: { menuId: MENU.AdminAccountList, action: 'read' } },
  { pattern: /^\/admin-account\/create$/, perm: { menuId: MENU.AdminAccountList, action: 'create' } },
  { pattern: /^\/admin-account\/[^/]+\/edit$/, perm: { menuId: MENU.AdminAccountList, action: 'update' } },
  { pattern: /^\/admin-account\/role$/, perm: { menuId: MENU.AdminRole, action: 'read' } },
  { pattern: /^\/admin-account\/role\/create$/, perm: { menuId: MENU.AdminRole, action: 'create' } },
  { pattern: /^\/admin-account\/role\/[^/]+\/edit$/, perm: { menuId: MENU.AdminRole, action: 'update' } },

  // 前台會員管理
  { pattern: /^\/member\/list$/, perm: { menuId: MENU.MemberList, action: 'read' } },
  { pattern: /^\/member\/create$/, perm: { menuId: MENU.MemberList, action: 'create' } },
  { pattern: /^\/member\/[^/]+\/edit$/, perm: { menuId: MENU.MemberList, action: 'update' } },
  { pattern: /^\/member\/verification\/register$/, perm: { menuId: MENU.VerificationRegister, action: 'read' } },
  { pattern: /^\/member\/verification\/forgot-password$/, perm: { menuId: MENU.VerificationForgotPassword, action: 'read' } },

  // 飲料選項
  { pattern: /^\/drink-option\/item$/, perm: { menuId: MENU.DrinkItem, action: 'read' } },
  { pattern: /^\/drink-option\/sugar$/, perm: { menuId: MENU.Sugar, action: 'read' } },
  { pattern: /^\/drink-option\/ice$/, perm: { menuId: MENU.Ice, action: 'read' } },
  { pattern: /^\/drink-option\/topping$/, perm: { menuId: MENU.Topping, action: 'read' } },
  { pattern: /^\/drink-option\/size$/, perm: { menuId: MENU.Size, action: 'read' } },

  // 訂單
  { pattern: /^\/order\/list$/, perm: { menuId: MENU.OrderList, action: 'read' } },

  // 店家
  { pattern: /^\/shop\/list$/, perm: { menuId: MENU.ShopList, action: 'read' } },
  { pattern: /^\/shop\/override$/, perm: { menuId: MENU.ShopOverride, action: 'read' } },

  // 通知
  { pattern: /^\/notification\/list$/, perm: { menuId: MENU.NotificationList, action: 'read' } },
  { pattern: /^\/notification\/by-group$/, perm: { menuId: MENU.NotificationByGroup, action: 'read' } },

  // 系統設定
  { pattern: /^\/system\/setting$/, perm: { menuId: MENU.SystemSetting, action: 'read' } },
]

function findPermission(path: string): RoutePermission | null {
  for (const route of routePermissions) {
    if (route.pattern.test(path)) {
      return route.perm
    }
  }
  return null
}

export default defineNuxtRouteMiddleware((to) => {
  // 登入頁不檢查權限
  if (to.path === '/login') return

  const required = findPermission(to.path)

  // 沒有對應的權限規則，允許通過（如首頁、修改密碼等）
  if (!required) return

  const menuStore = useMenuStore()
  const perm = menuStore.permissions.get(required.menuId)

  if (!perm || !perm[required.action]) {
    return navigateTo({ path: '/', query: { forbidden: '1' } })
  }
})

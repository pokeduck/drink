export const MENU = {
  AdminAccountList: 2,
  AdminRole: 3,
  MemberList: 5,
  VerificationRegister: 6,
  VerificationForgotPassword: 22,
  OrderList: 8,
  ShopList: 10,
  DrinkItem: 12,
  Sugar: 13,
  Ice: 14,
  Topping: 15,
  Size: 16,
  ShopOverride: 17,
  NotificationList: 19,
  NotificationByGroup: 20,
  SystemSetting: 21,
} as const

export type MenuId = (typeof MENU)[keyof typeof MENU]
export type CrudAction = 'read' | 'create' | 'update' | 'delete'

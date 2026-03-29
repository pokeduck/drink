export const ErrorCodes = {
  // 通用 (4-00-XX)
  ValidationError:         { code: 40001, error: 'VALIDATION_ERROR' },
  Unauthorized:            { code: 40002, error: 'UNAUTHORIZED' },
  Forbidden:               { code: 40003, error: 'FORBIDDEN' },
  NotFound:                { code: 40004, error: 'NOT_FOUND' },
  ServiceUnavailable:      { code: 40005, error: 'SERVICE_UNAVAILABLE' },

  // Admin Auth / Admin User (4-01-XX)
  InvalidCredentials:      { code: 40101, error: 'INVALID_CREDENTIALS' },
  InvalidPassword:         { code: 40102, error: 'INVALID_PASSWORD' },
  UsernameAlreadyExists:   { code: 40103, error: 'USERNAME_ALREADY_EXISTS' },
  AdminAccountInactive:    { code: 40104, error: 'ADMIN_ACCOUNT_INACTIVE' },
  CannotDeleteAdmin:       { code: 40105, error: 'CANNOT_DELETE_ADMIN' },
  CannotChangeAdminRole:   { code: 40106, error: 'CANNOT_CHANGE_ADMIN_ROLE' },

  // Admin Role (4-02-XX)
  RoleNotFound:            { code: 40201, error: 'ROLE_NOT_FOUND' },
  RoleAlreadyExists:       { code: 40202, error: 'ROLE_ALREADY_EXISTS' },
  CannotModifySystemRole:  { code: 40203, error: 'CANNOT_MODIFY_SYSTEM_ROLE' },
  CannotDeleteSystemRole:  { code: 40204, error: 'CANNOT_DELETE_SYSTEM_ROLE' },
  RoleHasStaff:            { code: 40205, error: 'ROLE_HAS_STAFF' },
  InvalidMenuId:           { code: 40206, error: 'INVALID_MENU_ID' },

  // User Auth (4-03-XX)
  EmailAlreadyExists:      { code: 40301, error: 'EMAIL_ALREADY_EXISTS' },
  UserInvalidCredentials:  { code: 40302, error: 'INVALID_CREDENTIALS' },
  EmailNotVerified:        { code: 40303, error: 'EMAIL_NOT_VERIFIED' },
  AccountInactive:         { code: 40304, error: 'ACCOUNT_INACTIVE' },
  GoogleDomainNotAllowed:  { code: 40305, error: 'GOOGLE_DOMAIN_NOT_ALLOWED' },
  InvalidToken:            { code: 40306, error: 'INVALID_TOKEN' },
  TokenAlreadyUsed:        { code: 40307, error: 'TOKEN_ALREADY_USED' },

  // Verification (4-04-XX)
  ResendTooFrequent:       { code: 40401, error: 'RESEND_TOO_FREQUENT' },

  // Order (4-05-XX)
  ShopNotAvailable:        { code: 40501, error: 'SHOP_NOT_AVAILABLE' },
  InvalidDeadline:         { code: 40502, error: 'INVALID_DEADLINE' },
  NotInitiator:            { code: 40503, error: 'NOT_INITIATOR' },
  OrderNotActive:          { code: 40504, error: 'ORDER_NOT_ACTIVE' },
  InvalidStatusTransition: { code: 40505, error: 'INVALID_STATUS_TRANSITION' },
  CannotCancelOrder:       { code: 40506, error: 'CANNOT_CANCEL_ORDER' },
  OrderNotFound:           { code: 40507, error: 'ORDER_NOT_FOUND' },

  // Shop (4-06-XX)
  ShopAlreadyExists:       { code: 40601, error: 'SHOP_ALREADY_EXISTS' },
  ShopNotFound:            { code: 40602, error: 'SHOP_NOT_FOUND' },
  CategoryAlreadyExists:   { code: 40603, error: 'CATEGORY_ALREADY_EXISTS' },
  CategoryNotFound:        { code: 40604, error: 'CATEGORY_NOT_FOUND' },
  MenuItemNotFound:        { code: 40605, error: 'MENU_ITEM_NOT_FOUND' },
  DrinkItemNotFound:       { code: 40606, error: 'DRINK_ITEM_NOT_FOUND' },

  // Drink Option (4-07-XX)
  DrinkItemAlreadyExists:  { code: 40701, error: 'DRINK_ITEM_ALREADY_EXISTS' },
  DrinkItemInUse:          { code: 40702, error: 'DRINK_ITEM_IN_USE' },
  SugarAlreadyExists:      { code: 40703, error: 'SUGAR_ALREADY_EXISTS' },
  SugarInUse:              { code: 40704, error: 'SUGAR_IN_USE' },
  SugarNotFound:           { code: 40705, error: 'SUGAR_NOT_FOUND' },
  IceAlreadyExists:        { code: 40706, error: 'ICE_ALREADY_EXISTS' },
  IceInUse:                { code: 40707, error: 'ICE_IN_USE' },
  IceNotFound:             { code: 40708, error: 'ICE_NOT_FOUND' },
  ToppingAlreadyExists:    { code: 40709, error: 'TOPPING_ALREADY_EXISTS' },
  ToppingInUse:            { code: 40710, error: 'TOPPING_IN_USE' },
  ToppingNotFound:         { code: 40711, error: 'TOPPING_NOT_FOUND' },
  SizeAlreadyExists:       { code: 40712, error: 'SIZE_ALREADY_EXISTS' },
  SizeInUse:               { code: 40713, error: 'SIZE_IN_USE' },
  SizeNotFound:            { code: 40714, error: 'SIZE_NOT_FOUND' },

  // System Setting (4-08-XX)
  SettingKeyNotFound:      { code: 40801, error: 'SETTING_KEY_NOT_FOUND' },
  SettingValueInvalid:     { code: 40802, error: 'SETTING_VALUE_INVALID' },

  // Notification (4-09-XX)
  NotificationNotFound:    { code: 40901, error: 'NOTIFICATION_NOT_FOUND' },
} as const

export type ErrorCode = typeof ErrorCodes[keyof typeof ErrorCodes]

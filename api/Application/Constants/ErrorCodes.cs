namespace Drink.Application.Constants;

public static class ErrorCodes
{
  // 通用 (4-00-XX)
  public static readonly (int Code, string Error) ValidationError = (40001, "VALIDATION_ERROR");
  public static readonly (int Code, string Error) Unauthorized = (40002, "UNAUTHORIZED");
  public static readonly (int Code, string Error) Forbidden = (40003, "FORBIDDEN");
  public static readonly (int Code, string Error) NotFound = (40004, "NOT_FOUND");
  public static readonly (int Code, string Error) ServiceUnavailable = (40005, "SERVICE_UNAVAILABLE");

  // Admin Auth / Admin User (4-01-XX)
  public static readonly (int Code, string Error) InvalidCredentials = (40101, "INVALID_CREDENTIALS");
  public static readonly (int Code, string Error) InvalidPassword = (40102, "INVALID_PASSWORD");
  public static readonly (int Code, string Error) UsernameAlreadyExists = (40103, "USERNAME_ALREADY_EXISTS");
  public static readonly (int Code, string Error) AdminAccountInactive = (40104, "ADMIN_ACCOUNT_INACTIVE");
  public static readonly (int Code, string Error) CannotDeleteAdmin = (40105, "CANNOT_DELETE_ADMIN");
  public static readonly (int Code, string Error) CannotChangeAdminRole = (40106, "CANNOT_CHANGE_ADMIN_ROLE");

  // Admin Role (4-02-XX)
  public static readonly (int Code, string Error) RoleNotFound = (40201, "ROLE_NOT_FOUND");
  public static readonly (int Code, string Error) RoleAlreadyExists = (40202, "ROLE_ALREADY_EXISTS");
  public static readonly (int Code, string Error) CannotModifySystemRole = (40203, "CANNOT_MODIFY_SYSTEM_ROLE");
  public static readonly (int Code, string Error) CannotDeleteSystemRole = (40204, "CANNOT_DELETE_SYSTEM_ROLE");
  public static readonly (int Code, string Error) RoleHasStaff = (40205, "ROLE_HAS_STAFF");
  public static readonly (int Code, string Error) InvalidMenuId = (40206, "INVALID_MENU_ID");

  // User Auth (4-03-XX)
  public static readonly (int Code, string Error) EmailAlreadyExists = (40301, "EMAIL_ALREADY_EXISTS");
  public static readonly (int Code, string Error) UserInvalidCredentials = (40302, "INVALID_CREDENTIALS");
  public static readonly (int Code, string Error) EmailNotVerified = (40303, "EMAIL_NOT_VERIFIED");
  public static readonly (int Code, string Error) AccountInactive = (40304, "ACCOUNT_INACTIVE");
  public static readonly (int Code, string Error) GoogleDomainNotAllowed = (40305, "GOOGLE_DOMAIN_NOT_ALLOWED");
  public static readonly (int Code, string Error) InvalidToken = (40306, "INVALID_TOKEN");
  public static readonly (int Code, string Error) TokenAlreadyUsed = (40307, "TOKEN_ALREADY_USED");

  // Verification (4-04-XX)
  public static readonly (int Code, string Error) ResendTooFrequent = (40401, "RESEND_TOO_FREQUENT");

  // Order (4-05-XX)
  public static readonly (int Code, string Error) ShopNotAvailable = (40501, "SHOP_NOT_AVAILABLE");
  public static readonly (int Code, string Error) InvalidDeadline = (40502, "INVALID_DEADLINE");
  public static readonly (int Code, string Error) NotInitiator = (40503, "NOT_INITIATOR");
  public static readonly (int Code, string Error) OrderNotActive = (40504, "ORDER_NOT_ACTIVE");
  public static readonly (int Code, string Error) InvalidStatusTransition = (40505, "INVALID_STATUS_TRANSITION");
  public static readonly (int Code, string Error) CannotCancelOrder = (40506, "CANNOT_CANCEL_ORDER");
  public static readonly (int Code, string Error) OrderNotFound = (40507, "ORDER_NOT_FOUND");

  // Shop (4-06-XX)
  public static readonly (int Code, string Error) ShopAlreadyExists = (40601, "SHOP_ALREADY_EXISTS");
  public static readonly (int Code, string Error) ShopNotFound = (40602, "SHOP_NOT_FOUND");
  public static readonly (int Code, string Error) CategoryAlreadyExists = (40603, "CATEGORY_ALREADY_EXISTS");
  public static readonly (int Code, string Error) CategoryNotFound = (40604, "CATEGORY_NOT_FOUND");
  public static readonly (int Code, string Error) MenuItemNotFound = (40605, "MENU_ITEM_NOT_FOUND");
  public static readonly (int Code, string Error) DrinkItemNotFound = (40606, "DRINK_ITEM_NOT_FOUND");

  // Drink Option (4-07-XX)
  public static readonly (int Code, string Error) DrinkItemAlreadyExists = (40701, "DRINK_ITEM_ALREADY_EXISTS");
  public static readonly (int Code, string Error) DrinkItemInUse = (40702, "DRINK_ITEM_IN_USE");
  public static readonly (int Code, string Error) SugarAlreadyExists = (40703, "SUGAR_ALREADY_EXISTS");
  public static readonly (int Code, string Error) SugarInUse = (40704, "SUGAR_IN_USE");
  public static readonly (int Code, string Error) SugarNotFound = (40705, "SUGAR_NOT_FOUND");
  public static readonly (int Code, string Error) IceAlreadyExists = (40706, "ICE_ALREADY_EXISTS");
  public static readonly (int Code, string Error) IceInUse = (40707, "ICE_IN_USE");
  public static readonly (int Code, string Error) IceNotFound = (40708, "ICE_NOT_FOUND");
  public static readonly (int Code, string Error) ToppingAlreadyExists = (40709, "TOPPING_ALREADY_EXISTS");
  public static readonly (int Code, string Error) ToppingInUse = (40710, "TOPPING_IN_USE");
  public static readonly (int Code, string Error) ToppingNotFound = (40711, "TOPPING_NOT_FOUND");
  public static readonly (int Code, string Error) SizeAlreadyExists = (40712, "SIZE_ALREADY_EXISTS");
  public static readonly (int Code, string Error) SizeInUse = (40713, "SIZE_IN_USE");
  public static readonly (int Code, string Error) SizeNotFound = (40714, "SIZE_NOT_FOUND");

  // System Setting (4-08-XX)
  public static readonly (int Code, string Error) SettingKeyNotFound = (40801, "SETTING_KEY_NOT_FOUND");
  public static readonly (int Code, string Error) SettingValueInvalid = (40802, "SETTING_VALUE_INVALID");

  // Notification (4-09-XX)
  public static readonly (int Code, string Error) NotificationNotFound = (40901, "NOTIFICATION_NOT_FOUND");

  // File Upload (4-10-XX)
  public static readonly (int Code, string Error) FileExtensionNotAllowed = (41001, "FILE_EXTENSION_NOT_ALLOWED");
  public static readonly (int Code, string Error) FileSizeExceeded = (41002, "FILE_SIZE_EXCEEDED");
  public static readonly (int Code, string Error) FileNotFound = (41003, "FILE_NOT_FOUND");
  public static readonly (int Code, string Error) FileUploadFailed = (41004, "FILE_UPLOAD_FAILED");
}
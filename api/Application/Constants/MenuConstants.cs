namespace Drink.Application.Constants;

/// <summary>
/// Menu ID 常數，對應 AdminMenu 資料表的 Seed Data
/// </summary>
public static class MenuConstants
{
  // 後台帳號管理
  public const int AdminAccountList = 2;
  public const int AdminRole = 3;

  // 會員管理
  public const int MemberList = 5;
  public const int VerificationRegister = 6;
  public const int VerificationForgotPassword = 22;

  // 訂單管理
  public const int OrderList = 8;

  // 店家管理
  public const int ShopList = 10;

  // 飲料選項
  public const int DrinkItem = 12;
  public const int Sugar = 13;
  public const int Ice = 14;
  public const int Topping = 15;
  public const int Size = 16;

  // 店家覆寫設定
  public const int ShopOverride = 17;

  // 通知管理
  public const int NotificationList = 19;
  public const int NotificationByGroup = 20;

  // 系統設定
  public const int SystemSetting = 21;
}

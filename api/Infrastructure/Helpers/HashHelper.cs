using Isopoh.Cryptography.Argon2;

namespace Drink.Infrastructure.Helpers;

public static class HashHelper
{
  public static string HashPassword(string password, string pepper)
  {
    var input = password + pepper;
    return Argon2.Hash(input);
  }

  public static bool VerifyPassword(string password, string pepper, string hashedPassword)
  {
    var input = password + pepper;
    return Argon2.Verify(hashedPassword, input);
  }
}
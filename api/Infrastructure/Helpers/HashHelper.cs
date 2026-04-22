using Drink.Application.Interfaces;
using Isopoh.Cryptography.Argon2;

namespace Drink.Infrastructure.Helpers;

public class Argon2PasswordHasher : IPasswordHasher
{
  public string HashPassword(string password, string pepper)
  {
    var input = password + pepper;
    return Argon2.Hash(input);
  }

  public bool VerifyPassword(string password, string pepper, string hashedPassword)
  {
    var input = password + pepper;
    return Argon2.Verify(hashedPassword, input);
  }
}

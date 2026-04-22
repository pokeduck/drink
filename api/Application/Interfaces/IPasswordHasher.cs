namespace Drink.Application.Interfaces;

public interface IPasswordHasher
{
  string HashPassword(string password, string pepper);
  bool VerifyPassword(string password, string pepper, string hashedPassword);
}

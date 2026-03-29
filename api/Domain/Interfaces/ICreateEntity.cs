namespace Drink.Domain.Interfaces;

public interface ICreateEntity
{
  DateTime CreatedAt { get; set; }
  int Creator { get; set; }
}
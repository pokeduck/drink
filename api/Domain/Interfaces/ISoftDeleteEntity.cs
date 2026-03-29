namespace Drink.Domain.Interfaces;

public interface ISoftDeleteEntity
{
  bool IsDeleted { get; set; }
  DateTime? DeletedAt { get; set; }
}
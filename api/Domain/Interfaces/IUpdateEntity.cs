namespace Drink.Domain.Interfaces;

public interface IUpdateEntity
{
    DateTime UpdatedAt { get; set; }
    int Updater { get; set; }
}

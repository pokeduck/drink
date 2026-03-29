using Drink.Infrastructure.Data;
using Microsoft.Extensions.Configuration;

namespace Drink.Migrator.Seeders;

public interface ISeeder
{
  int Order { get; }
  Task Seed(DrinkDbContext context, IConfiguration configuration);
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Drink.Infrastructure.Data;

public class DrinkDbContextFactory : IDesignTimeDbContextFactory<DrinkDbContext>
{
  public DrinkDbContext CreateDbContext(string[] args)
  {
    var configuration = new ConfigurationBuilder()
      .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../Migrator"))
      .AddJsonFile("appsettings.json", optional: false)
      .AddJsonFile("appsettings.Development.json", optional: true)
      .Build();

    var connectionString = configuration.GetConnectionString("DefaultConnection")
      ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

    var optionsBuilder = new DbContextOptionsBuilder<DrinkDbContext>();
    optionsBuilder.UseNpgsql(connectionString, o =>
      o.MigrationsHistoryTable("__ef_migration_history"));

    return new DrinkDbContext(optionsBuilder.Options);
  }
}

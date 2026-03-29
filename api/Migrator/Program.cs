using System.Reflection;
using Drink.Infrastructure.Data;
using Drink.Migrator.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Drink.Migrator;

public class Program
{
  public static async Task Main(string[] args)
  {
    var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";

    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false)
        .AddJsonFile($"appsettings.{environment}.json", optional: true)
        .AddEnvironmentVariables()
        .Build();

    var connectionString = configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

    var optionsBuilder = new DbContextOptionsBuilder<DrinkDbContext>();
    optionsBuilder.UseNpgsql(connectionString, o =>
        o.MigrationsHistoryTable("__ef_migration_history"));

    await using var context = new DrinkDbContext(optionsBuilder.Options);

    try
    {
      await context.Database.MigrateAsync();
      Console.WriteLine("Migration 完成");

      await RunSeeders(context, configuration);
      Console.WriteLine("Seed Data 完成");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"發生錯誤: {ex.Message}");
    }
  }

  private static async Task RunSeeders(DrinkDbContext context, IConfiguration configuration)
  {
    var seeders = Assembly.GetExecutingAssembly()
        .GetTypes()
        .Where(t => t is { IsClass: true, IsAbstract: false } && typeof(ISeeder).IsAssignableFrom(t))
        .Select(t => (ISeeder)Activator.CreateInstance(t)!)
        .OrderBy(s => s.Order)
        .ToList();

    foreach (var seeder in seeders)
    {
      await seeder.Seed(context, configuration);
    }
  }
}

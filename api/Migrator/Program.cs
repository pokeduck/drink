using Drink.Infrastructure.Data;
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
    optionsBuilder.UseNpgsql(connectionString);

    await using var context = new DrinkDbContext(optionsBuilder.Options);

    try
    {
      await context.Database.MigrateAsync();
      Console.WriteLine("資料庫更新成功！");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"發生錯誤: {ex.Message}");
    }
  }
}

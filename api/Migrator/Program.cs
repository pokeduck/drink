using Drink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Drink.Migrator;

public class Program
{
  public static async Task Main(string[] args)
  {
    // 這裡建議從 appsettings.json 讀取連線字串
    var connectionString = "Your_Connection_String";
    var optionsBuilder = new DbContextOptionsBuilder<DrinkDbContext>();
    optionsBuilder.UseNpgsql(connectionString);

    await using var context = new DrinkDbContext(optionsBuilder.Options);

    try
    {
      // 這行就是執行 Update-Database 的程式碼版本
      await context.Database.MigrateAsync();
      Console.WriteLine("✅ 資料庫更新成功！");
    }
    catch (Exception ex)
    {
      Console.WriteLine($"❌ 發生錯誤: {ex.Message}");
    }
  }
}
using Drink.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Drink.Infrastructure.Data;

public class DrinkDbContext : DbContext
{
  public DrinkDbContext(DbContextOptions<DrinkDbContext> options) : base(options)
  { }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    base.OnConfiguring(optionsBuilder);
    optionsBuilder.UseSnakeCaseNamingConvention();
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    modelBuilder.RegisterAllEntities();
  }
}
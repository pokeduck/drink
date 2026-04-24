using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MoveMaxToppingCountToMenuItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_topping_count",
                table: "shop");

            migrationBuilder.AddColumn<int>(
                name: "max_topping_count",
                table: "shop_menu_item",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "max_topping_count",
                table: "shop_menu_item");

            migrationBuilder.AddColumn<int>(
                name: "max_topping_count",
                table: "shop",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

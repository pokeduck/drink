using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopCoverAndExternalUrl : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cover_image_path",
                table: "shop",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "external_url",
                table: "shop",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cover_image_path",
                table: "shop");

            migrationBuilder.DropColumn(
                name: "external_url",
                table: "shop");
        }
    }
}

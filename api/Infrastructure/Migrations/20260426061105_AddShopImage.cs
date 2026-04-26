using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Drink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShopImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shop_image",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    drink_item_id = table.Column<int>(type: "integer", nullable: true),
                    path = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    is_cover = table.Column<bool>(type: "boolean", nullable: false),
                    original_file_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_image", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_image_drink_item_drink_item_id",
                        column: x => x.drink_item_id,
                        principalTable: "drink_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shop_image_shop_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shop_image_drink_item_id",
                table: "shop_image",
                column: "drink_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_image_hash",
                table: "shop_image",
                column: "hash");

            migrationBuilder.CreateIndex(
                name: "ix_shop_image_shop_id",
                table: "shop_image",
                column: "shop_id",
                filter: "drink_item_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_shop_image_shop_id_drink_item_id",
                table: "shop_image",
                columns: new[] { "shop_id", "drink_item_id" },
                unique: true,
                filter: "is_cover = true");

            migrationBuilder.CreateIndex(
                name: "ix_shop_image_shop_id_drink_item_id_sort",
                table: "shop_image",
                columns: new[] { "shop_id", "drink_item_id", "sort" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shop_image");
        }
    }
}

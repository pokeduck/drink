using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Drink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminMenuIsPermissionOnly : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sort",
                table: "shop_topping_override");

            migrationBuilder.DropColumn(
                name: "sort",
                table: "shop_sugar_override");

            migrationBuilder.AddColumn<bool>(
                name: "is_permission_only",
                table: "admin_menu",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "shop_enabled_ice",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    ice_id = table.Column<int>(type: "integer", nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_enabled_ice", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_enabled_ice_ice_ice_id",
                        column: x => x.ice_id,
                        principalTable: "ice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shop_enabled_ice_shop_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_enabled_size",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    size_id = table.Column<int>(type: "integer", nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_enabled_size", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_enabled_size_shop_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shop_enabled_size_size_size_id",
                        column: x => x.size_id,
                        principalTable: "size",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shop_enabled_sugar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    sugar_id = table.Column<int>(type: "integer", nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_enabled_sugar", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_enabled_sugar_shop_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shop_enabled_sugar_sugar_sugar_id",
                        column: x => x.sugar_id,
                        principalTable: "sugar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shop_enabled_topping",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    topping_id = table.Column<int>(type: "integer", nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_enabled_topping", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_enabled_topping_shop_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shop_enabled_topping_topping_topping_id",
                        column: x => x.topping_id,
                        principalTable: "topping",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shop_enabled_ice_ice_id",
                table: "shop_enabled_ice",
                column: "ice_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_enabled_ice_shop_id_ice_id",
                table: "shop_enabled_ice",
                columns: new[] { "shop_id", "ice_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_enabled_size_shop_id_size_id",
                table: "shop_enabled_size",
                columns: new[] { "shop_id", "size_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_enabled_size_size_id",
                table: "shop_enabled_size",
                column: "size_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_enabled_sugar_shop_id_sugar_id",
                table: "shop_enabled_sugar",
                columns: new[] { "shop_id", "sugar_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_enabled_sugar_sugar_id",
                table: "shop_enabled_sugar",
                column: "sugar_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_enabled_topping_shop_id_topping_id",
                table: "shop_enabled_topping",
                columns: new[] { "shop_id", "topping_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_enabled_topping_topping_id",
                table: "shop_enabled_topping",
                column: "topping_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shop_enabled_ice");

            migrationBuilder.DropTable(
                name: "shop_enabled_size");

            migrationBuilder.DropTable(
                name: "shop_enabled_sugar");

            migrationBuilder.DropTable(
                name: "shop_enabled_topping");

            migrationBuilder.DropColumn(
                name: "is_permission_only",
                table: "admin_menu");

            migrationBuilder.AddColumn<int>(
                name: "sort",
                table: "shop_topping_override",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "sort",
                table: "shop_sugar_override",
                type: "integer",
                nullable: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Drink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddShop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "shop",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    max_topping_per_item = table.Column<int>(type: "integer", nullable: false),
                    max_topping_count = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "shop_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_category", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_category_shop_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_sugar_override",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    sugar_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    sort = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_sugar_override", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_sugar_override_shop_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shop_sugar_override_sugar_sugar_id",
                        column: x => x.sugar_id,
                        principalTable: "sugar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shop_topping_override",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    topping_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: true),
                    sort = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_topping_override", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_topping_override_shop_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shop_topping_override_topping_topping_id",
                        column: x => x.topping_id,
                        principalTable: "topping",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shop_menu_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    category_id = table.Column<int>(type: "integer", nullable: false),
                    drink_item_id = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sort = table.Column<int>(type: "integer", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_menu_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_menu_item_drink_item_drink_item_id",
                        column: x => x.drink_item_id,
                        principalTable: "drink_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shop_menu_item_shop_category_category_id",
                        column: x => x.category_id,
                        principalTable: "shop_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_menu_item_ice",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    ice_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_menu_item_ice", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_menu_item_ice_ice_ice_id",
                        column: x => x.ice_id,
                        principalTable: "ice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_shop_menu_item_ice_shop_menu_item_menu_item_id",
                        column: x => x.menu_item_id,
                        principalTable: "shop_menu_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "shop_menu_item_size",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    size_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_menu_item_size", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_menu_item_size_shop_menu_item_menu_item_id",
                        column: x => x.menu_item_id,
                        principalTable: "shop_menu_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shop_menu_item_size_size_size_id",
                        column: x => x.size_id,
                        principalTable: "size",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shop_menu_item_sugar",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    sugar_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_menu_item_sugar", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_menu_item_sugar_shop_menu_item_menu_item_id",
                        column: x => x.menu_item_id,
                        principalTable: "shop_menu_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shop_menu_item_sugar_sugar_sugar_id",
                        column: x => x.sugar_id,
                        principalTable: "sugar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "shop_menu_item_topping",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    topping_id = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_shop_menu_item_topping", x => x.id);
                    table.ForeignKey(
                        name: "fk_shop_menu_item_topping_shop_menu_item_menu_item_id",
                        column: x => x.menu_item_id,
                        principalTable: "shop_menu_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_shop_menu_item_topping_topping_topping_id",
                        column: x => x.topping_id,
                        principalTable: "topping",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_shop_name",
                table: "shop",
                column: "name",
                unique: true,
                filter: "is_deleted = false");

            migrationBuilder.CreateIndex(
                name: "ix_shop_category_shop_id",
                table: "shop_category",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_menu_item_category_id",
                table: "shop_menu_item",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_menu_item_drink_item_id",
                table: "shop_menu_item",
                column: "drink_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_menu_item_ice_ice_id",
                table: "shop_menu_item_ice",
                column: "ice_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_menu_item_ice_menu_item_id",
                table: "shop_menu_item_ice",
                column: "menu_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_menu_item_size_menu_item_id",
                table: "shop_menu_item_size",
                column: "menu_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_menu_item_size_size_id",
                table: "shop_menu_item_size",
                column: "size_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_menu_item_sugar_menu_item_id",
                table: "shop_menu_item_sugar",
                column: "menu_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_menu_item_sugar_sugar_id",
                table: "shop_menu_item_sugar",
                column: "sugar_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_menu_item_topping_menu_item_id",
                table: "shop_menu_item_topping",
                column: "menu_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_menu_item_topping_topping_id",
                table: "shop_menu_item_topping",
                column: "topping_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_sugar_override_shop_id_sugar_id",
                table: "shop_sugar_override",
                columns: new[] { "shop_id", "sugar_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_sugar_override_sugar_id",
                table: "shop_sugar_override",
                column: "sugar_id");

            migrationBuilder.CreateIndex(
                name: "ix_shop_topping_override_shop_id_topping_id",
                table: "shop_topping_override",
                columns: new[] { "shop_id", "topping_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_shop_topping_override_topping_id",
                table: "shop_topping_override",
                column: "topping_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "shop_menu_item_ice");

            migrationBuilder.DropTable(
                name: "shop_menu_item_size");

            migrationBuilder.DropTable(
                name: "shop_menu_item_sugar");

            migrationBuilder.DropTable(
                name: "shop_menu_item_topping");

            migrationBuilder.DropTable(
                name: "shop_sugar_override");

            migrationBuilder.DropTable(
                name: "shop_topping_override");

            migrationBuilder.DropTable(
                name: "shop_menu_item");

            migrationBuilder.DropTable(
                name: "shop_category");

            migrationBuilder.DropTable(
                name: "shop");
        }
    }
}

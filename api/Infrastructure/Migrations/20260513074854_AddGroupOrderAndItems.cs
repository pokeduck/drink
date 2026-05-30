using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Drink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupOrderAndItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "group_order",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    shop_id = table.Column<int>(type: "integer", nullable: false),
                    initiator_id = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    deadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_order", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_order_shop_shop_id",
                        column: x => x.shop_id,
                        principalTable: "shop",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_group_order_user_initiator_id",
                        column: x => x.initiator_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_item",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    group_order_id = table.Column<int>(type: "integer", nullable: false),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    recipient_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    menu_item_id = table.Column<int>(type: "integer", nullable: false),
                    size_id = table.Column<int>(type: "integer", nullable: false),
                    sugar_id = table.Column<int>(type: "integer", nullable: false),
                    ice_id = table.Column<int>(type: "integer", nullable: false),
                    item_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    sugar_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    topping_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_item", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_item_group_order_group_order_id",
                        column: x => x.group_order_id,
                        principalTable: "group_order",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_item_ice_ice_id",
                        column: x => x.ice_id,
                        principalTable: "ice",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_item_shop_menu_item_menu_item_id",
                        column: x => x.menu_item_id,
                        principalTable: "shop_menu_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_item_size_size_id",
                        column: x => x.size_id,
                        principalTable: "size",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_item_sugar_sugar_id",
                        column: x => x.sugar_id,
                        principalTable: "sugar",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_order_item_user_user_id",
                        column: x => x.user_id,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "order_item_topping",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    order_item_id = table.Column<int>(type: "integer", nullable: false),
                    topping_id = table.Column<int>(type: "integer", nullable: false),
                    price = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creator = table.Column<int>(type: "integer", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updater = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_item_topping", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_item_topping_order_item_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_item",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_order_item_topping_topping_topping_id",
                        column: x => x.topping_id,
                        principalTable: "topping",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_group_order_deadline",
                table: "group_order",
                column: "deadline");

            migrationBuilder.CreateIndex(
                name: "ix_group_order_initiator_id",
                table: "group_order",
                column: "initiator_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_order_shop_id",
                table: "group_order",
                column: "shop_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_order_status",
                table: "group_order",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_group_order_id",
                table: "order_item",
                column: "group_order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_ice_id",
                table: "order_item",
                column: "ice_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_menu_item_id",
                table: "order_item",
                column: "menu_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_size_id",
                table: "order_item",
                column: "size_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_sugar_id",
                table: "order_item",
                column: "sugar_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_user_id",
                table: "order_item",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_topping_order_item_id",
                table: "order_item_topping",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_item_topping_topping_id",
                table: "order_item_topping",
                column: "topping_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "order_item_topping");

            migrationBuilder.DropTable(
                name: "order_item");

            migrationBuilder.DropTable(
                name: "group_order");
        }
    }
}

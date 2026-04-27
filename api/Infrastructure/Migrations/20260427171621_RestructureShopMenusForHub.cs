using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Drink.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestructureShopMenusForHub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 既有環境（admin_menu 有資料）才執行：
            //   1. Id=17 (覆寫設定) endpoint 改為 hub 子 tab pattern，標為 permission-only
            //   2. INSERT Id=23 (選項管理) 為 permission-only 節點
            //   3. 為 system role (id=1) 補 admin_menu_role 全 CRUD
            //   4. 重設 admin_menu sequence
            // Fresh install（admin_menu 空表）：跳過，由 AdminMenuSeeder 處理。
            migrationBuilder.Sql(@"
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM admin_menu) THEN
    UPDATE admin_menu
       SET endpoint = '/shop/[id]/overrides',
           is_permission_only = true,
           updated_at = now() AT TIME ZONE 'UTC'
     WHERE id = 17;

    INSERT INTO admin_menu
      (id, parent_id, name, icon, endpoint, sort, is_permission_only,
       created_at, creator, updated_at, updater)
    VALUES
      (23, 9, '選項管理', 'Filter', '/shop/[id]/options', 3, true,
       now() AT TIME ZONE 'UTC', 0, now() AT TIME ZONE 'UTC', 0)
    ON CONFLICT (id) DO NOTHING;

    INSERT INTO admin_menu_role
      (role_id, menu_id, can_read, can_create, can_update, can_delete,
       created_at, creator, updated_at, updater)
    VALUES
      (1, 23, true, true, true, true,
       now() AT TIME ZONE 'UTC', 0, now() AT TIME ZONE 'UTC', 0)
    ON CONFLICT (role_id, menu_id) DO NOTHING;

    PERFORM setval(
      pg_get_serial_sequence('admin_menu', 'id'),
      GREATEST((SELECT MAX(id) FROM admin_menu), 23) + 1,
      false
    );
  END IF;
END $$;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
BEGIN
  IF EXISTS (SELECT 1 FROM admin_menu) THEN
    DELETE FROM admin_menu_role WHERE menu_id = 23;
    DELETE FROM admin_menu WHERE id = 23;
    UPDATE admin_menu
       SET endpoint = '/shop/override',
           is_permission_only = false,
           updated_at = now() AT TIME ZONE 'UTC'
     WHERE id = 17;
  END IF;
END $$;
");
        }
    }
}

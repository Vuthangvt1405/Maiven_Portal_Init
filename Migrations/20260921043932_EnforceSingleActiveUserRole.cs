using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleActiveUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT [user_id]
                    FROM [USER_ROLES]
                    WHERE [isDelete] = 0
                    GROUP BY [user_id]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51000, 'Cannot enforce one active role per user because USER_ROLES contains users with multiple active role assignments.', 1;
                END;
                """);

            migrationBuilder.DropIndex(
                name: "UX_USER_ROLES_user_id_role_id",
                table: "USER_ROLES");

            migrationBuilder.CreateIndex(
                name: "UX_USER_ROLES_user_id_active",
                table: "USER_ROLES",
                column: "user_id",
                unique: true,
                filter: "[isDelete] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT [user_id], [role_id]
                    FROM [USER_ROLES]
                    GROUP BY [user_id], [role_id]
                    HAVING COUNT(*) > 1
                )
                BEGIN
                    THROW 51001, 'Cannot restore the former USER_ROLES index because duplicate user and role assignments exist.', 1;
                END;
                """);

            migrationBuilder.DropIndex(
                name: "UX_USER_ROLES_user_id_active",
                table: "USER_ROLES");

            migrationBuilder.CreateIndex(
                name: "UX_USER_ROLES_user_id_role_id",
                table: "USER_ROLES",
                columns: new[] { "user_id", "role_id" },
                unique: true);
        }
    }
}

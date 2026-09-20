using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicYearManagementConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS
                (
                    SELECT 1
                    FROM [ACADEMIC_YEARS]
                    WHERE [isDelete] = 0
                    GROUP BY [name] COLLATE SQL_Latin1_General_CP1_CI_AS
                    HAVING COUNT(*) > 1
                )
                    THROW 50010, 'Cannot add academic-year constraints: duplicate non-deleted names exist.', 1;

                IF
                (
                    SELECT COUNT(*)
                    FROM [ACADEMIC_YEARS]
                    WHERE [isDelete] = 0 AND [status] = 'ACTIVE'
                ) > 1
                    THROW 50011, 'Cannot add academic-year constraints: multiple non-deleted ACTIVE years exist.', 1;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "ACADEMIC_YEARS",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                collation: "SQL_Latin1_General_CP1_CI_AS",
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "UX_ACADEMIC_YEARS_active_not_deleted",
                table: "ACADEMIC_YEARS",
                column: "status",
                unique: true,
                filter: "[isDelete] = 0 AND [status] = 'ACTIVE'");

            migrationBuilder.CreateIndex(
                name: "UX_ACADEMIC_YEARS_name_not_deleted",
                table: "ACADEMIC_YEARS",
                column: "name",
                unique: true,
                filter: "[isDelete] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_ACADEMIC_YEARS_active_not_deleted",
                table: "ACADEMIC_YEARS");

            migrationBuilder.DropIndex(
                name: "UX_ACADEMIC_YEARS_name_not_deleted",
                table: "ACADEMIC_YEARS");

            migrationBuilder.AlterColumn<string>(
                name: "name",
                table: "ACADEMIC_YEARS",
                type: "varchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(200)",
                oldMaxLength: 200,
                oldCollation: "SQL_Latin1_General_CP1_CI_AS");
        }
    }
}

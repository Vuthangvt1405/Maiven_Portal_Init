using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class AlignStatusesWithSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_USER_ROLES_isDelete_status",
                table: "USER_ROLES");

            migrationBuilder.DropCheckConstraint(
                name: "CK_USER_ROLES_status",
                table: "USER_ROLES");

            migrationBuilder.DropIndex(
                name: "IX_SEMESTERS_isDelete_status",
                table: "SEMESTERS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SEMESTERS_status",
                table: "SEMESTERS");

            migrationBuilder.DropIndex(
                name: "IX_ROLES_isDelete_status",
                table: "ROLES");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ROLES_status",
                table: "ROLES");

            migrationBuilder.DropIndex(
                name: "IX_ENROLLMENTS_isDelete_status",
                table: "ENROLLMENTS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ENROLLMENTS_status",
                table: "ENROLLMENTS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_COURSE_SECTIONS_status",
                table: "COURSE_SECTIONS");

            migrationBuilder.DropIndex(
                name: "IX_ANNOUNCEMENTS_isDelete_status",
                table: "ANNOUNCEMENTS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ANNOUNCEMENTS_status",
                table: "ANNOUNCEMENTS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ACADEMIC_YEARS_status",
                table: "ACADEMIC_YEARS");

            migrationBuilder.DropColumn(
                name: "status",
                table: "USER_ROLES");

            migrationBuilder.DropColumn(
                name: "status",
                table: "SEMESTERS");

            migrationBuilder.DropColumn(
                name: "status",
                table: "ROLES");

            migrationBuilder.DropColumn(
                name: "status",
                table: "ENROLLMENTS");

            migrationBuilder.DropColumn(
                name: "status",
                table: "ANNOUNCEMENTS");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLES_isDelete",
                table: "USER_ROLES",
                column: "isDelete");

            migrationBuilder.CreateIndex(
                name: "IX_SEMESTERS_isDelete",
                table: "SEMESTERS",
                column: "isDelete");

            migrationBuilder.CreateIndex(
                name: "IX_ROLES_isDelete",
                table: "ROLES",
                column: "isDelete");

            migrationBuilder.CreateIndex(
                name: "IX_ENROLLMENTS_isDelete",
                table: "ENROLLMENTS",
                column: "isDelete");

            migrationBuilder.AddCheckConstraint(
                name: "CK_COURSE_SECTIONS_status",
                table: "COURSE_SECTIONS",
                sql: "[status] IN ('OPEN', 'COMPLETED', 'CANCELLED')");

            migrationBuilder.CreateIndex(
                name: "IX_ANNOUNCEMENTS_isDelete",
                table: "ANNOUNCEMENTS",
                column: "isDelete");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ACADEMIC_YEARS_status",
                table: "ACADEMIC_YEARS",
                sql: "[status] IN ('ACTIVE', 'COMPLETED')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_USER_ROLES_isDelete",
                table: "USER_ROLES");

            migrationBuilder.DropIndex(
                name: "IX_SEMESTERS_isDelete",
                table: "SEMESTERS");

            migrationBuilder.DropIndex(
                name: "IX_ROLES_isDelete",
                table: "ROLES");

            migrationBuilder.DropIndex(
                name: "IX_ENROLLMENTS_isDelete",
                table: "ENROLLMENTS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_COURSE_SECTIONS_status",
                table: "COURSE_SECTIONS");

            migrationBuilder.DropIndex(
                name: "IX_ANNOUNCEMENTS_isDelete",
                table: "ANNOUNCEMENTS");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ACADEMIC_YEARS_status",
                table: "ACADEMIC_YEARS");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "USER_ROLES",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ACTIVE");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "SEMESTERS",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ACTIVE");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "ROLES",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ACTIVE");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "ENROLLMENTS",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ENROLLED");

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "ANNOUNCEMENTS",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "DRAFT");

            migrationBuilder.UpdateData(
                table: "ROLES",
                keyColumn: "id",
                keyValue: 1L,
                column: "status",
                value: "ACTIVE");

            migrationBuilder.UpdateData(
                table: "ROLES",
                keyColumn: "id",
                keyValue: 2L,
                column: "status",
                value: "ACTIVE");

            migrationBuilder.UpdateData(
                table: "ROLES",
                keyColumn: "id",
                keyValue: 3L,
                column: "status",
                value: "ACTIVE");

            migrationBuilder.CreateIndex(
                name: "IX_USER_ROLES_isDelete_status",
                table: "USER_ROLES",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_USER_ROLES_status",
                table: "USER_ROLES",
                sql: "[status] IN ('ACTIVE', 'INACTIVE')");

            migrationBuilder.CreateIndex(
                name: "IX_SEMESTERS_isDelete_status",
                table: "SEMESTERS",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_SEMESTERS_status",
                table: "SEMESTERS",
                sql: "[status] IN ('PLANNED', 'ACTIVE', 'COMPLETED')");

            migrationBuilder.CreateIndex(
                name: "IX_ROLES_isDelete_status",
                table: "ROLES",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ROLES_status",
                table: "ROLES",
                sql: "[status] IN ('ACTIVE', 'INACTIVE')");

            migrationBuilder.CreateIndex(
                name: "IX_ENROLLMENTS_isDelete_status",
                table: "ENROLLMENTS",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ENROLLMENTS_status",
                table: "ENROLLMENTS",
                sql: "[status] IN ('ENROLLED', 'DROPPED', 'COMPLETED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_COURSE_SECTIONS_status",
                table: "COURSE_SECTIONS",
                sql: "[status] IN ('PLANNED', 'OPEN', 'CLOSED', 'COMPLETED', 'CANCELLED')");

            migrationBuilder.CreateIndex(
                name: "IX_ANNOUNCEMENTS_isDelete_status",
                table: "ANNOUNCEMENTS",
                columns: new[] { "isDelete", "status" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ANNOUNCEMENTS_status",
                table: "ANNOUNCEMENTS",
                sql: "[status] IN ('DRAFT', 'PUBLISHED', 'ARCHIVED')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ACADEMIC_YEARS_status",
                table: "ACADEMIC_YEARS",
                sql: "[status] IN ('PLANNED', 'ACTIVE', 'COMPLETED')");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class SeedStudentRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ROLES",
                columns: new[] { "id", "code", "created_at", "description", "name", "status", "updated_at" },
                values: new object[] { -1L, "STUDENT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Student self-registration role.", "Student", "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ROLES",
                keyColumn: "id",
                keyValue: -1L);
        }
    }
}

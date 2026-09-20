using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "ROLES",
                columns: new[] { "id", "code", "created_at", "description", "name", "status", "updated_at" },
                values: new object[,]
                {
                    { 1L, "STUDENT", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Student self-registration role.", "Student", "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 2L, "TEACHER", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teacher role.", "Teacher", "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { 3L, "ADMIN", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "System administrator role.", "Admin", "ACTIVE", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ROLES",
                keyColumn: "id",
                keyValue: 1L);

            migrationBuilder.DeleteData(
                table: "ROLES",
                keyColumn: "id",
                keyValue: 2L);

            migrationBuilder.DeleteData(
                table: "ROLES",
                keyColumn: "id",
                keyValue: 3L);
        }
    }
}

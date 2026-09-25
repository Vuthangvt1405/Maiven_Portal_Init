using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailSuffixWhitelist : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EMAIL_SUFFIX_WHITELIST",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    suffix = table.Column<string>(type: "varchar(253)", maxLength: 253, nullable: false),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EMAIL_SUFFIX_WHITELIST", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EMAIL_SUFFIX_WHITELIST_isDelete",
                table: "EMAIL_SUFFIX_WHITELIST",
                column: "isDelete");

            migrationBuilder.CreateIndex(
                name: "UX_EMAIL_SUFFIX_WHITELIST_suffix",
                table: "EMAIL_SUFFIX_WHITELIST",
                column: "suffix",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EMAIL_SUFFIX_WHITELIST");
        }
    }
}

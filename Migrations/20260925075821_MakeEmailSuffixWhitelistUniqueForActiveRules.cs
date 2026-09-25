using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmailSuffixWhitelistUniqueForActiveRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_EMAIL_SUFFIX_WHITELIST_suffix",
                table: "EMAIL_SUFFIX_WHITELIST");

            migrationBuilder.CreateIndex(
                name: "UX_EMAIL_SUFFIX_WHITELIST_suffix",
                table: "EMAIL_SUFFIX_WHITELIST",
                column: "suffix",
                unique: true,
                filter: "[isDelete] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_EMAIL_SUFFIX_WHITELIST_suffix",
                table: "EMAIL_SUFFIX_WHITELIST");

            migrationBuilder.CreateIndex(
                name: "UX_EMAIL_SUFFIX_WHITELIST_suffix",
                table: "EMAIL_SUFFIX_WHITELIST",
                column: "suffix",
                unique: true);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class AddPasswordResetRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PASSWORD_RESET_REQUESTS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    method = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false),
                    otp_hash = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    reset_token_hash = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    expires_at_utc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    verified_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    consumed_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    invalidated_at_utc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    otp_attempt_count = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PASSWORD_RESET_REQUESTS", x => x.id);
                    table.CheckConstraint("CK_PASSWORD_RESET_REQUESTS_method", "[method] IN ('OTP', 'FACE')");
                    table.ForeignKey(
                        name: "FK_PASSWORD_RESET_REQUESTS_USERS_user_id",
                        column: x => x.user_id,
                        principalTable: "USERS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_PASSWORD_RESET_REQUESTS_isDelete",
                table: "PASSWORD_RESET_REQUESTS",
                column: "isDelete");

            migrationBuilder.CreateIndex(
                name: "IX_PASSWORD_RESET_REQUESTS_token_hash",
                table: "PASSWORD_RESET_REQUESTS",
                column: "reset_token_hash");

            migrationBuilder.CreateIndex(
                name: "IX_PASSWORD_RESET_REQUESTS_user_method_created",
                table: "PASSWORD_RESET_REQUESTS",
                columns: new[] { "user_id", "method", "created_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PASSWORD_RESET_REQUESTS");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class AddFaceCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FACE_CREDENTIALS",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    embedding = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    embedding_dimension = table.Column<int>(type: "int", nullable: false),
                    model_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    model_version = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    deleted_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FACE_CREDENTIALS", x => x.id);
                    table.CheckConstraint("CK_FACE_CREDENTIALS_delete_state", "([isDelete] = 0 AND [embedding] IS NOT NULL AND [deleted_at] IS NULL) OR ([isDelete] = 1 AND [embedding] IS NULL AND [deleted_at] IS NOT NULL)");
                    table.CheckConstraint("CK_FACE_CREDENTIALS_embedding_dimension", "[embedding_dimension] > 0");
                    table.CheckConstraint("CK_FACE_CREDENTIALS_embedding_length", "[embedding] IS NULL OR DATALENGTH([embedding]) = [embedding_dimension] * 4");
                    table.ForeignKey(
                        name: "FK_FACE_CREDENTIALS_USERS_user_id",
                        column: x => x.user_id,
                        principalTable: "USERS",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FACE_CREDENTIALS_isDelete",
                table: "FACE_CREDENTIALS",
                column: "isDelete");

            migrationBuilder.CreateIndex(
                name: "UX_FACE_CREDENTIALS_user_id_active",
                table: "FACE_CREDENTIALS",
                column: "user_id",
                unique: true,
                filter: "[isDelete] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FACE_CREDENTIALS");
        }
    }
}

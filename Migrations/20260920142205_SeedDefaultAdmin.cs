using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Maiven_Portal_Managment.Migrations
{
    /// <inheritdoc />
    public partial class SeedDefaultAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DECLARE @AdminEmail varchar(320) = 'admin@example.com';
                DECLARE @AdminRoleId bigint = (
                    SELECT TOP (1) [id]
                    FROM [ROLES]
                    WHERE [code] = 'ADMIN' AND [isDelete] = 0
                );

                IF @AdminRoleId IS NULL
                    THROW 50001, 'The required ADMIN role is not configured.', 1;

                DECLARE @AdminUserId bigint = (
                    SELECT TOP (1) [id]
                    FROM [USERS]
                    WHERE [email] = @AdminEmail
                );

                IF @AdminUserId IS NULL
                BEGIN
                    IF EXISTS (SELECT 1 FROM [USERS] WHERE [id] = -1)
                        THROW 50002, 'Reserved default admin user ID -1 is already in use.', 1;

                    SET IDENTITY_INSERT [USERS] ON;

                    INSERT INTO [USERS]
                        ([id], [email], [password_hash], [full_name], [date_of_birth],
                         [gender], [phone], [address], [avatar_url], [isDelete],
                         [created_at], [updated_at])
                    VALUES
                        (-1, @AdminEmail,
                         'AQAAAAIAAYagAAAAEJ/KCvgl7PZUjbsaJhqZV2WObua70FgNca2WMSEVliL1z2TWqd4luAJ1eGH3a6E+Jg==',
                         'System Administrator', NULL, NULL, NULL, NULL, NULL, 0,
                         SYSUTCDATETIME(), SYSUTCDATETIME());

                    SET IDENTITY_INSERT [USERS] OFF;
                    SET @AdminUserId = -1;
                END
                ELSE
                BEGIN
                    UPDATE [USERS]
                    SET [isDelete] = 0,
                        [updated_at] = CASE
                            WHEN [isDelete] = 1 THEN SYSUTCDATETIME()
                            ELSE [updated_at]
                        END
                    WHERE [id] = @AdminUserId;
                END;

                DECLARE @AdminUserRoleId bigint = (
                    SELECT TOP (1) [id]
                    FROM [USER_ROLES]
                    WHERE [user_id] = @AdminUserId AND [role_id] = @AdminRoleId
                );

                IF @AdminUserRoleId IS NULL
                BEGIN
                    IF EXISTS (SELECT 1 FROM [USER_ROLES] WHERE [id] = -1)
                        THROW 50003, 'Reserved default admin user-role ID -1 is already in use.', 1;

                    SET IDENTITY_INSERT [USER_ROLES] ON;

                    INSERT INTO [USER_ROLES]
                        ([id], [user_id], [role_id], [isDelete], [created_at], [updated_at])
                    VALUES
                        (-1, @AdminUserId, @AdminRoleId, 0,
                         SYSUTCDATETIME(), SYSUTCDATETIME());

                    SET IDENTITY_INSERT [USER_ROLES] OFF;
                END
                ELSE
                BEGIN
                    UPDATE [USER_ROLES]
                    SET [isDelete] = 0,
                        [updated_at] = CASE
                            WHEN [isDelete] = 1 THEN SYSUTCDATETIME()
                            ELSE [updated_at]
                        END
                    WHERE [id] = @AdminUserRoleId;
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE [userRole]
                FROM [USER_ROLES] AS [userRole]
                INNER JOIN [USERS] AS [user] ON [user].[id] = [userRole].[user_id]
                INNER JOIN [ROLES] AS [role] ON [role].[id] = [userRole].[role_id]
                WHERE [userRole].[id] = -1
                  AND [user].[email] = 'admin@example.com'
                  AND [role].[code] = 'ADMIN';

                DELETE FROM [USERS]
                WHERE [id] = -1 AND [email] = 'admin@example.com';
                """);
        }
    }
}

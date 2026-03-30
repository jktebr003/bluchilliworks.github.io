using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class HardenUserSessionsForServerAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionToken",
                schema: "users",
                table: "UserSessions");

            migrationBuilder.Sql(@"
DELETE FROM users.""UserSessions""
WHERE ""Id"" NOT IN (
    '4b93dd09-a846-43c5-a01e-4ea9a68ec5e1',
    '4b93dd09-a846-43c5-a01e-4ea9a68ec5e2'
);");

            // PostgreSQL cannot auto-cast TEXT → TIMESTAMPTZ; explicit USING clause required.
            migrationBuilder.Sql("""
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "LastAccessedOn" TYPE timestamp with time zone
                    USING COALESCE("LastAccessedOn"::timestamp with time zone, TIMESTAMPTZ '-infinity');
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "LastAccessedOn" SET NOT NULL;
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "LastAccessedOn" SET DEFAULT TIMESTAMPTZ '-infinity';
                """);

            migrationBuilder.Sql("""
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "ExpiresOn" TYPE timestamp with time zone
                    USING COALESCE("ExpiresOn"::timestamp with time zone, TIMESTAMPTZ '-infinity');
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "ExpiresOn" SET NOT NULL;
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "ExpiresOn" SET DEFAULT TIMESTAMPTZ '-infinity';
                """);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "AbsoluteExpiresOn",
                schema: "users",
                table: "UserSessions",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RevokedOn",
                schema: "users",
                table: "UserSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SessionTokenHash",
                schema: "users",
                table: "UserSessions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserStateVersion",
                schema: "users",
                table: "UserSessions",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "users",
                table: "UserSessions",
                keyColumn: "Id",
                keyValue: new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e1"),
                columns: new[] { "AbsoluteExpiresOn", "ExpiresOn", "LastAccessedOn", "RevokedOn", "SessionTokenHash", "UserStateVersion" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 1, 0, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "B8C7F7B607DCCC71B797AFEB056F2FB306BAD288AFD943334C2FA97BE7AAEB82", "seed-version-1" });

            migrationBuilder.UpdateData(
                schema: "users",
                table: "UserSessions",
                keyColumn: "Id",
                keyValue: new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e2"),
                columns: new[] { "AbsoluteExpiresOn", "ExpiresOn", "LastAccessedOn", "RevokedOn", "SessionTokenHash", "UserStateVersion" },
                values: new object[] { new DateTimeOffset(new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 1, 0, 45, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), new DateTimeOffset(new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, "EC6843B0FBD9D71CB28F4AF7005812CC5A9B95C8F162445BB669F1B5CC830EC1", "seed-version-2" });

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_SessionTokenHash",
                schema: "users",
                table: "UserSessions",
                column: "SessionTokenHash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId",
                schema: "users",
                table: "UserSessions",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserSessions_SessionTokenHash",
                schema: "users",
                table: "UserSessions");

            migrationBuilder.DropIndex(
                name: "IX_UserSessions_UserId",
                schema: "users",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "AbsoluteExpiresOn",
                schema: "users",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "RevokedOn",
                schema: "users",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "SessionTokenHash",
                schema: "users",
                table: "UserSessions");

            migrationBuilder.DropColumn(
                name: "UserStateVersion",
                schema: "users",
                table: "UserSessions");

            // PostgreSQL cannot auto-cast TIMESTAMPTZ → TEXT; explicit USING clause required.
            migrationBuilder.Sql("""
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "LastAccessedOn" DROP DEFAULT;
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "LastAccessedOn" DROP NOT NULL;
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "LastAccessedOn" TYPE text
                    USING "LastAccessedOn"::text;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "ExpiresOn" DROP DEFAULT;
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "ExpiresOn" DROP NOT NULL;
                ALTER TABLE users."UserSessions"
                    ALTER COLUMN "ExpiresOn" TYPE text
                    USING "ExpiresOn"::text;
                """);

            migrationBuilder.AddColumn<string>(
                name: "SessionToken",
                schema: "users",
                table: "UserSessions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                schema: "users",
                table: "UserSessions",
                keyColumn: "Id",
                keyValue: new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e1"),
                columns: new[] { "ExpiresOn", "LastAccessedOn", "SessionToken" },
                values: new object[] { "2026-03-01T01:00:00.0000000Z", null, "sessiontoken123" });

            migrationBuilder.UpdateData(
                schema: "users",
                table: "UserSessions",
                keyColumn: "Id",
                keyValue: new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e2"),
                columns: new[] { "ExpiresOn", "LastAccessedOn", "SessionToken" },
                values: new object[] { "2026-03-01T02:00:00.0000000Z", null, "sessiontoken456" });
        }
    }
}

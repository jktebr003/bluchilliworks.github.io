using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddedUserSessionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserSessions",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SessionToken = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IdleDuration = table.Column<int>(type: "integer", nullable: false),
                    LastAccessedOn = table.Column<string>(type: "text", nullable: true),
                    ExpiresOn = table.Column<string>(type: "text", nullable: true),
                    IsExpired = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "UserSessions",
                columns: new[] { "Id", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "ExpiresOn", "IdleDuration", "IsActive", "IsExpired", "LastAccessedOn", "ModifiedBy", "ModifiedOn", "SessionToken", "UserId" },
                values: new object[,]
                {
                    { new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e1"), "Seeder", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "2026-03-01T01:00:00.0000000Z", 30, true, false, null, null, null, "sessiontoken123", "user123" },
                    { new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e2"), "Seeder", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "2026-03-01T02:00:00.0000000Z", 45, true, false, null, null, null, "sessiontoken456", "user456" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSessions",
                schema: "users");
        }
    }
}

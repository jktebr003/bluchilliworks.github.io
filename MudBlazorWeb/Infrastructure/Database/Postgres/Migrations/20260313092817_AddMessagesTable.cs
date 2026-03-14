using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddMessagesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    SentOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "text", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    LastAttemptedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Messages",
                columns: new[] { "Id", "AttemptCount", "Body", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "EmailAddress", "LastAttemptedOn", "LastErrorMessage", "MaxRetries", "ModifiedBy", "ModifiedOn", "Name", "SentOn", "Status", "Subject" },
                values: new object[,]
                {
                    { new Guid("e3425ce6-67e6-4318-b522-920f2c96fa41"), 0, "This is a test message.", "Seeder", new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "john.doe@example.com", null, null, 3, null, null, "John Doe", new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Utc), 0, "Hello" },
                    { new Guid("e3425ce6-67e6-4318-b522-920f2c96fa42"), 0, "This is another test message.", "Seeder", new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "jane.smith@example.com", null, null, 3, null, null, "Jane Smith", new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Utc), 0, "Hi" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");
        }
    }
}

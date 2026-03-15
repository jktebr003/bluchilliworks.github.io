using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddPostsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Heading = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Author = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TotalViews = table.Column<int>(type: "integer", nullable: false),
                    Likes = table.Column<List<string>>(type: "text[]", nullable: false),
                    PostedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_Posts", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Posts",
                columns: new[] { "Id", "Author", "Category", "Content", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "Description", "Heading", "Likes", "ModifiedBy", "ModifiedOn", "PostedOn", "Title", "TotalViews", "UserId" },
                values: new object[,]
                {
                    { new Guid("6a202a02-654c-4e2e-9730-a30174d5eb41"), "John Doe", "General", "This is a test message.", "Seeder", new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "This is the first post.", "First Post Heading", new List<string>(), null, null, new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), "First Post", 0, "seed-user-1" },
                    { new Guid("6a202a02-654c-4e2e-9730-a30174d5eb42"), "Jane Smith", "General", "This is another test message.", "Seeder", new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "This is the second post.", "Second Post Heading", new List<string>(), null, null, new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Second Post", 0, "seed-user-2" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Posts");
        }
    }
}

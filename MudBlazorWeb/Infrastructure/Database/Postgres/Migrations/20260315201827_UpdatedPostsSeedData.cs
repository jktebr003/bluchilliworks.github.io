using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedPostsSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("6a202a02-654c-4e2e-9730-a30174d5eb41"),
                column: "Likes",
                value: new List<string>());

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("6a202a02-654c-4e2e-9730-a30174d5eb42"),
                column: "Likes",
                value: new List<string>());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("6a202a02-654c-4e2e-9730-a30174d5eb41"),
                column: "Likes",
                value: new List<string>());

            migrationBuilder.UpdateData(
                table: "Posts",
                keyColumn: "Id",
                keyValue: new Guid("6a202a02-654c-4e2e-9730-a30174d5eb42"),
                column: "Likes",
                value: new List<string>());
        }
    }
}

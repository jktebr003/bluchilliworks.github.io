using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class SeePackagesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Packages",
                columns: new[] { "Id", "Code", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "Description", "ModifiedBy", "ModifiedOn", "Name", "Price", "Type" },
                values: new object[,]
                {
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d1"), "INDV-1E-MTH", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "1 educator", null, null, "Individual Educator", "R75.00", 1 },
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d2"), "INDV-1E-2YR", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "1 educator", null, null, "Individual Educator", "R45.00", 2 },
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d3"), "SCH-20E-MTH", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Up to 20 educators", null, null, "School Sign On", "R650.00", 1 },
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d4"), "SCH-20E-2YR", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Up to 20 educators", null, null, "School Sign On", "R425.00", 2 },
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d5"), "SCH-40E-MTH", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Up to 40 educators", null, null, "School Sign On", "R900.00", 1 },
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d6"), "SCH-40E-2YR", "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, null, "Up to 40 educators", null, null, "School Sign On", "R650.00", 2 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d1"));

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d2"));

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d3"));

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d4"));

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d5"));

            migrationBuilder.DeleteData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d6"));
        }
    }
}

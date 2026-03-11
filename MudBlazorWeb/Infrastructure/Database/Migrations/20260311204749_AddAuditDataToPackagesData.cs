using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MudBlazorWeb.Infrastructure.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditDataToPackagesData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d1"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d2"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d3"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d4"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d5"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d6"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d1"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d2"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d3"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d4"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d5"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });

            migrationBuilder.UpdateData(
                table: "Packages",
                keyColumn: "Id",
                keyValue: new Guid("10905436-1671-4900-9d0e-6fced3fbd3d6"),
                columns: new[] { "CreatedBy", "CreatedOn" },
                values: new object[] { "", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified) });
        }
    }
}

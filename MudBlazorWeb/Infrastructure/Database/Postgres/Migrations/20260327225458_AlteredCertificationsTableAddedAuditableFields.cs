using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AlteredCertificationsTableAddedAuditableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "users",
                table: "Certifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                schema: "users",
                table: "Certifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "users",
                table: "Certifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "users",
                table: "Certifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "users",
                table: "Certifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "users",
                table: "Certifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                schema: "users",
                table: "Certifications",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "users",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "users",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "users",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "users",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "users",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "users",
                table: "Certifications");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                schema: "users",
                table: "Certifications");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AlteredQualificationsTableAddedAuditableFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                schema: "users",
                table: "Qualifications",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedOn",
                schema: "users",
                table: "Qualifications",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "DeletedBy",
                schema: "users",
                table: "Qualifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedOn",
                schema: "users",
                table: "Qualifications",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "users",
                table: "Qualifications",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ModifiedBy",
                schema: "users",
                table: "Qualifications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedOn",
                schema: "users",
                table: "Qualifications",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "users",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "CreatedOn",
                schema: "users",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                schema: "users",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "DeletedOn",
                schema: "users",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "users",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "users",
                table: "Qualifications");

            migrationBuilder.DropColumn(
                name: "ModifiedOn",
                schema: "users",
                table: "Qualifications");
        }
    }
}

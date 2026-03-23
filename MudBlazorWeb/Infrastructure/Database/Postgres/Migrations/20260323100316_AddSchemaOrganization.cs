using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemaOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certification_Users_UserId",
                table: "Certification");

            migrationBuilder.DropForeignKey(
                name: "FK_Job_Users_UserId",
                table: "Job");

            migrationBuilder.DropForeignKey(
                name: "FK_Qualification_Users_UserId",
                table: "Qualification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Qualification",
                table: "Qualification");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Job",
                table: "Job");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Certification",
                table: "Certification");

            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "user");

            migrationBuilder.EnsureSchema(
                name: "system");

            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.RenameTable(
                name: "Users",
                newName: "Users",
                newSchema: "user");

            migrationBuilder.RenameTable(
                name: "Posts",
                newName: "Posts",
                newSchema: "content");

            migrationBuilder.RenameTable(
                name: "Packages",
                newName: "Packages",
                newSchema: "catalog");

            migrationBuilder.RenameTable(
                name: "Messages",
                newName: "Messages",
                newSchema: "system");

            migrationBuilder.RenameTable(
                name: "Audits",
                newName: "Audits",
                newSchema: "audit");

            migrationBuilder.RenameTable(
                name: "Qualification",
                newName: "Qualifications",
                newSchema: "user");

            migrationBuilder.RenameTable(
                name: "Job",
                newName: "Jobs",
                newSchema: "user");

            migrationBuilder.RenameTable(
                name: "Certification",
                newName: "Certifications",
                newSchema: "user");

            migrationBuilder.RenameIndex(
                name: "IX_Qualification_UserId",
                schema: "user",
                table: "Qualifications",
                newName: "IX_Qualifications_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Job_UserId",
                schema: "user",
                table: "Jobs",
                newName: "IX_Jobs_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Certification_UserId",
                schema: "user",
                table: "Certifications",
                newName: "IX_Certifications_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "user",
                table: "Qualifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Institution",
                schema: "user",
                table: "Qualifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StartDate",
                schema: "user",
                table: "Jobs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Responsibilities",
                schema: "user",
                table: "Jobs",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Position",
                schema: "user",
                table: "Jobs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EndDate",
                schema: "user",
                table: "Jobs",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Company",
                schema: "user",
                table: "Jobs",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                schema: "user",
                table: "Certifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Institution",
                schema: "user",
                table: "Certifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Qualifications",
                schema: "user",
                table: "Qualifications",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Jobs",
                schema: "user",
                table: "Jobs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Certifications",
                schema: "user",
                table: "Certifications",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Certifications_Users_UserId",
                schema: "user",
                table: "Certifications",
                column: "UserId",
                principalSchema: "user",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Jobs_Users_UserId",
                schema: "user",
                table: "Jobs",
                column: "UserId",
                principalSchema: "user",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Qualifications_Users_UserId",
                schema: "user",
                table: "Qualifications",
                column: "UserId",
                principalSchema: "user",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certifications_Users_UserId",
                schema: "user",
                table: "Certifications");

            migrationBuilder.DropForeignKey(
                name: "FK_Jobs_Users_UserId",
                schema: "user",
                table: "Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Qualifications_Users_UserId",
                schema: "user",
                table: "Qualifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Qualifications",
                schema: "user",
                table: "Qualifications");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Jobs",
                schema: "user",
                table: "Jobs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Certifications",
                schema: "user",
                table: "Certifications");

            migrationBuilder.RenameTable(
                name: "Users",
                schema: "user",
                newName: "Users");

            migrationBuilder.RenameTable(
                name: "Posts",
                schema: "content",
                newName: "Posts");

            migrationBuilder.RenameTable(
                name: "Packages",
                schema: "catalog",
                newName: "Packages");

            migrationBuilder.RenameTable(
                name: "Messages",
                schema: "system",
                newName: "Messages");

            migrationBuilder.RenameTable(
                name: "Audits",
                schema: "audit",
                newName: "Audits");

            migrationBuilder.RenameTable(
                name: "Qualifications",
                schema: "user",
                newName: "Qualification");

            migrationBuilder.RenameTable(
                name: "Jobs",
                schema: "user",
                newName: "Job");

            migrationBuilder.RenameTable(
                name: "Certifications",
                schema: "user",
                newName: "Certification");

            migrationBuilder.RenameIndex(
                name: "IX_Qualifications_UserId",
                table: "Qualification",
                newName: "IX_Qualification_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Jobs_UserId",
                table: "Job",
                newName: "IX_Job_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Certifications_UserId",
                table: "Certification",
                newName: "IX_Certification_UserId");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Qualification",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Institution",
                table: "Qualification",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StartDate",
                table: "Job",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Responsibilities",
                table: "Job",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Position",
                table: "Job",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EndDate",
                table: "Job",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Company",
                table: "Job",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Certification",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Institution",
                table: "Certification",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Qualification",
                table: "Qualification",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Job",
                table: "Job",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Certification",
                table: "Certification",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Certification_Users_UserId",
                table: "Certification",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Job_Users_UserId",
                table: "Job",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Qualification_Users_UserId",
                table: "Qualification",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

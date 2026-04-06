using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MudBlazorWeb.Infrastructure.Database.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "audit");

            migrationBuilder.EnsureSchema(
                name: "users");

            migrationBuilder.EnsureSchema(
                name: "system");

            migrationBuilder.EnsureSchema(
                name: "catalog");

            migrationBuilder.EnsureSchema(
                name: "content");

            migrationBuilder.CreateTable(
                name: "Audits",
                schema: "audit",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AuditType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AuditUser = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    KeyValues = table.Column<string>(type: "text", nullable: true),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    ChangedColumns = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Audits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Subject = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Body = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SentOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    MaxRetries = table.Column<int>(type: "integer", nullable: false),
                    LastAttemptedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastErrorMessage = table.Column<string>(type: "text", nullable: true),
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

            migrationBuilder.CreateTable(
                name: "Packages",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Price = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_Packages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                schema: "content",
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
                    Likes = table.Column<List<string>>(type: "text[]", nullable: false, defaultValueSql: "'{}'::text[]"),
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

            migrationBuilder.CreateTable(
                name: "Users",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FirstName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LastName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Username = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmailAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TelephoneNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MobileNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    HashedPassword = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    EmailVerificationToken = table.Column<string>(type: "text", nullable: true),
                    EmailVerificationTokenExpiry = table.Column<string>(type: "text", nullable: true),
                    PasswordResetToken = table.Column<string>(type: "text", nullable: true),
                    PasswordResetTokenExpiry = table.Column<string>(type: "text", nullable: true),
                    Gender = table.Column<string>(type: "text", nullable: true),
                    DateOfBirth = table.Column<string>(type: "text", nullable: true),
                    PackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    Avatar = table.Column<int>(type: "integer", nullable: false),
                    UserType = table.Column<int>(type: "integer", nullable: false),
                    Skills = table.Column<string>(type: "text", nullable: true),
                    Hobbies = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SessionTokenHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IdleDuration = table.Column<int>(type: "integer", nullable: false),
                    LastAccessedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ExpiresOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AbsoluteExpiresOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RevokedOn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    UserStateVersion = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
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

            migrationBuilder.CreateTable(
                name: "Certifications",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Institution = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Certifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Certifications_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jobs",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Company = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Position = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StartDate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EndDate = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Responsibilities = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jobs_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Qualifications",
                schema: "users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Institution = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ModifiedBy = table.Column<string>(type: "text", nullable: true),
                    DeletedOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Qualifications_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "users",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "system",
                table: "Messages",
                columns: new[] { "Id", "AttemptCount", "Body", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "EmailAddress", "LastAttemptedOn", "LastErrorMessage", "MaxRetries", "ModifiedBy", "ModifiedOn", "Name", "SentOn", "Status", "Subject" },
                values: new object[,]
                {
                    { new Guid("e3425ce6-67e6-4318-b522-920f2c96fa41"), 0, "This is a test message.", "Seeder", new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "john.doe@example.com", null, null, 3, null, null, "John Doe", new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Utc), 0, "Hello" },
                    { new Guid("e3425ce6-67e6-4318-b522-920f2c96fa42"), 0, "This is another test message.", "Seeder", new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "jane.smith@example.com", null, null, 3, null, null, "Jane Smith", new DateTime(2026, 3, 13, 0, 0, 0, 0, DateTimeKind.Utc), 0, "Hi" }
                });

            migrationBuilder.InsertData(
                schema: "catalog",
                table: "Packages",
                columns: new[] { "Id", "Code", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "Description", "ModifiedBy", "ModifiedOn", "Name", "Price", "Type" },
                values: new object[,]
                {
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d1"), "INDV-1E-MTH", "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "1 educator", null, null, "Individual Educator", "R75.00", 1 },
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d2"), "INDV-1E-2YR", "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "1 educator", null, null, "Individual Educator", "R45.00", 2 },
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d3"), "SCH-20E-MTH", "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Up to 20 educators", null, null, "School Sign On", "R650.00", 1 },
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d4"), "SCH-20E-2YR", "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Up to 20 educators", null, null, "School Sign On", "R425.00", 2 },
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d5"), "SCH-40E-MTH", "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Up to 40 educators", null, null, "School Sign On", "R900.00", 1 },
                    { new Guid("10905436-1671-4900-9d0e-6fced3fbd3d6"), "SCH-40E-2YR", "System", new DateTime(2026, 3, 11, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "Up to 40 educators", null, null, "School Sign On", "R650.00", 2 }
                });

            migrationBuilder.InsertData(
                schema: "content",
                table: "Posts",
                columns: new[] { "Id", "Author", "Category", "Content", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "Description", "Heading", "ModifiedBy", "ModifiedOn", "PostedOn", "Title", "TotalViews", "UserId" },
                values: new object[,]
                {
                    { new Guid("6a202a02-654c-4e2e-9730-a30174d5eb41"), "John Doe", "General", "This is a test message.", "Seeder", new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "This is the first post.", "First Post Heading", null, null, new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), "First Post", 0, "seed-user-1" },
                    { new Guid("6a202a02-654c-4e2e-9730-a30174d5eb42"), "Jane Smith", "General", "This is another test message.", "Seeder", new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, null, "This is the second post.", "Second Post Heading", null, null, new DateTime(2026, 3, 15, 0, 0, 0, 0, DateTimeKind.Utc), "Second Post", 0, "seed-user-2" }
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "UserSessions",
                columns: new[] { "Id", "AbsoluteExpiresOn", "CreatedBy", "CreatedOn", "DeletedBy", "DeletedOn", "ExpiresOn", "IdleDuration", "IsActive", "IsExpired", "LastAccessedOn", "ModifiedBy", "ModifiedOn", "RevokedOn", "SessionTokenHash", "UserId", "UserStateVersion" },
                values: new object[,]
                {
                    { new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e1"), new DateTimeOffset(new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Seeder", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTimeOffset(new DateTime(2026, 3, 1, 0, 30, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 30, true, false, new DateTimeOffset(new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "B8C7F7B607DCCC71B797AFEB056F2FB306BAD288AFD943334C2FA97BE7AAEB82", "user123", "seed-version-1" },
                    { new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e2"), new DateTimeOffset(new DateTime(2026, 3, 1, 8, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Seeder", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTimeOffset(new DateTime(2026, 3, 1, 0, 45, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), 45, true, false, new DateTimeOffset(new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, null, null, "EC6843B0FBD9D71CB28F4AF7005812CC5A9B95C8F162445BB669F1B5CC830EC1", "user456", "seed-version-2" }
                });

            migrationBuilder.InsertData(
                schema: "users",
                table: "Users",
                columns: new[] { "Id", "Avatar", "CreatedBy", "CreatedOn", "DateOfBirth", "DeletedBy", "DeletedOn", "EmailAddress", "EmailVerificationToken", "EmailVerificationTokenExpiry", "EmailVerified", "FirstName", "Gender", "HashedPassword", "Hobbies", "LastName", "MobileNumber", "ModifiedBy", "ModifiedOn", "Name", "PackageId", "PasswordResetToken", "PasswordResetTokenExpiry", "Skills", "TelephoneNumber", "UserType", "Username" },
                values: new object[,]
                {
                    { new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e1"), 0, "Seeder", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "johndoe@example.com", null, null, false, "John", null, "hashedpassword123", null, "Doe", null, null, null, "John Doe", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 0, "johndoe" },
                    { new Guid("4b93dd09-a846-43c5-a01e-4ea9a68ec5e2"), 0, "Seeder", new DateTime(2026, 3, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "janesmith@example.com", null, null, false, "Jane", null, "hashedpassword123", null, "Smith", null, null, null, "Jane Smith", new Guid("00000000-0000-0000-0000-000000000000"), null, null, null, null, 0, "janesmith" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Certifications_UserId",
                schema: "users",
                table: "Certifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Jobs_UserId",
                schema: "users",
                table: "Jobs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Qualifications_UserId",
                schema: "users",
                table: "Qualifications",
                column: "UserId");

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
            migrationBuilder.DropTable(
                name: "Audits",
                schema: "audit");

            migrationBuilder.DropTable(
                name: "Certifications",
                schema: "users");

            migrationBuilder.DropTable(
                name: "Jobs",
                schema: "users");

            migrationBuilder.DropTable(
                name: "Messages",
                schema: "system");

            migrationBuilder.DropTable(
                name: "Packages",
                schema: "catalog");

            migrationBuilder.DropTable(
                name: "Posts",
                schema: "content");

            migrationBuilder.DropTable(
                name: "Qualifications",
                schema: "users");

            migrationBuilder.DropTable(
                name: "UserSessions",
                schema: "users");

            migrationBuilder.DropTable(
                name: "Users",
                schema: "users");
        }
    }
}

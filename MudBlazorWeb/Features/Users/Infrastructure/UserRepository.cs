using Dapper;

using Microsoft.EntityFrameworkCore;

using MudBlazorWeb.Features.Users.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres;
using MudBlazorWeb.Shared.Enums;

namespace MudBlazorWeb.Features.Users.Infrastructure;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> FilterUsersByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                u.""Id"",
                u.""Name"",
                u.""FirstName"",
                u.""LastName"",
                u.""Username"",
                u.""EmailAddress"",
                u.""TelephoneNumber"",
                u.""MobileNumber"",
                u.""HashedPassword"",
                u.""EmailVerified"",
                u.""EmailVerificationToken"",
                u.""EmailVerificationTokenExpiry"",
                u.""PasswordResetToken"",
                u.""PasswordResetTokenExpiry"",
                u.""Gender"",
                u.""DateOfBirth"",
                u.""PackageId"",
                u.""Avatar"",
                u.""UserType"",
                u.""Skills"",
                u.""Hobbies"",
                u.""CreatedOn"",
                u.""CreatedBy"",
                u.""ModifiedOn"",
                u.""ModifiedBy"",
                u.""DeletedOn"",
                u.""DeletedBy"",
                u.""IsDeleted""
            FROM users.""Users"" u
            WHERE u.""IsDeleted"" = false AND u.""EmailAddress"" = @EmailAddress
            ORDER BY u.""CreatedOn"" DESC";

        var rows = await connection.QueryAsync<UserRow>(
            new CommandDefinition(sql, new { EmailAddress = emailAddress }, cancellationToken: cancellationToken));

        return rows.Select(MapRowToUser).ToList();
    }

    public async Task<User?> GetUserByEmailAddressAsync(string? emailAddress, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(emailAddress))
        {
            return null;
        }

        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                u.""Id"",
                u.""Name"",
                u.""FirstName"",
                u.""LastName"",
                u.""Username"",
                u.""EmailAddress"",
                u.""TelephoneNumber"",
                u.""MobileNumber"",
                u.""HashedPassword"",
                u.""EmailVerified"",
                u.""EmailVerificationToken"",
                u.""EmailVerificationTokenExpiry"",
                u.""PasswordResetToken"",
                u.""PasswordResetTokenExpiry"",
                u.""Gender"",
                u.""DateOfBirth"",
                u.""PackageId"",
                u.""Avatar"",
                u.""UserType"",
                u.""Skills"",
                u.""Hobbies"",
                u.""CreatedOn"",
                u.""CreatedBy"",
                u.""ModifiedOn"",
                u.""ModifiedBy"",
                u.""DeletedOn"",
                u.""DeletedBy"",
                u.""IsDeleted""
            FROM users.""Users"" u
            WHERE u.""IsDeleted"" = false AND u.""EmailAddress"" = @EmailAddress";

        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(sql, new { EmailAddress = emailAddress }, cancellationToken: cancellationToken));

        return row == null ? null : MapRowToUser(row);
    }

    public async Task<List<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                u.""Id"",
                u.""Name"",
                u.""FirstName"",
                u.""LastName"",
                u.""Username"",
                u.""EmailAddress"",
                u.""TelephoneNumber"",
                u.""MobileNumber"",
                u.""HashedPassword"",
                u.""EmailVerified"",
                u.""EmailVerificationToken"",
                u.""EmailVerificationTokenExpiry"",
                u.""PasswordResetToken"",
                u.""PasswordResetTokenExpiry"",
                u.""Gender"",
                u.""DateOfBirth"",
                u.""PackageId"",
                u.""Avatar"",
                u.""UserType"",
                u.""Skills"",
                u.""Hobbies"",
                u.""CreatedOn"",
                u.""CreatedBy"",
                u.""ModifiedOn"",
                u.""ModifiedBy"",
                u.""DeletedOn"",
                u.""DeletedBy"",
                u.""IsDeleted""
            FROM users.""Users"" u
            WHERE u.""IsDeleted"" = false
            ORDER BY u.""CreatedOn"" DESC";

        var rows = await connection.QueryAsync<UserRow>(
            new CommandDefinition(sql, cancellationToken: cancellationToken));

        return rows.Select(MapRowToUser).ToList();
    }

    public async Task<User> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                u.""Id"",
                u.""Name"",
                u.""FirstName"",
                u.""LastName"",
                u.""Username"",
                u.""EmailAddress"",
                u.""TelephoneNumber"",
                u.""MobileNumber"",
                u.""HashedPassword"",
                u.""EmailVerified"",
                u.""EmailVerificationToken"",
                u.""EmailVerificationTokenExpiry"",
                u.""PasswordResetToken"",
                u.""PasswordResetTokenExpiry"",
                u.""Gender"",
                u.""DateOfBirth"",
                u.""PackageId"",
                u.""Avatar"",
                u.""UserType"",
                u.""Skills"",
                u.""Hobbies"",
                u.""CreatedOn"",
                u.""CreatedBy"",
                u.""ModifiedOn"",
                u.""ModifiedBy"",
                u.""DeletedOn"",
                u.""DeletedBy"",
                u.""IsDeleted""
            FROM users.""Users"" u
            WHERE u.""Id"" = @Id AND u.""IsDeleted"" = false";

        var row = await connection.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken));

        return row == null
            ? throw new InvalidOperationException($"User with ID {id} was not found.")
            : MapRowToUser(row);
    }

    public async Task SaveUserAsync(User user, CancellationToken cancellationToken = default)
    {
        await _context.Set<User>().AddAsync(user, cancellationToken);
        _context.SaveChanges(user.CreatedBy, cancellationToken);
    }

    public async Task UpdateUserAsync(User user, CancellationToken cancellationToken = default)
    {
        var trackedUser = await _context.Set<User>()
            .AsTracking()
            .SingleOrDefaultAsync(existing => existing.Id == user.Id, cancellationToken);

        if (trackedUser == null)
        {
            throw new InvalidOperationException($"User with ID {user.Id} was not found.");
        }

        var currentRecord = _context.Set<User>().Entry(trackedUser);
        currentRecord.CurrentValues.SetValues(user);

        _context.SaveChanges(user.ModifiedBy ?? "System", cancellationToken);
    }

    public async Task<List<User>> SearchUsersAsync(
        string? search = null,
        UserType? role = null,
        string? gender = null,
        string? package = null,
        bool? emailVerified = null,
        DateTime? dobFrom = null,
        DateTime? dobTo = null,
        CancellationToken cancellationToken = default)
    {
        var connection = _context.Database.GetDbConnection();

        var sql = @"
            SELECT
                u.""Id"",
                u.""Name"",
                u.""FirstName"",
                u.""LastName"",
                u.""Username"",
                u.""EmailAddress"",
                u.""TelephoneNumber"",
                u.""MobileNumber"",
                u.""HashedPassword"",
                u.""EmailVerified"",
                u.""EmailVerificationToken"",
                u.""EmailVerificationTokenExpiry"",
                u.""PasswordResetToken"",
                u.""PasswordResetTokenExpiry"",
                u.""Gender"",
                u.""DateOfBirth"",
                u.""PackageId"",
                u.""Avatar"",
                u.""UserType"",
                u.""Skills"",
                u.""Hobbies"",
                u.""CreatedOn"",
                u.""CreatedBy"",
                u.""ModifiedOn"",
                u.""ModifiedBy"",
                u.""DeletedOn"",
                u.""DeletedBy"",
                u.""IsDeleted""
                        FROM users.""Users"" u
                        LEFT JOIN catalog.""Packages"" p ON p.""Id"" = u.""PackageId""
            WHERE u.""IsDeleted"" = false
              AND (@Search IS NULL
                    OR u.""Name"" ILIKE @SearchPattern
                    OR u.""FirstName"" ILIKE @SearchPattern
                    OR u.""LastName"" ILIKE @SearchPattern
                    OR u.""Username"" ILIKE @SearchPattern
                    OR u.""EmailAddress"" ILIKE @SearchPattern
                    OR COALESCE(u.""MobileNumber"", '') ILIKE @SearchPattern)
              AND (@Role IS NULL OR u.""UserType"" = @Role)
              AND (@Gender IS NULL OR u.""Gender"" ILIKE @Gender)
              AND (@Package IS NULL
                    OR COALESCE(p.""Name"", '') ILIKE @PackagePattern
                    OR CAST(u.""PackageId"" AS text) ILIKE @PackagePattern)
              AND (@EmailVerified IS NULL OR u.""EmailVerified"" = @EmailVerified)
              AND (@DobFrom IS NULL
                    OR (u.""DateOfBirth"" ~ '^\\d{4}-\\d{2}-\\d{2}$' AND u.""DateOfBirth""::date >= @DobFrom))
              AND (@DobTo IS NULL
                    OR (u.""DateOfBirth"" ~ '^\\d{4}-\\d{2}-\\d{2}$' AND u.""DateOfBirth""::date <= @DobTo))
            ORDER BY u.""CreatedOn"" DESC";

        var parameters = new DynamicParameters();
        parameters.Add("Search", string.IsNullOrWhiteSpace(search) ? null : search);
        parameters.Add("SearchPattern", string.IsNullOrWhiteSpace(search) ? null : $"%{search}%");
        parameters.Add("Role", role.HasValue ? (int)role.Value : null);
        parameters.Add("Gender", string.IsNullOrWhiteSpace(gender) ? null : gender);
        parameters.Add("Package", string.IsNullOrWhiteSpace(package) ? null : package);
        parameters.Add("PackagePattern", string.IsNullOrWhiteSpace(package) ? null : $"%{package}%");
        parameters.Add("EmailVerified", emailVerified);
        parameters.Add("DobFrom", dobFrom?.Date);
        parameters.Add("DobTo", dobTo?.Date);

        var rows = await connection.QueryAsync<UserRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        return rows.Select(MapRowToUser).ToList();
    }

    private static User MapRowToUser(UserRow row)
    {
        return new User
        {
            Id = row.Id,
            Name = row.Name,
            FirstName = row.FirstName,
            LastName = row.LastName,
            Username = row.Username,
            EmailAddress = row.EmailAddress,
            TelephoneNumber = row.TelephoneNumber,
            MobileNumber = row.MobileNumber,
            HashedPassword = row.HashedPassword,
            EmailVerified = row.EmailVerified,
            EmailVerificationToken = row.EmailVerificationToken,
            EmailVerificationTokenExpiry = row.EmailVerificationTokenExpiry,
            PasswordResetToken = row.PasswordResetToken,
            PasswordResetTokenExpiry = row.PasswordResetTokenExpiry,
            Gender = row.Gender,
            DateOfBirth = row.DateOfBirth,
            PackageId = row.PackageId,
            Avatar = row.Avatar,
            UserType = row.UserType,
            Skills = row.Skills,
            Hobbies = row.Hobbies,
            CreatedOn = row.CreatedOn,
            CreatedBy = row.CreatedBy,
            ModifiedOn = row.ModifiedOn,
            ModifiedBy = row.ModifiedBy,
            DeletedOn = row.DeletedOn,
            DeletedBy = row.DeletedBy,
            IsDeleted = row.IsDeleted
        };
    }

    private sealed class UserRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string EmailAddress { get; set; } = string.Empty;
        public string? TelephoneNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string HashedPassword { get; set; } = string.Empty;
        public bool EmailVerified { get; set; }
        public string? EmailVerificationToken { get; set; }
        public string? EmailVerificationTokenExpiry { get; set; }
        public string? PasswordResetToken { get; set; }
        public string? PasswordResetTokenExpiry { get; set; }
        public string? Gender { get; set; }
        public string? DateOfBirth { get; set; }
        public Guid PackageId { get; set; }
        public int Avatar { get; set; }
        public int UserType { get; set; }
        public string? Skills { get; set; }
        public string? Hobbies { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? ModifiedOn { get; set; }
        public string? ModifiedBy { get; set; }
        public DateTime? DeletedOn { get; set; }
        public string? DeletedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}

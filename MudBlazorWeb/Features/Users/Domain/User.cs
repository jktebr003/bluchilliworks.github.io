using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using MudBlazorWeb.Features.Pricing.Domain;
using MudBlazorWeb.Infrastructure.Database.Postgres.Common;

namespace MudBlazorWeb.Features.Users.Domain;

public class User : BaseAuditableEntity
{
    [Key]
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Username { get; set; }
    public string EmailAddress { get; set; }
    public string? TelephoneNumber { get; set; }
    public string? MobileNumber { get; set; }
    public string HashedPassword { get; set; }
    public bool EmailVerified { get; set; } = false;
    public string? EmailVerificationToken { get; set; }
    public string? EmailVerificationTokenExpiry { get; set; }
    public string? PasswordResetToken { get; set; }
    public string? PasswordResetTokenExpiry { get; set; }
    public string? Gender { get; set; }
    public string? DateOfBirth { get; set; }
    // public Package? Package { get; set; }
    public Guid PackageId { get; set; }
    public List<Job>? Jobs { get; set; } = null;
    public List<Qualification>? Qualifications { get; set; } = null;
    public List<Certification>? Certifications { get; set; } = null;
    public int Avatar { get; set; }
    public int UserType { get; set; }
    public string? Skills { get; set; }
    public string? Hobbies { get; set; }
}

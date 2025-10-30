using Shared.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class UpdateUserRequest
{
    public string Id { get; set; }
    public string? Name { get; set; } = null;

    public string? FirstName { get; set; } = null;

    public string? LastName { get; set; } = null;

    public string? Username { get; set; } = null;

    public string? EmailAddress { get; set; } = null;

    public string? TelephoneNumber { get; set; }

    public string? MobileNumber { get; set; }

    public string? EncryptedPassword { get; set; } = null;

    public string? DecryptedPassword { get; set; } = null;

    public string? Gender { get; set; } = null;

    public string? DateOfBirth { get; set; } = null;

    public string? PackageId { get; set; }

    public IEnumerable<JobResponse>? Jobs { get; set; } = null;

    public IEnumerable<QualificationResponse>? Qualifications { get; set; } = null;

    public IEnumerable<CertificationResponse>? Certifications { get; set; } = null;
    public UserType UserType { get; set; }

    public int? Avatar { get; set; } = 17;

    public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;

    public string ModifiedBy { get; set; } = "system";
}

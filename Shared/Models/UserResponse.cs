using Shared.Enums;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class UserResponse : BaseResponse
{
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

    public PackageResponse? Package { get; set; }

    public IEnumerable<JobResponse>? Jobs { get; set; } = null;

    public IEnumerable<QualificationResponse>? Qualifications { get; set; } = null;

    public IEnumerable<CertificationResponse>? Certifications { get; set; } = null;

    //public AvatarType Avatar { get; set; }

    public UserType UserRole { get; set; }
}

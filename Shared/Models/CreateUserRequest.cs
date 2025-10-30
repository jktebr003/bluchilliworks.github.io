using Shared.Enums;
using Shared.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class CreateUserRequest
{
    public string? Name { get; set; } = null;
    public string? FirstName { get; set; } = null;
    public string? LastName { get; set; } = null;
    public string? Username { get; set; } = null;
    public string? EmailAddress { get; set; } = null;
    public string? EncryptedPassword { get; set; } = null;
    public string? DecryptedPassword { get; set; } = null;
    public string? PackageId { get; set; }
    public int? UserType { get; set; }
    public int? Avatar { get; set; }
    public DateTime CreatedOn { get; set; } = DateTimeExtension.GetSouthAfricanTime();
    public string CreatedBy { get; set; } = "system";
}

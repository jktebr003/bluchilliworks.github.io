using Shared.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class UpdateUserSessionRequest
{
    public string? UserId { get; set; }
    public string? SessionKey { get; set; }
    public string? LastAccessedOn { get; set; }
    public bool IsExpired { get; set; }
    public DateTime ModifiedOn { get; set; } = DateTimeExtension.GetSouthAfricanTime();
    public string ModifiedBy { get; set; } = "system";
}

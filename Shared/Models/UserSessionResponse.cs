using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models;

public class UserSessionResponse : BaseResponse
{
    // <summary>
    /// The User Id
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// The Session Value
    /// </summary>
    public string? SessionToken { get; set; }

    /// <summary>
    /// The Session Idle Duration
    /// </summary>
    public int IdleDuration { get; set; }

    /// <summary>
    /// The Last Accessed Date
    /// </summary>
    public string? LastAccessedOn { get; set; }

    /// <summary>
    /// The Expires On Date
    /// </summary>
    public string? ExpiresOn { get; set; }

    /// <summary>
    /// Is Session Expired
    /// </summary>
    public bool IsExpired { get; set; }

    /// <summary>
    /// Is Session Active
    /// </summary>
    public bool IsActive { get; set; }
}

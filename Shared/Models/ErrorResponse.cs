using Shared.Enums;

namespace Shared.Models;

public class ErrorResponse
{
    /// <summary>
    /// Creates a <see cref="ErrorResponse"/> instance.
    /// </summary>
    public ErrorResponse() : this(String.Empty, String.Empty, Severity.None)
    { }

    /// <summary>
    /// Creates a <see cref="ErrorResponse"/> instance.
    /// </summary>
    /// <param name="code"></param>
    /// <param name="description"></param>
    /// <param name="severity"></param>
    public ErrorResponse(string code, string description, Severity severity)
    {
        Code = code;
        Description = description;
        Severity = severity;
    }

    /// <summary>
    /// The Error Code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// The Error Description
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// The Error Severity
    /// </summary>
    public Severity Severity { get; set; }
}

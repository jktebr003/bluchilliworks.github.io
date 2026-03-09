using MudBlazorWeb.Features.Pricing.Application;

namespace MudBlazorWeb.Shared;

public class Response<T>
{
    public bool Success { get; set; }
    public string Code { get; set; }
    public string Message { get; set; }
    public T Value { get; set; }
}

public record Result<T>(T Value, bool Success, string? Code = null, string? Message = null);

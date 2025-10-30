namespace Api.Filters;

public class AuthenticationFilter : IEndpointFilter
{
    #region ===[ Private Members ]=============================================================

    private readonly string _apiKey;
    private readonly string _apiKeySecondary;
    private readonly bool _canUseSecondaryApiKey;

    #endregion

    #region ===[ Constructor ]=================================================================

    public AuthenticationFilter(IConfiguration configuration)
    {
        _apiKey = $"{configuration["SecretKeys:ApiKey"]}";
        _apiKeySecondary = $"{configuration["SecretKeys:ApiKeySecondary"]}";
        _canUseSecondaryApiKey = Convert.ToBoolean($"{configuration["SecretKeys:UseSecondaryKey"]}");
    }

    #endregion

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var apiKeyHeader = context.HttpContext.Request.Headers["Authorization"].ToString();
        //var authController = new Controllers.AuthController();

        if (apiKeyHeader.Any())
        {
            var keys = new List<string>
            {
                _apiKey
            };

            if (_canUseSecondaryApiKey)
            {
                keys.AddRange(_apiKeySecondary.Split(','));
            }

            if (keys.FindIndex(x => x.Equals(apiKeyHeader, StringComparison.OrdinalIgnoreCase)) == -1)
            {
                return Results.Unauthorized();
            }
        }
        else
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}

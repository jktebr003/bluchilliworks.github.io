using MudBlazorWeb.Features.Authentication.Application;

namespace MudBlazorWeb.Features.Authentication.Infrastructure;

internal sealed class SmtpEmailSender : IEmailSender
{
    private readonly IConfiguration _configuration;

    public SmtpEmailSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<bool> SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        => AuthenticationCommandHelpers.TrySendEmailAsync(_configuration, to, subject, body, cancellationToken);
}

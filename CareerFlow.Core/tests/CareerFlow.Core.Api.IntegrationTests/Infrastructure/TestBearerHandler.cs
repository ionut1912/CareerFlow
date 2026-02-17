using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace CareerFlow.Core.Api.IntegrationTests.Infrastructure;

public sealed class TestBearerHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestBearer";
    private readonly TestUserContext _ctx;

    public TestBearerHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        TestUserContext ctx)
        : base(options, logger, encoder) => _ctx = ctx;

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Guid.Empty means the client is anonymous — let [Authorize] reject it.
        if (_ctx.AccountId == Guid.Empty)
            return Task.FromResult(AuthenticateResult.NoResult());

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, _ctx.AccountId.ToString()),
            new Claim("sub",                     _ctx.AccountId.ToString()),
            new Claim("accountId",               _ctx.AccountId.ToString()),
        };

        var ticket = new AuthenticationTicket(
            new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName)),
            SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
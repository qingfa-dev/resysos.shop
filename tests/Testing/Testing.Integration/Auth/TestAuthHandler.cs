using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Testing.Integration.Auth;

/// <summary>
/// Authenticates every request as a test user. Override the user id with the
/// <c>X-Test-User</c> header when a test needs a specific identity.
/// </summary>
public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>The authentication scheme registered by <see cref="Hosting.ApiFactory{TProgram}"/>.</summary>
    public const string SchemeName = "Test";

    /// <summary>Header that overrides the authenticated user id.</summary>
    public const string UserIdHeader = "X-Test-User";

    /// <summary>User id used when the header is absent.</summary>
    public const string DefaultUserId = "test-user";

    /// <summary>Creates the handler.</summary>
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string userId = Request.Headers.TryGetValue(UserIdHeader, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value.ToString()
            : DefaultUserId;

        Claim[] claims =
        [
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userId),
        ];

        ClaimsPrincipal principal = new(new ClaimsIdentity(claims, SchemeName));

        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}

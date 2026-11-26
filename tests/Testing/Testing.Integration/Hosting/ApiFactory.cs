using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testing.Integration.Auth;

namespace Testing.Integration.Hosting;

/// <summary>
/// Boots the API composition root in memory and swaps authentication for
/// <see cref="TestAuthHandler"/> so functional tests never need real tokens.
/// </summary>
/// <typeparam name="TProgram">The entry point of the host under test.</typeparam>
public class ApiFactory<TProgram> : WebApplicationFactory<TProgram>
    where TProgram : class
{
    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, static _ => { });
        });
    }
}

using Distech.CloudRelay.API.FunctionalTests.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Distech.CloudRelay.API.FunctionalTests
{
    public class CloudRelayAPIApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        bool m_Disposed = false;

        protected override void Dispose(bool disposing)
        {
            if (!m_Disposed && disposing)
            {
                m_Disposed = true;
                
                // dispose any additional resources here
            }

            base.Dispose(disposing);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot(Path.Combine("src", "Distech.CloudRelay.API"));
            builder.ConfigureServices(services =>
            {
                services.AddTransient<IStartupFilter, IdentityMiddlewareFilter>();
            });
        }

        class IdentityMiddlewareFilter
            : IStartupFilter
        {
            private readonly IConfiguration m_Configuration;

            public IdentityMiddlewareFilter(IConfiguration configuration)
            {
                m_Configuration = configuration;
            }

            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
            {
                var configuration = m_Configuration;
                return builder =>
                {
                    if (configuration.GetValue(TestEnvironment.UseIdentity, false))
                    {
                        builder.UseMiddleware<IdentityMiddleware>();
                    }

                    next(builder);
                };

            }
        }
    }
}

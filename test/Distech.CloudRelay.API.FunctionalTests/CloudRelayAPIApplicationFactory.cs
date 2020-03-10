using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
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

        protected override IWebHostBuilder CreateWebHostBuilder()
        {
            return WebHost.CreateDefaultBuilder<TStartup>(null);
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseSolutionRelativeContentRoot(Path.Combine("src", "Distech.CloudRelay.API"));
        }
    }
}

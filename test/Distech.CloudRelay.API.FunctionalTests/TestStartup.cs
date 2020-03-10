using Distech.CloudRelay.API.FunctionalTests.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.FunctionalTests
{
    public class TestStartup : Startup
    {
        public TestStartup(IConfiguration configuration)
           : base(configuration)
        {
        }

        protected override void ConfigureAuthentication(IApplicationBuilder app)
        {
            base.ConfigureAuthentication(app);

            if (Configuration.GetValue(TestEnvironment.UseIdentity, false))
                app.UseMiddleware<IdentityMiddleware>();
        }
    }
}

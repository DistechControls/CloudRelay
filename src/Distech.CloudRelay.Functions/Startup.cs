using Distech.CloudRelay.Functions.Options;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

[assembly: FunctionsStartup(typeof(Distech.CloudRelay.Functions.Startup))]

namespace Distech.CloudRelay.Functions
{
    class Startup
        : FunctionsStartup
    {
        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            // add user secrets configuration provider
            builder.ConfigurationBuilder.AddUserSecrets<Startup>();

            base.ConfigureAppConfiguration(builder);
        }

        public override void Configure(IFunctionsHostBuilder builder)
        {
            // inject common services
            builder.Services.AddCloudRelayFileService((services, options) =>
            {
                var configuration = services.GetRequiredService<IConfiguration>();
                configuration.GetSection(FunctionAppEnvironment.FileStorageSectionKey).Bind(options);
            });
            builder.Services.AddOptions<BlobCleanupOptions>()
                .Configure<IConfiguration>((options, configuration) =>
                {
                    if (uint.TryParse(configuration[FunctionAppEnvironment.BlobCleanupExpirationDelayKey], out uint delay))
                    {
                        options.MinutesExpirationDelay = delay;
                    }
                });
        }
    }
}

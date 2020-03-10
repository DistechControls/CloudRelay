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
        public override void Configure(IFunctionsHostBuilder builder)
        {
            // register additional configuration providers
            // official support is under investigation: https://github.com/Azure/azure-functions-host/issues/5274
            AddConfigurations(builder);
            
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

        private void AddConfigurations(IFunctionsHostBuilder builder)
        {
            // keep track of all configuration providers prior to remove the existing IConfiguration service
            var providers = new List<IConfigurationProvider>();

            foreach (var descriptor in builder.Services.Where(descriptor => descriptor.ServiceType == typeof(IConfiguration)).ToList())
            {
                var existingConfiguration = descriptor.ImplementationInstance as IConfigurationRoot;

                if (existingConfiguration is null)
                    continue;

                providers.AddRange(existingConfiguration.Providers);

                builder.Services.Remove(descriptor);
            }

            // add custom configurations to builder
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddUserSecrets<Startup>();

            // build custom configurations and add related providers to existing collection
            providers.AddRange(configBuilder.Build().Providers);

            // register the IConfiguration service using the updated list of providers
            builder.Services.AddSingleton<IConfiguration>(new ConfigurationRoot(providers));
        }
    }
}

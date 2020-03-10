using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods used to setup DI container.
    /// </summary>
    public static class CommonServiceCollectionExtension
    {
        public static IServiceCollection AddCloudRelayFileService(this IServiceCollection services, Action<IServiceProvider, Distech.CloudRelay.Common.Options.FileStorageOptions> setupAction)
        {
            services.AddOptions<Distech.CloudRelay.Common.Options.FileStorageOptions>()
                .Configure<IServiceProvider>((options, provider) => setupAction(provider, options));
            services.ConfigureOptions<Distech.CloudRelay.Common.Options.FileStoragePostConfigureOptions>();
            services.AddScoped<Distech.CloudRelay.Common.DAL.IBlobRepository, Distech.CloudRelay.Common.DAL.AzureStorageBlobRepository>();
            services.AddScoped<Distech.CloudRelay.Common.Services.IFileService, Distech.CloudRelay.Common.Services.FileService>();

            return services;
        }
    }
}

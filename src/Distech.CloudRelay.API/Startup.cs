using Distech.CloudRelay.API.Middleware;
using Distech.CloudRelay.API.Model;
using Distech.CloudRelay.API.Options;
using Distech.CloudRelay.API.Services;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureAD.UI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Linq;
using System.Reflection;

namespace Distech.CloudRelay.API
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureDevelopmentServices(IServiceCollection services)
        {
            //add diagnostics support for the JwtBearer middleware
            services.PostConfigure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, options =>
            {
                options.Events = JwtBearerMiddlewareDiagnostics.Subscribe(options.Events);
            });

            ConfigureServices(services);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            //inject authentication related entities and services
            services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
                    .AddAzureADBearer(options => Configuration.Bind(ApiEnvironment.AzureADOptions, options));

            //inject common services
            services.AddCloudRelayFileService((serviceProvider, options) =>
            {
                Configuration.GetSection(ApiEnvironment.FileStorageSectionKey).Bind(options);
               
                var adapterOptions = serviceProvider.GetService<IOptionsSnapshot<AzureIotHubAdapterOptions>>();
                options.ConnectionString = ApiEnvironment.GetConnectionString(ApiEnvironment.ResourceType.StorageAccount, adapterOptions.Value.EnvironmentId, Configuration);

                // files uploaded by the server are stored in a dedicated sub folder
                options.ServerFileUploadSubFolder = adapterOptions.Value.MethodName;
            });

            //inject local services
            services.AddScoped<IDeviceCommunicationAdapter, AzureIotHubAdapter>();
            services.AddScoped<IDeviceService, DeviceService>();
            services.AddScoped<ServiceClient>(serviceProvider =>
            {
                var options = serviceProvider.GetService<IOptionsSnapshot<AzureIotHubAdapterOptions>>();
                var connectionString = ApiEnvironment.GetConnectionString(ApiEnvironment.ResourceType.IoTHubService, options.Value.EnvironmentId, Configuration);
                return ServiceClient.CreateFromConnectionString(connectionString);
            });
            services.ConfigureOptions<AzureIotHubAdapterPostConfigureOptions>();

            //enables Application Insights telemetry (APPINSIGHTS_INSTRUMENTATIONKEY and ApplicationInsights:InstrumentationKey are supported)
            services.AddApplicationInsightsTelemetry(options =>
            {
                options.ApplicationVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
            });
            services.AddApplicationInsightsTelemetryProcessor<ExpectedExceptionTelemetryProcessor>();
            services.AddSingleton<ITelemetryInitializer, RequestHeadersTelemetryInitializer>();

            //enables CORS using the default policy
            services.AddCors((options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(HeaderNames.ContentDisposition);
                });
            }));

            services.AddMvc(opts =>
            {
                // 'AzureAD' is currently the only supported authentication provider name and anything else will allow anonymous access
                if (IdentityProviders.AzureActiveDirectory.Equals(Configuration.GetSection(ApiEnvironment.AuthenticationProvider).Value, StringComparison.OrdinalIgnoreCase))
                {
                    AuthorizationPolicy policy;

                    string[] roles = Configuration.GetSection(ApiEnvironment.AzureADRoles).GetChildren().Select(role => role.Value).ToArray();
                    string[] scopes = Configuration.GetSection(ApiEnvironment.AzureADScopes).GetChildren().Select(role => role.Value).ToArray();

                    if (roles.Count() > 0)
                    {
                        // authentication + authorization based on application permissions aka roles
                        policy = new AuthorizationPolicyBuilder().RequireRole(roles).Build();
                    }
                    else if (scopes.Count() > 0)
                    {
                        // authentication + authorization based on delegated permissions aka scopes
                        policy = new AuthorizationPolicyBuilder().RequireAssertion((ctx =>
                        {
                            var scopeClaim = ctx.User.FindFirst(c => c.Type == "http://schemas.microsoft.com/identity/claims/scope")?.Value.Split(' ');
                            return scopeClaim != null && scopes.All(s => scopeClaim.Contains(s));
                        }))
                        .Build();
                    }
                    else
                    {
                        // authentication only
                        policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
                    }

                    opts.Filters.Add(new AuthorizeFilter(policy));
                }
            })
            .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            //allow accessing the HttpContext inside a service class
            services.AddHttpContextAccessor();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors();
            app.UseHttpsRedirection();
            ConfigureAuthentication(app);

            app.UseApplicationErrorHandler();
            app.UseMultiPartRequestBuffering();

            app.UseMvc();
        }

        protected virtual void ConfigureAuthentication(IApplicationBuilder app)
        {
            app.UseAuthentication();
        }
    }
}

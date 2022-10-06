using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Distech.CloudRelay.API;
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
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Azure.Devices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);

//inject authentication related entities and services
#pragma warning disable 0618 //requires breaking changes to replace with Microsoft.Identity.Web
builder.Services.AddAuthentication(AzureADDefaults.BearerAuthenticationScheme)
#pragma warning restore 0618
    .AddAzureADBearer(options => builder.Configuration.Bind(ApiEnvironment.AzureADOptions, options));

if (builder.Environment.IsDevelopment())
{
    //add diagnostics support for the JwtBearer middleware
#pragma warning disable 0618 //requires breaking changes to replace with Microsoft.Identity.Web
    builder.Services.PostConfigure<JwtBearerOptions>(AzureADDefaults.JwtBearerAuthenticationScheme, static options =>
#pragma warning restore 0618
    {
        options.Events = JwtBearerMiddlewareDiagnostics.Subscribe(options.Events);
    });
}

//inject common services
builder.Services.AddCloudRelayFileService(static (serviceProvider, options) =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    config.GetSection(ApiEnvironment.FileStorageSectionKey).Bind(options);

    var adapterOptions = serviceProvider.GetService<IOptionsSnapshot<AzureIotHubAdapterOptions>>();
    options.ConnectionString = ApiEnvironment.GetConnectionString(ApiEnvironment.ResourceType.StorageAccount, adapterOptions.Value.EnvironmentId, config);

    // files uploaded by the server are stored in a dedicated sub folder
    options.ServerFileUploadSubFolder = adapterOptions.Value.MethodName;
});

//inject local services
builder.Services.AddScoped<IDeviceCommunicationAdapter, AzureIotHubAdapter>();
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<ServiceClient>(static serviceProvider =>
{
    var config = serviceProvider.GetRequiredService<IConfiguration>();
    var options = serviceProvider.GetService<IOptionsSnapshot<AzureIotHubAdapterOptions>>();
    var connectionString = ApiEnvironment.GetConnectionString(ApiEnvironment.ResourceType.IoTHubService, options.Value.EnvironmentId, config);
    return ServiceClient.CreateFromConnectionString(connectionString);
});
builder.Services.ConfigureOptions<AzureIotHubAdapterPostConfigureOptions>();

//enables Application Insights telemetry (APPINSIGHTS_INSTRUMENTATIONKEY and ApplicationInsights:InstrumentationKey are supported)
builder.Services.AddApplicationInsightsTelemetry(static options =>
{
    options.ApplicationVersion = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
});
builder.Services.AddApplicationInsightsTelemetryProcessor<ExpectedExceptionTelemetryProcessor>();
builder.Services.AddSingleton<ITelemetryInitializer, RequestHeadersTelemetryInitializer>();

//enables CORS using the default policy
builder.Services.AddCors(static options =>
{
    options.AddDefaultPolicy(static corsBuilder =>
    {
        corsBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(HeaderNames.ContentDisposition);
    });
});

//allow accessing the HttpContext inside a service class
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers(configure =>
{
    // 'AzureAD' is currently the only supported authentication provider name and anything else will allow anonymous access
    if (IdentityProviders.AzureActiveDirectory.Equals(builder.Configuration.GetSection(ApiEnvironment.AuthenticationProvider).Value, StringComparison.OrdinalIgnoreCase))
    {
        AuthorizationPolicy policy;

        string[] roles = builder.Configuration.GetSection(ApiEnvironment.AzureADRoles).GetChildren().Select(role => role.Value).ToArray();
        string[] scopes = builder.Configuration.GetSection(ApiEnvironment.AzureADScopes).GetChildren().Select(role => role.Value).ToArray();

        if (roles.Length > 0)
        {
            // authentication + authorization based on application permissions aka roles
            policy = new AuthorizationPolicyBuilder().RequireRole(roles).Build();
        }
        else if (scopes.Length > 0)
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

        configure.Filters.Add(new AuthorizeFilter(policy));
    }
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseCors();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseApplicationErrorHandler();
app.UseMultiPartRequestBuffering();

app.MapControllers();

app.Run();

/// <summary>
/// Intended for test projects using WebApplicationFactory.
/// </summary>
public partial class Program { }

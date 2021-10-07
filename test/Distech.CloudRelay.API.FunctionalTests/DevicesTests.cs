using Distech.CloudRelay.API.Model;
using Distech.CloudRelay.API.Services;
using Distech.CloudRelay.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Distech.CloudRelay.API.FunctionalTests
{
    public class DevicesControllerTests : IClassFixture<CloudRelayAPIApplicationFactory<TestStartup>>
    {
        #region Member Variables

        private readonly CloudRelayAPIApplicationFactory<TestStartup> m_ApiAppFactory;

        #endregion

        #region Constructors

        public DevicesControllerTests(CloudRelayAPIApplicationFactory<TestStartup> apiAppFactory)
        {
            m_ApiAppFactory = apiAppFactory;
        }

        #endregion

        #region GET

        [Fact]
        public async Task GetDeviceRequest_HappyPath_ReturnsOk()
        {
            //Arrange
            string deviceId = nameof(GetDeviceRequest_HappyPath_ReturnsOk);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ReturnsAsync(new InvocationResult(StatusCodes.Status200OK, GetMockedResponse(StatusCodes.Status200OK, new object())));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });

            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.GetAsync($"api/v1/devices/{deviceId}/request");

            //Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task GetDeviceRequest_DeviceNotFound_ReturnsNotFound()
        {
            //Arrange
            string deviceId = nameof(GetDeviceRequest_DeviceNotFound_ReturnsNotFound);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ThrowsAsync(new IdNotFoundException(ErrorCodes.DeviceNotFound, deviceId));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.GetAsync($"api/v1/devices/{deviceId}/request");
            var content = await response.Content.ReadAsStringAsync();
            ApplicationProblemDetails problem = JsonConvert.DeserializeObject<ApplicationProblemDetails>(content);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorCodes.DeviceNotFound, problem.ErrorCode);
        }

        [Fact]
        public async Task GetDeviceRequest_EnvironmentNotFound_ReturnsNotFound()
        {
            //Arrange
            string deviceId = nameof(GetDeviceRequest_EnvironmentNotFound_ReturnsNotFound);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ThrowsAsync(new IdNotFoundException(ErrorCodes.EnvironmentNotFound, deviceId));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.GetAsync($"api/v1/devices/{deviceId}/request?environmentId=someId");
            var content = await response.Content.ReadAsStringAsync();
            ApplicationProblemDetails problem = JsonConvert.DeserializeObject<ApplicationProblemDetails>(content);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorCodes.EnvironmentNotFound, problem.ErrorCode);
        }

        #endregion

        #region POST

        [Fact]
        public async Task PostDeviceRequest_HappyPath_ReturnsOk()
        {
            //Arrange
            string deviceId = nameof(PostDeviceRequest_HappyPath_ReturnsOk);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ReturnsAsync(new InvocationResult(StatusCodes.Status200OK, GetMockedResponse(StatusCodes.Status200OK, new object[] { })));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.PostAsJsonAsync($"api/v1/devices/{deviceId}/request", default(string));

            //Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task PostDeviceRequest_DeviceNotFound_ReturnsNotFound()
        {
            //Arrange
            string deviceId = nameof(PostDeviceRequest_DeviceNotFound_ReturnsNotFound);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ThrowsAsync(new IdNotFoundException(ErrorCodes.DeviceNotFound, deviceId));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.PostAsJsonAsync($"api/v1/devices/{deviceId}/request", default(string));
            var content = await response.Content.ReadAsStringAsync();
            ApplicationProblemDetails problem = JsonConvert.DeserializeObject<ApplicationProblemDetails>(content);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorCodes.DeviceNotFound, problem.ErrorCode);
        }

        [Fact]
        public async Task PostDeviceRequest_EnvironmentNotFound_ReturnsNotFound()
        {
            //Arrange
            string deviceId = nameof(PostDeviceRequest_EnvironmentNotFound_ReturnsNotFound);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ThrowsAsync(new IdNotFoundException(ErrorCodes.EnvironmentNotFound, deviceId));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.PostAsJsonAsync($"api/v1/devices/{deviceId}/request?environmentId=someId", default(string));
            var content = await response.Content.ReadAsStringAsync();
            ApplicationProblemDetails problem = JsonConvert.DeserializeObject<ApplicationProblemDetails>(content);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorCodes.EnvironmentNotFound, problem.ErrorCode);
        }

        #endregion

        #region PUT

        [Fact]
        public async Task PuDeviceRequest_HappyPath_ReturnsOk()
        {
            //Arrange
            string deviceId = nameof(PuDeviceRequest_HappyPath_ReturnsOk);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ReturnsAsync(new InvocationResult(StatusCodes.Status200OK, GetMockedLegacyResponse(StatusCodes.Status200OK, default(string))));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.PutAsJsonAsync($"api/v1/devices/{deviceId}/request", default(string));

            //Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task PutDeviceRequest_DeviceNotFound_ReturnsNotFound()
        {
            //Arrange
            string deviceId = nameof(PutDeviceRequest_DeviceNotFound_ReturnsNotFound);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ThrowsAsync(new IdNotFoundException(ErrorCodes.DeviceNotFound, deviceId));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.PutAsJsonAsync($"api/v1/devices/{deviceId}/request", default(string));
            var content = await response.Content.ReadAsStringAsync();
            ApplicationProblemDetails problem = JsonConvert.DeserializeObject<ApplicationProblemDetails>(content);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorCodes.DeviceNotFound, problem.ErrorCode);
        }

        [Fact]
        public async Task PutDeviceRequest_EnvironmentNotFound_ReturnsNotFound()
        {
            //Arrange
            string deviceId = nameof(PutDeviceRequest_EnvironmentNotFound_ReturnsNotFound);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ThrowsAsync(new IdNotFoundException(ErrorCodes.EnvironmentNotFound, deviceId));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.PutAsJsonAsync($"api/v1/devices/{deviceId}/request?environmentId=someId", default(string));
            var content = await response.Content.ReadAsStringAsync();
            ApplicationProblemDetails problem = JsonConvert.DeserializeObject<ApplicationProblemDetails>(content);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorCodes.EnvironmentNotFound, problem.ErrorCode);
        }

        #endregion

        #region DELETE

        [Fact]
        public async Task DeleteDeviceRequest_HappyPath_ReturnsOk()
        {
            //Arrange
            string deviceId = nameof(DeleteDeviceRequest_HappyPath_ReturnsOk);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ReturnsAsync(new InvocationResult(StatusCodes.Status200OK, GetMockedLegacyResponse(StatusCodes.Status200OK, default(string))));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.DeleteAsync($"api/v1/devices/{deviceId}/request");

            //Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task DeleteDeviceRequest_DeviceNotFound_ReturnsNotFound()
        {
            //Arrange
            string deviceId = nameof(DeleteDeviceRequest_DeviceNotFound_ReturnsNotFound);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ThrowsAsync(new IdNotFoundException(ErrorCodes.DeviceNotFound, deviceId));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.DeleteAsync($"api/v1/devices/{deviceId}/request");
            var content = await response.Content.ReadAsStringAsync();
            ApplicationProblemDetails problem = JsonConvert.DeserializeObject<ApplicationProblemDetails>(content);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorCodes.DeviceNotFound, problem.ErrorCode);
        }

        [Fact]
        public async Task DeleteDeviceRequest_EnvironmentNotFound_ReturnsNotFound()
        {
            //Arrange
            string deviceId = nameof(DeleteDeviceRequest_EnvironmentNotFound_ReturnsNotFound);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ThrowsAsync(new IdNotFoundException(ErrorCodes.EnvironmentNotFound, deviceId));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.DeleteAsync($"api/v1/devices/{deviceId}/request?environmentId=someId");
            var content = await response.Content.ReadAsStringAsync();
            ApplicationProblemDetails problem = JsonConvert.DeserializeObject<ApplicationProblemDetails>(content);

            //Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Equal(ErrorCodes.EnvironmentNotFound, problem.ErrorCode);
        }

        #endregion

        #region Authentication

        [Fact]
        public async Task ApiRequest_AzureADAuthentication_ReturnsOk()
        {
            //Arrange
            string deviceId = nameof(ApiRequest_AzureADAuthentication_ReturnsOk);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ReturnsAsync(new InvocationResult(StatusCodes.Status200OK, GetMockedLegacyResponse(StatusCodes.Status200OK, default(string))));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });

                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddInMemoryCollection(new[]
                    {
                        // enable Azure AD authentication
                        new KeyValuePair<string, string>(ApiEnvironment.AuthenticationProvider, IdentityProviders.AzureActiveDirectory),

                        // use fake identity
                        new KeyValuePair<string, string>(TestEnvironment.UseIdentity, bool.TrueString),
                    });
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.GetAsync($"api/v1/devices/{deviceId}/request");

            //Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ApiRequest_AzureADAuthentication_ReturnsUnauthorized()
        {
            //Arrange
            string deviceId = nameof(ApiRequest_AzureADAuthentication_ReturnsUnauthorized);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddInMemoryCollection(new[]
                    {
                        // enable Azure AD authentication
                        // use anonymous identity
                        new KeyValuePair<string, string>(ApiEnvironment.AuthenticationProvider, IdentityProviders.AzureActiveDirectory),
                    });
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.GetAsync($"api/v1/devices/{deviceId}/request");

            //Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task ApiRequest_AzureADRolesAuthorization_ReturnsOk()
        {
            //Arrange
            string deviceId = nameof(ApiRequest_AzureADRolesAuthorization_ReturnsOk);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ReturnsAsync(new InvocationResult(StatusCodes.Status200OK, GetMockedLegacyResponse(StatusCodes.Status200OK, default(string))));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });

                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddInMemoryCollection(new[]
                    {
                        // enable Azure AD authentication w/ application permissions
                        new KeyValuePair<string, string>(ApiEnvironment.AuthenticationProvider, IdentityProviders.AzureActiveDirectory),
                        new KeyValuePair<string, string>($"{ApiEnvironment.AzureADRoles}:0", "SomeRoles"),

                        // use fake identity w/ application permissions
                        new KeyValuePair<string, string>(TestEnvironment.UseIdentity, bool.TrueString),
                        new KeyValuePair<string, string>(TestEnvironment.IdentityRoles, "SomeRoles")
                    });
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.GetAsync($"api/v1/devices/{deviceId}/request");

            //Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ApiRequest_AzureADRolesAuthorization_ReturnsForbidden()
        {
            //Arrange
            string deviceId = nameof(ApiRequest_AzureADRolesAuthorization_ReturnsForbidden);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddInMemoryCollection(new[]
                    {
                        // enable Azure AD authentication w/ application permissions
                        new KeyValuePair<string, string>(ApiEnvironment.AuthenticationProvider, IdentityProviders.AzureActiveDirectory),
                        new KeyValuePair<string, string>($"{ApiEnvironment.AzureADRoles}:0", "SomeRoles"),

                        // use fake identity w/o application permissions
                        new KeyValuePair<string, string>(TestEnvironment.UseIdentity, bool.TrueString)
                    });
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.GetAsync($"api/v1/devices/{deviceId}/request");

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task ApiRequest_AzureADScopesAuthorization_ReturnsOk()
        {
            //Arrange
            string deviceId = nameof(ApiRequest_AzureADScopesAuthorization_ReturnsOk);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ReturnsAsync(new InvocationResult(StatusCodes.Status200OK, GetMockedLegacyResponse(StatusCodes.Status200OK, default(string))));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });

                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddInMemoryCollection(new[]
                    {
                        // enable Azure AD authentication w/ delegated permissions
                        new KeyValuePair<string, string>(ApiEnvironment.AuthenticationProvider, IdentityProviders.AzureActiveDirectory),
                        new KeyValuePair<string, string>($"{ApiEnvironment.AzureADScopes}:0", "SomeScopes"),

                        // use fake identity w/ delegated permissions
                        new KeyValuePair<string, string>(TestEnvironment.UseIdentity, bool.TrueString),
                        new KeyValuePair<string, string>(TestEnvironment.IdentityScopes, "SomeScopes")
                    });
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.GetAsync($"api/v1/devices/{deviceId}/request");

            //Assert
            response.EnsureSuccessStatusCode();
        }

        [Fact]
        public async Task ApiRequest_AzureADScopesAuthorization_ReturnsForbidden()
        {
            //Arrange
            string deviceId = nameof(ApiRequest_AzureADScopesAuthorization_ReturnsForbidden);
            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddInMemoryCollection(new[]
                    {
                        // enable Azure AD authentication w/ delegated permissions
                        new KeyValuePair<string, string>(ApiEnvironment.AuthenticationProvider, IdentityProviders.AzureActiveDirectory),
                        new KeyValuePair<string, string>($"{ApiEnvironment.AzureADScopes}:0", "SomeScopes"),

                        // use fake identity w/o delegated permissions
                        new KeyValuePair<string, string>(TestEnvironment.UseIdentity, bool.TrueString)
                    });
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.GetAsync($"api/v1/devices/{deviceId}/request");

            //Assert
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        #endregion

        #region Common

        [Fact]
        public async Task Common_ErrorResponse_OriginalDeviceError()
        {
            //Arrange
            string deviceId = nameof(Common_ErrorResponse_OriginalDeviceError);
            var mockStatusCode = StatusCodes.Status400BadRequest;
            var mockBody = "Device is in error because...";

            var webAppFactory = m_ApiAppFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    var stubAdapter = new Mock<IDeviceCommunicationAdapter>();
                    stubAdapter.Setup(a => a.InvokeCommandAsync(deviceId, It.IsAny<string>()))
                        .ReturnsAsync(new InvocationResult(mockStatusCode, GetMockedLegacyResponse(mockStatusCode, mockBody)));
                    services.AddScoped<IDeviceCommunicationAdapter>(_ => stubAdapter.Object);
                });
            });
            var client = webAppFactory.CreateClient();
            client.DefaultRequestHeaders.Add(DeviceRequest.RemoteQueryHeaderName, "/some/path");

            //Act
            var response = await client.GetAsync($"api/v1/devices/{deviceId}/request");
            var content = await response.Content.ReadAsStringAsync();

            //Assert
            Assert.Equal(mockStatusCode, (int)response.StatusCode);
            Assert.Equal(mockBody, content);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Returns a mocked response (ECLYPSE implementation).
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private string GetMockedLegacyResponse(int statusCode, string body)
        {
            return JsonConvert.SerializeObject(new DeviceInlineResponse()
            {
                Headers = new DeviceResponseHeaders() { Status = statusCode },
                Body = body
            });
        }

        /// <summary>
        /// Returns a mocked response (preferred implementation).
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        private string GetMockedResponse(int statusCode, object body)
        {
            return JsonConvert.SerializeObject(new DeviceInlineResponse()
            {
                Status = statusCode,
                Body = body
            });
        }

        #endregion
    }
}

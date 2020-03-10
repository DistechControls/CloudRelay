using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.FunctionalTests.Middleware
{
    class IdentityMiddleware
    {
        private readonly RequestDelegate _next;

        public IdentityMiddleware(RequestDelegate rd)
        {
            _next = rd;
        }

        public async Task Invoke(HttpContext httpContext, IConfiguration configuration)
        {
            // simulate an authenticated user
            var identity = new ClaimsIdentity("AuthenticationTypes.Federation");

            // simulate authorization based on application permissions aka roles
            var identityRoles = configuration.GetSection(TestEnvironment.IdentityRoles).Value;

            if (!string.IsNullOrEmpty(identityRoles))
                identity.AddClaim(new Claim(ClaimTypes.Role, identityRoles));

            // simulate authorization based on delegated permissions aka scopes
            var identityScopes = configuration.GetSection(TestEnvironment.IdentityScopes).Value;

            if (!string.IsNullOrEmpty(identityScopes))
                identity.AddClaim(new Claim("http://schemas.microsoft.com/identity/claims/scope", identityScopes));

            httpContext.User = new ClaimsPrincipal(identity);

            await _next.Invoke(httpContext);
        }
    }
}

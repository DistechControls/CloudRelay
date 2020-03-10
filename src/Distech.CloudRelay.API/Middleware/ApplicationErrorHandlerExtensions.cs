using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Middleware
{
    /// <summary>
    /// Contains extensions method to configure the ApplicationErrorHandlerMiddleware middleware.
    /// </summary>
    public static class ApplicationErrorHandlerExtensions
    {
        /// <summary>
        /// Registers middleware to the application which properly log, configure and serialize application related exceptions.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseApplicationErrorHandler(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<ApplicationErrorHandlerMiddleware>();
        }
    }
}

using Microsoft.AspNetCore.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Middleware
{
    /// <summary>
    /// Contains extensions method to configure the MultiPartRequestBufferingMiddleware middleware.
    /// </summary>
    public static class MultiPartRequestBufferingExtensions
    {
        /// <summary>
        /// Registers middleware to the application which properly configure request body buffering.
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseMultiPartRequestBuffering(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            return app.UseMiddleware<MultiPartRequestBufferingMiddleware>();
        }
    }
}

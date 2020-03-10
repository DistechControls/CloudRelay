using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Middleware
{
    /// <summary>
    /// Middleware used to enable request buffering in order to read the body multiple times.
    /// </summary>
    public class MultiPartRequestBufferingMiddleware
    {
        #region Members

        private readonly RequestDelegate m_Next;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="next"></param>
        public MultiPartRequestBufferingMiddleware(RequestDelegate next)
        {
            m_Next = next;
        }

        #endregion

        #region Middleware Support

        public async Task InvokeAsync(HttpContext context)
        {
            // enables request body buffering only when necessary.
            // in this case, allows reading it after already being parsed to fill the IFormFile/IFormFileCollection
            if (context.Request.HasFormContentType)
                context.Request.EnableBuffering();

            await m_Next(context);
        }

        #endregion
    }
}

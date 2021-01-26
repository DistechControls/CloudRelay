using Distech.CloudRelay.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Middleware
{
    /// <summary>
    /// Middleware used to handle application errors and format an appropriate response to the client.
    /// </summary>
    public class ApplicationErrorHandlerMiddleware
    {
        #region Constants

        private const string RequestPayloadTelemetryKey = "RequestPayload";

        #endregion

        #region Shared Members

        private static readonly RouteData DefaultRouteData = new RouteData();
        private static readonly ActionDescriptor DefaultActionDescriptor = new ActionDescriptor();

        #endregion

        #region Members

        private readonly RequestDelegate m_Next;
        private readonly ILogger<ApplicationErrorHandlerMiddleware> m_Logger;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        /// <param name="next"></param>
        /// <param name="logger"></param>
        public ApplicationErrorHandlerMiddleware(RequestDelegate next, ILogger<ApplicationErrorHandlerMiddleware> logger)
        {
            m_Next = next;
            m_Logger = logger;
        }

        #endregion

        #region Middleware Support

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await m_Next(context);
            }
            catch (Exception ex)
            {
                if (context.Response.HasStarted)
                {
                    //cannot handle exception formatting if response has already started
                    throw;
                }

                if (!TryHandleException(ex, context, out ProblemDetails details))
                {
                    //only handle exceptions we know how to format
                    throw;
                }

                var result = new ObjectResult(details)
                {
                    StatusCode = details.Status,
                    DeclaredType = details.GetType()
                };

                result.ContentTypes.Add("application/problem+json");
                result.ContentTypes.Add("application/problem+xml");

                var actionContext = new ActionContext(context, context.GetRouteData() ?? DefaultRouteData, DefaultActionDescriptor);
                await result.ExecuteResultAsync(actionContext);
            }
        }

        #endregion

        #region Exception Handling

        /// <summary>
        /// Handles exception formatting and logging.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="context"></param>
        /// <param name="details"></param>
        /// <returns></returns>
        private bool TryHandleException(Exception ex, HttpContext context, out ProblemDetails details)
        {
            //keep a reference to original exception prior to adapt any convertion/formatting
            Exception exToFormat = ex ?? throw new ArgumentNullException(nameof(ex));

            //apply common rules for well-known exceptions convertible to an ApiException with its associated error code
            switch (exToFormat)
            {
                //add case based on exception type when specific rules are required
                case ApiException _:
                default:
                    //exception is already formattable as is, or we do not have any convertion yet
                    break;
            }

            details = null;
            if (exToFormat is ApiException apiException)
            {
                details = apiException.ToProblemDetails();
            }
            else
            {
                LogException(exToFormat, context);

                //format unknow exceptions as problem details
                details = new ProblemDetails()
                {
                    Type = "about:blank",
                    Title = ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError),
                    Status = StatusCodes.Status500InternalServerError,
                    Detail = "An error occurred while executing the request"
                };
            }

            return details != null;
        }

        /// <summary>
        /// Logs the exception with related context information.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="context"></param>
        private void LogException(Exception ex, HttpContext context)
        {
            // build the full URL
            var requestURL = $"{context.Request.Method} {context.Request.Scheme}://{context.Request.Host.ToUriComponent()}{context.Request.PathBase.ToUriComponent()}{context.Request.Path.ToUriComponent()}{context.Request.QueryString.ToUriComponent()}";

            //disabled telemetry customization with current request body for now because:
            //  1. body stream is not seekable so we need to either
            //     a) always activate the Request.EnableBuffering() for all incoming requests in order to read the body stream a 2nd time
            //     b) expose the current DeviceInlineRequest instance which contains the actual body stream data
            //  2. user data persistency concern as device POST requests can contain secrets

            /*
            // ensure request body is human readable (prevent logging binary payload)
            var requestTelemetry = context.Features.Get<RequestTelemetry>();

            if (requestTelemetry != null &&
                context.Request.ContentType != null &&
                (context.Request.ContentType.Contains(MediaTypeNames.Application.Json, StringComparison.OrdinalIgnoreCase) || context.Request.ContentType.Contains(MediaTypeNames.Application.Xml, StringComparison.OrdinalIgnoreCase)))
            {
                string requestPayload;

                try
                {
                    using (StreamReader reader = new StreamReader(context.Request.Body))
                    {
                        requestPayload = reader.ReadToEnd();
                    }
                }
                catch (Exception exception)
                {
                    requestPayload = $"Unable to read request paylod. {exception.Message}";
                }

                // add the request body payload to telemetry
                requestTelemetry.Properties.Add(RequestPayloadTelemetryKey, requestPayload);
            }
            */

            // log exception to all registered targets
            m_Logger.LogError(ex, $"Unhandled exception while executing the request {requestURL}");
        }

        #endregion
    }
}

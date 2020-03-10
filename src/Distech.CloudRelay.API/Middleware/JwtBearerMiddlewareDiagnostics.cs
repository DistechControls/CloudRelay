using Microsoft.AspNetCore.Authentication.JwtBearer;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Middleware
{
    /// <summary>
    /// Diagnostics for the JwtBearer middleware.
    /// </summary>
    public class JwtBearerMiddlewareDiagnostics
    {
        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        static Func<AuthenticationFailedContext, Task> onAuthenticationFailed;

        /// <summary>
        /// Invoked when a protocol message is first received.
        /// </summary>
        static Func<MessageReceivedContext, Task> onMessageReceived;

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated.
        /// </summary>
        static Func<TokenValidatedContext, Task> onTokenValidated;

        /// <summary>
        /// Invoked before a challenge is sent back to the caller.
        /// </summary>
        static Func<JwtBearerChallengeContext, Task> onChallenge;

        /// <summary>
        /// Subscribes to all the JwtBearer events, to help debugging, while
        /// preserving the previous handlers (which are called)
        /// </summary>
        /// <param name="events">Events to subscribe to</param>
        public static JwtBearerEvents Subscribe(JwtBearerEvents events)
        {
            if (events == null)
            {
                events = new JwtBearerEvents();
            }

            onAuthenticationFailed = events.OnAuthenticationFailed;
            events.OnAuthenticationFailed = OnAuthenticationFailed;

            onMessageReceived = events.OnMessageReceived;
            events.OnMessageReceived = OnMessageReceived;

            onTokenValidated = events.OnTokenValidated;
            events.OnTokenValidated = OnTokenValidated;

            onChallenge = events.OnChallenge;
            events.OnChallenge = OnChallenge;

            return events;
        }

        static async Task OnMessageReceived(MessageReceivedContext context)
        {
            Debug.WriteLine($"Begin {nameof(OnMessageReceived)}");
            await onMessageReceived(context);
            Debug.WriteLine($"End - {nameof(OnMessageReceived)}");
        }

        static async Task OnAuthenticationFailed(AuthenticationFailedContext context)
        {
            Debug.WriteLine($"Begin {nameof(OnAuthenticationFailed)}");
            await onAuthenticationFailed(context);
            Debug.WriteLine($"End - {nameof(OnAuthenticationFailed)}");
        }

        static async Task OnTokenValidated(TokenValidatedContext context)
        {
            Debug.WriteLine($"Begin {nameof(OnTokenValidated)}");
            await onTokenValidated(context);
            Debug.WriteLine($"End - {nameof(OnTokenValidated)}");
        }

        static async Task OnChallenge(JwtBearerChallengeContext context)
        {
            Debug.WriteLine($"Begin {nameof(OnChallenge)}");
            await onChallenge(context);
            Debug.WriteLine($"End - {nameof(OnChallenge)}");
        }
    }
}
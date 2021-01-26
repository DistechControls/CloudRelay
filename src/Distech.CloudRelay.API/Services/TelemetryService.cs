using Distech.CloudRelay.API.Model;
using Microsoft.Extensions.Logging;
using StatsdClient;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Services
{
    public sealed class TelemetryService : ITelemetryService
    {
        private readonly DogStatsdService dogStatsdService;
        private readonly ILogger<TelemetryService> logger;

        public TelemetryService(ILogger<TelemetryService> logger)
        {
            if (TelemetryConfig.statsdConfig != null)
            {
                dogStatsdService = new DogStatsdService();
                dogStatsdService.Configure(TelemetryConfig.statsdConfig);
            }
            this.logger = logger;
        }

        public async Task IncrementCounterAsync(string metricName, string[] tags)
        {
            if (dogStatsdService == null) return;
            try
            {
                dogStatsdService.Increment($"{metricName}.increment", tags: TelemetryConfig.CoreTags.Concat(tags).ToArray());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error sending telemetry to Datadog {ex.Message}");
            }
        }

        public async Task<T> StartTimerAsync<T>(Func<Task<T>> func, string metricName, string[] tags)
        {
            if (dogStatsdService == null) return await func.Invoke();

            T result = default(T);

            try
            {
                result = await dogStatsdService.Time(func, metricName, 1, tags: TelemetryConfig.CoreTags.Concat(tags).ToArray());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error sending telemetry to Datadog {ex.Message}");
            }
            return result;
        }

        public void Dispose()
        {
            if (dogStatsdService != null)
            {
                dogStatsdService.Flush();
                dogStatsdService.Dispose();
            }
        }
    }
}

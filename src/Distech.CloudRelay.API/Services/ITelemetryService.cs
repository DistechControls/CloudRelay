using System;
using System.Threading.Tasks;

namespace Distech.CloudRelay.API.Services
{
    public interface ITelemetryService : IDisposable
    {
        Task IncrementCounterAsync(string metricName, string[] tags);
        Task<T> StartTimerAsync<T>(Func<Task<T>> func, string metricName, string[] tags);
    }
}

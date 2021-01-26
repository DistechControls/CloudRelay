using StatsdClient;
using System;

namespace Distech.CloudRelay.API.Model
{
    public static class TelemetryConfig
    {
        public static string[] CoreTags { get; } = new string[] { $"env:{Environment.GetEnvironmentVariable("DEPLOYED_ENVIRONMENT")}", };

        public static string StatsdHost { get; } = Environment.GetEnvironmentVariable("STATSD_HOST");

        public static int StatsdPort
        {
            get
            {
                var _StatsdPort = Environment.GetEnvironmentVariable("STATSD_PORT");
                int statsport;
                if (Int32.TryParse(_StatsdPort, out statsport))
                    return statsport;
                return -1;
            }
        }

        public static StatsdConfig statsdConfig { get {
                if (!string.IsNullOrEmpty(StatsdHost) && StatsdPort > 0)
                {
                    return new StatsdConfig
                    {
                        StatsdServerName = StatsdHost,
                        StatsdPort = StatsdPort,
                        Prefix = "relay_api",
                        ServiceName = "distech.cloudrelay",
                        Environment = Environment.GetEnvironmentVariable("DEPLOYED_ENVIRONMENT")
                    };
                }
                else
                    return null;
            }
        }

    }
}

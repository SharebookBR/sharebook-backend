using Rollbar;
using System;

namespace ShareBook.Api.Services
{
    public static class RollbarConfigurator
    {
        public static bool IsActive { get; private set; }

        public static void Configure(string environment, string isActive, string token, string logLevel)
        {
            if (string.IsNullOrEmpty(environment) || 
                string.IsNullOrEmpty(isActive) ||
                string.IsNullOrEmpty(token))
                return;

            var result = ErrorLevel.TryParse(logLevel, out ErrorLevel logLevelEnum);

            if (!result)
                throw new Exception("Rollbar invalid logLevel: " + logLevel);

            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(token) { Environment = environment, LogLevel = logLevelEnum });
            RollbarLocator.RollbarInstance.Info($"Rollbar is configured properly in {environment} environment.");

            IsActive = true;
        }
    }
}
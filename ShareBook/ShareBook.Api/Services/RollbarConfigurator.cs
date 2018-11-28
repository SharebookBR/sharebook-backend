using Rollbar;

namespace ShareBook.Api.Services
{
    public static class RollbarConfigurator
    {
        public static void Configure(string environment)
        {
            if (string.IsNullOrEmpty(environment) || environment == "local")
            {
                return;
            }

            RollbarLocator.RollbarInstance.Configure(new RollbarConfig("2efbb216f34747b58371ef04ee27b074") { Environment = environment });
            RollbarLocator.RollbarInstance.Info($"Rollbar is configured properly in {environment} environment.");
        }
    }
}

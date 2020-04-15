using Rollbar;

namespace ShareBook.Api.Services
{
    public static class RollbarConfigurator
    {
        public static bool IsActive { get; private set; }

        public static void Configure(string environment, string isActive, string token)
        {
            if (string.IsNullOrEmpty(environment) || environment == "Development" ||
                isActive == "false" || string.IsNullOrEmpty(isActive) ||
                string.IsNullOrEmpty(token))
                return;

            RollbarLocator.RollbarInstance.Configure(new RollbarConfig(token) { Environment = environment });
            RollbarLocator.RollbarInstance.Info($"Rollbar is configured properly in {environment} environment.");

            IsActive = true;
        }
    }
}
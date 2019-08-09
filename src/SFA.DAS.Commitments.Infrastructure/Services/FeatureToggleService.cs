using Microsoft.Azure;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class FeatureToggleService : IFeatureToggleService
    {
        public bool IsEnabled(string featureToggleName)
        {
            var value = CloudConfigurationManager.GetSetting($"FeatureToggle.{featureToggleName}");

            if (value == null || !bool.TryParse(value, out var result))
            {
                return false;
            }

            return result;
        }
    }
}
using SFA.DAS.Commitments.Domain.Interfaces;
using System.Configuration;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class FeatureToggleService : IFeatureToggleService
    {
        public bool IsEnabled(string featureToggleName)
        {
            var value = ConfigurationManager.AppSettings[$"FeatureToggle.{featureToggleName}"];

            if (value == null || !bool.TryParse(value, out var result))
            {
                return false;
            }

            return result;
        }
    }
}
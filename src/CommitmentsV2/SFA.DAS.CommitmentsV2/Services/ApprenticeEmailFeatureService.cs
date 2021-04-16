using System.Threading.Tasks;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ApprenticeEmailFeatureService : IApprenticeEmailFeatureService
    {
        private FeatureToggle _apprenticeEmailFeature;
        private bool _useEmployerProviderList;

        public ApprenticeEmailFeatureService(FeatureTogglesService<CustomisedFeaturesConfiguration, FeatureToggle> featureToggle, CustomisedFeaturesConfiguration config)
        {
            _apprenticeEmailFeature = featureToggle.GetFeatureToggle("ApprenticeEmail");
            _useEmployerProviderList = config.ApprenticeEmailFeatureUseEmployerProviderList;
        }

        public bool IsEnabled => _apprenticeEmailFeature.IsEnabled;

        public async Task<bool> ApprenticeEmailIsRequiredFor(long employerAccountId, long providerId)
        {
            if (_useEmployerProviderList)
            {
                // check if employer & provider exist
            }

            return await Task.FromResult(true);
        }
    }
}
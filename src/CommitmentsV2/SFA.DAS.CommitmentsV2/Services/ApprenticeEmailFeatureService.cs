using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class ApprenticeEmailFeatureService : IApprenticeEmailFeatureService
    {
        private readonly FeatureToggle _apprenticeEmailFeature;
        private readonly bool _usePrivateBetaList;
        private readonly List<PrivateBetaItem> _privateBetaList;

        public ApprenticeEmailFeatureService(FeatureTogglesService<CustomisedFeaturesConfiguration, FeatureToggle> featureToggle, CustomisedFeaturesConfiguration config)
        {
            _apprenticeEmailFeature = featureToggle.GetFeatureToggle("ApprenticeEmail");
            _usePrivateBetaList = config.ApprenticeEmailFeatureUsePrivateBetaList;
            _privateBetaList = config.PrivateBetaList;
        }

        public bool IsEnabled => _apprenticeEmailFeature.IsEnabled;

        public bool ApprenticeEmailIsRequiredFor(long employerAccountId, long providerId)
        {
            if (_usePrivateBetaList)
            {
                var found = _privateBetaList.Any(i => i.EmployerAccountId == employerAccountId && i.ProviderId == providerId);
                return found;
            }

            return true;
        }

        public bool ApprenticeEmailIsRequiredFor(long providerId)
        {
            if (_usePrivateBetaList)
            {
                var found = _privateBetaList.Any(i => i.ProviderId == providerId);
                return found;
            }

            return true;
        }
    }
}
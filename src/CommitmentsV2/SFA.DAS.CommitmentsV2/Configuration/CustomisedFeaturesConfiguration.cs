using System.Collections.Generic;
using SFA.DAS.Authorization.Features.Configuration;

namespace SFA.DAS.CommitmentsV2.Configuration
{
    public class CustomisedFeaturesConfiguration : FeaturesConfiguration
    {
        public bool ApprenticeEmailFeatureUsePrivateBetaList { get; set; }
        public List<PrivateBetaItem> PrivateBetaList { get; set; }
    }

    public class PrivateBetaItem
    {
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
    }

}

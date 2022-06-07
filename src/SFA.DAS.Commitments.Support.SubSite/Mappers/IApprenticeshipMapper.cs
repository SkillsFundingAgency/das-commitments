using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public interface IApprenticeshipMapper
    {
        ApprenticeshipViewModel MapToApprenticeshipViewModel(GetSupportApprenticeshipQueryResult response, GetChangeOfProviderChainQueryResult providerChainQueryResult);

        UlnSummaryViewModel MapToUlnResultView(GetSupportApprenticeshipQueryResult response);

        ApprenticeshipSearchItemViewModel MapToApprenticeshipSearchItemViewModel(SupportApprenticeshipDetails apprenticeship);
    }
}
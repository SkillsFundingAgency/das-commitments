using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfProviderChain;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Commitments.Support.SubSite.Application.Queries.GetSupportApprenticeship;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public interface IApprenticeshipMapper
    {
        ApprenticeshipViewModel MapToApprenticeshipViewModel(GetSupportApprenticeshipQueryResult response, GetChangeOfProviderChainQueryResult providerChainQueryResult);

        ApprenticeshipUpdateViewModel MapToUpdateApprenticeshipViewModel(GetApprenticeshipUpdateQueryResult apprenticeships, SupportApprenticeshipDetails originalApprenticeship);

        UlnSummaryViewModel MapToUlnResultView(GetSupportApprenticeshipQueryResult response);

        ApprenticeshipSearchItemViewModel MapToApprenticeshipSearchItemViewModel(SupportApprenticeshipDetails apprenticeship);
        OverlappingTrainingDateRequestViewModel MapToOverlappingTrainingDateRequest(GetOverlappingTrainingDateRequestQueryResult.OverlappingTrainingDateRequest overlappingTrainingDateRequest);
    }
}
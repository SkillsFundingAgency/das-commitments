using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public interface IApprenticeshipMapper
    {
        ApprenticeshipViewModel MapToApprenticeshipViewModel(GetApprenticeshipQueryResult response);

        UlnSummaryViewModel MapToUlnResultView(GetApprenticeshipsQueryResult response);

        ApprenticeshipSearchItemViewModel MapToApprenticeshipSearchItemViewModel(Apprenticeship apprenticeship);
    }
}
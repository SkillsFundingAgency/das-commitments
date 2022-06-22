using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Application.Queries.GetSupportApprenticeship;
using SFA.DAS.CommitmentsV2.Models;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public interface IApprenticeshipMapper
    {
        ApprenticeshipViewModel MapToApprenticeshipViewModel(GetSupportApprenticeshipQueryResult response);

        ApprenticeshipUpdateViewModel MapToUpdateApprenticeshipViewModel(GetApprenticeshipUpdateQueryResult updateApprenticeship, SupportApprenticeshipDetails originalApprenticeship);

        UlnSummaryViewModel MapToUlnResultView(GetSupportApprenticeshipQueryResult response);

        ApprenticeshipSearchItemViewModel MapToApprenticeshipSearchItemViewModel(SupportApprenticeshipDetails apprenticeship);
    }
}
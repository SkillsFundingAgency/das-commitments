using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Support.SubSite.Models;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public interface IApprenticeshipMapper
    {

        ApprenticeshipViewModel MapToApprenticeshipViewModel(Apprenticeship response);
        UlnSummaryViewModel MapToUlnResultView(GetApprenticeshipsByUlnResponse response);
        ApprenticeshipSearchItemViewModel MapToApprenticeshipSearchItemViewModel(Apprenticeship apprenticeship);
    }
}
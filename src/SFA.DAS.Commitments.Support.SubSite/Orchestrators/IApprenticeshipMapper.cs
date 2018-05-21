using SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Support.SubSite.Models;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public interface IApprenticeshipMapper
    {

        ApprenticeshipViewModel MapToApprenticeshipViewModel(Apprenticeship response);
        UlnSearchResultSummaryViewModel MapToUlnResultView(GetApprenticeshipsByUlnResponse response);
    }
}
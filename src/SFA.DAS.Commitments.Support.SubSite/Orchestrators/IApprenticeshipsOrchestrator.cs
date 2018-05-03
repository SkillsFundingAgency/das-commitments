using SFA.DAS.Commitments.Support.SubSite.Models;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public interface IApprenticeshipsOrchestrator
    {
        Task<UlnSearchResultSummaryViewModel> GetApprenticeshipsByUln(ApprenticeshipSearchQuery searchQuery);

    }
}

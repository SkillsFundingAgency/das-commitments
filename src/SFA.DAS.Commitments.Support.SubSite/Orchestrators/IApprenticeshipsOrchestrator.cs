using System;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Support.SubSite.Orchestrators
{
    public interface IApprenticeshipsOrchestrator
    {
        Task<int> GetApprenticeshipsByUln(String unl);
    }
}

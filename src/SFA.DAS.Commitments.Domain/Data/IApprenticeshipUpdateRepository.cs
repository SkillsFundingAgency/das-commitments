using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IApprenticeshipUpdateRepository
    {
        Task<ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long apprenticeshipId);
        Task CreateApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate);
    }
}

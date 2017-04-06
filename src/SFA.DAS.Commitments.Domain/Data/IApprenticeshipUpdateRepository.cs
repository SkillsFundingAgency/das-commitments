using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IApprenticeshipUpdateRepository
    {
        Task<ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long apprenticeshipId);

        Task CreateApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate, Apprenticeship apprenticeship);

        Task ApproveApprenticeshipUpdate(long apprenticeshipId, string userId, Apprenticeship apprenticeship, Caller caller);

        Task RejectApprenticeshipUpdate(long apprenticeshipUpdateId, string userId);

        Task UndoApprenticeshipUpdate(long apprenticeshipUpdateId, string userId);
    }
}

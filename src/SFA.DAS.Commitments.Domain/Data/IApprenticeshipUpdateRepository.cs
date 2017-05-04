using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IApprenticeshipUpdateRepository
    {
        Task<ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long apprenticeshipId);

        Task CreateApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate, Apprenticeship apprenticeship);

        Task ApproveApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate, string userId, Apprenticeship apprenticeship, Caller caller);

        Task RejectApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate, string userId);

        Task UndoApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate, string userId);

        Task SupercedeApprenticeshipUpdate(long apprenticeshipUpdateId);
    }
}

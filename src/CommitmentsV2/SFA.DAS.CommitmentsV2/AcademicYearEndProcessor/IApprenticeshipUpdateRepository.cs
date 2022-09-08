using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Data
{
    public interface IApprenticeshipUpdateRepository
    {
        Task<ApprenticeshipUpdate_new> GetPendingApprenticeshipUpdate(long apprenticeshipId);

        Task CreateApprenticeshipUpdate(ApprenticeshipUpdate_new apprenticeshipUpdate, Apprenticeship_new apprenticeship);

        Task ApproveApprenticeshipUpdate(ApprenticeshipUpdate_new apprenticeshipUpdate, Apprenticeship_new apprenticeship, Caller caller);

        Task RejectApprenticeshipUpdate(ApprenticeshipUpdate_new apprenticeshipUpdate);

        Task UndoApprenticeshipUpdate(ApprenticeshipUpdate_new apprenticeshipUpdate);

        Task<IEnumerable<ApprenticeshipUpdate_new>> GetExpiredApprenticeshipUpdates(DateTime currentAcademicYearStartDate);

        Task ExpireApprenticeshipUpdate(long apprenticeshipUpdateId);
    }
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Data
{
    public interface IApprenticeshipUpdateRepository
    {
        Task<IEnumerable<ApprenticeshipUpdateDetails>> GetExpiredApprenticeshipUpdates(DateTime currentAcademicYearStartDate);

        Task ExpireApprenticeshipUpdate(long apprenticeshipUpdateId);
    }
}

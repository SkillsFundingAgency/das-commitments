using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Domain.Data
{
    public interface IApprenticeshipUpdateRepository
    {
        Task<IEnumerable<ApprenticeshipUpdate>> GetExpiredApprenticeshipUpdates(DateTime currentAcademicYearStartDate);
        Task<int> ExpireApprenticeshipUpdate(ApprenticeshipUpdate apprenticeshipUpdate);
    }
}

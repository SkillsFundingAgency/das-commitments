using SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces.AddEpaToApprenticeship
{
    public interface IPaymentEvents
    {
        Task<IEnumerable<DataLockStatus>> GetDataLockEvents(long sinceEventId = 0, DateTime? sinceTime = null, string employerAccountId = null, long ukprn = 0, int page = 1);

        Task<PageOfResults<SubmissionEvent>> GetSubmissionEvents(long sinceEventId = 0, DateTime? sinceTime = null, long ukprn = 0, int page = 1);
    }
}

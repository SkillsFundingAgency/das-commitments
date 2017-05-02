﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using SFA.DAS.Commitments.Domain.Entities.DataLock;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IPaymentEvents
    {
        Task<IEnumerable<DataLockStatus>> GetDataLockEvents(long sinceEventId = 0, DateTime? sinceTime = null, string employerAccountId = null, long ukprn = 0, int page = 1);
    }
}
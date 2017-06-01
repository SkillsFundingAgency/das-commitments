using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetEmployerAccountSummary
{
    public class GetEmployerAccountSummaryResponse : QueryResponse<IEnumerable<ApprenticeshipStatusSummary>> { }
}

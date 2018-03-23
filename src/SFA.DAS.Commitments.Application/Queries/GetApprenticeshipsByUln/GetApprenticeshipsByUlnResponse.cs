using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeshipsByUln
{
    public class GetApprenticeshipsByUlnResponse
    {
        public IList<Apprenticeship> Apprenticeships { get; set; }

        public int TotalCount { get; set; }
    }
}

using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetApprenticeships
{
    public sealed class GetApprenticeshipsResponse
    {
        public IList<Apprenticeship> Apprenticeships { get; set; }

        public int TotalCount { get; set; }
    }
}

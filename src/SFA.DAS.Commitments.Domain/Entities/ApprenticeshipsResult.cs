using System.Collections.Generic;

namespace SFA.DAS.Commitments.Domain.Entities
{
    public class ApprenticeshipsResult
    {
        public List<Apprenticeship> Apprenticeships { get; set; }

        public int TotalCount { get; set; }
    }
}
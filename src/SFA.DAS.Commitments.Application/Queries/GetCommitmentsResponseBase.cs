using System.Collections.Generic;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries
{
    public abstract class GetCommitmentsResponseBase
    {
        public IList<Commitment> Commitments { get; set; }

        public bool HasError { get; set; }
    }
}

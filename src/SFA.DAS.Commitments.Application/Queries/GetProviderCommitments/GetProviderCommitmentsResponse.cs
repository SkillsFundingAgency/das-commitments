using System.Collections.Generic;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetProviderCommitments
{
    public sealed class GetProviderCommitmentsResponse
    {
        public IList<Commitment> Commitments { get; set; }

        public bool HasError { get; set; }
    }
}

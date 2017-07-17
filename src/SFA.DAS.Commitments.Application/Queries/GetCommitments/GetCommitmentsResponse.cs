using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitments
{
    public sealed class GetCommitmentsResponse : QueryResponse<IList<CommitmentSummary>>
    {
    }
}

using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Commitment;

namespace SFA.DAS.Commitments.Application.Queries.GetCommitments
{
    public sealed class GetCommitmentsResponse : QueryResponse<IList<CommitmentListItem>>
    {
    }
}

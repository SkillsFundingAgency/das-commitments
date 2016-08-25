using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Queries.GetEmployerCommitments
{
    public sealed class GetEmployerCommitmentsResponse : QueryResponse<IList<CommitmentListItem>>
    {
    }
}

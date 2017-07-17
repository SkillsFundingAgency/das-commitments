using System.Collections.Generic;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Api.Orchestrators.Mappers
{
    public interface ICommitmentMapper
    {
        IEnumerable<CommitmentListItem> MapFrom(IEnumerable<CommitmentSummary> source, CallerType callerType);
        CommitmentListItem MapFrom(CommitmentSummary source, CallerType callerType);
    }
}

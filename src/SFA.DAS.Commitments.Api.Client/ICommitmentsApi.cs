using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types;

namespace SFA.DAS.Commitments.Api.Client
{
    public interface ICommitmentsApi
    {
        Task<List<CommitmentListItem>> GetProviderCommitments(long providerId);
        Task<List<CommitmentListItem>> GetEmployerCommitments(long employerAccountId);
    }
}
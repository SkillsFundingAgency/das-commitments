using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.ProviderPayment;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public interface IEmployerOrchestrator
    {
        Task<IEnumerable<Types.Commitment.CommitmentListItem>> GetCommitments(long accountId);
        Task<Types.Commitment.CommitmentView> GetCommitment(long accountId, long commitmentId);
        Task<long> CreateCommitment(long accountId, Types.Commitment.CommitmentRequest commitmentRequest);
        Task<IEnumerable<Types.Apprenticeship.Apprenticeship>> GetApprenticeships(long accountId);
        Task<Types.Apprenticeship.ApprenticeshipSearchResponse> GetApprenticeships(long accountId, Types.Apprenticeship.ApprenticeshipSearchQuery query);
        Task<Types.Apprenticeship.Apprenticeship> GetApprenticeship(long accountId, long apprenticeshipId);
        Task<long> CreateApprenticeship(long accountId, long commitmentId, Types.Apprenticeship.ApprenticeshipRequest apprenticeshipRequest);
        Task PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, Types.Apprenticeship.ApprenticeshipRequest apprenticeshipRequest);
        Task UpdateCustomProviderPaymentPriority(long accountId, ProviderPaymentPrioritySubmission submission);
        Task<IEnumerable<ProviderPaymentPriorityItem>> GetCustomProviderPaymentPriority(long accountId);
        Task PatchCommitment(long accountId, long commitmentId, CommitmentSubmission submission);
        Task PatchApprenticeship(long accountId, long apprenticeshipId, Types.Apprenticeship.ApprenticeshipSubmission apprenticeshipSubmission);
        Task DeleteApprenticeship(long accountId, long apprenticeshipId, string userId, string userName);
        Task DeleteCommitment(long accountId, long commitmentId, string userId, string userName);
        Task<Types.Apprenticeship.ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long accountId, long apprenticeshipId);
        Task CreateApprenticeshipUpdate(long accountId, Types.Apprenticeship.ApprenticeshipUpdateRequest updateRequest);
        Task PatchApprenticeshipUpdate(long accountId, long apprenticeshipId, Types.Apprenticeship.ApprenticeshipUpdateSubmission submission);
        Task<IEnumerable<Types.ApprenticeshipStatusSummary>> GetAccountSummary(long accountId);
        Task<IEnumerable<Types.Apprenticeship.Apprenticeship>> GetActiveApprenticeshipsForUln(long accountId, string uln);
    }
}
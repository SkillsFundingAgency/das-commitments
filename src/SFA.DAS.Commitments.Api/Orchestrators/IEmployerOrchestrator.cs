using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.ProviderPayment;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public interface IEmployerOrchestrator
    {
        Task<IEnumerable<Types.Commitment.CommitmentListItem>> GetCommitments(long accountId);
        Task<Types.Commitment.CommitmentView> GetCommitment(long accountId, long commitmentId);
        Task<Types.Commitment.CommitmentView> GetCommitment(long accountId, long commitmentId, CallerType callerType);
        Task<long> CreateCommitment(long accountId, Types.Commitment.CommitmentRequest commitmentRequest);
        Task<IEnumerable<Apprenticeship>> GetApprenticeships(long accountId);
        Task<ApprenticeshipSearchResponse> GetApprenticeships(long accountId, ApprenticeshipSearchQuery query);
        Task<Apprenticeship> GetApprenticeship(long accountId, long apprenticeshipId);
        Task<long> CreateApprenticeship(long accountId, long commitmentId, ApprenticeshipRequest apprenticeshipRequest);
        Task PutApprenticeship(long accountId, long commitmentId, long apprenticeshipId, ApprenticeshipRequest apprenticeshipRequest);
        Task UpdateCustomProviderPaymentPriority(long accountId, ProviderPaymentPrioritySubmission submission);
        Task<IEnumerable<ProviderPaymentPriorityItem>> GetCustomProviderPaymentPriority(long accountId);
        Task PatchCommitment(long accountId, long commitmentId, CommitmentSubmission submission);
        Task PatchApprenticeship(long accountId, long apprenticeshipId, ApprenticeshipSubmission apprenticeshipSubmission);
        Task DeleteApprenticeship(long accountId, long apprenticeshipId, string userId, string userName);
        Task DeleteCommitment(long accountId, long commitmentId, string userId, string userName);
        Task<ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long accountId, long apprenticeshipId);
        Task CreateApprenticeshipUpdate(long accountId, ApprenticeshipUpdateRequest updateRequest);
        Task PatchApprenticeshipUpdate(long accountId, long apprenticeshipId, ApprenticeshipUpdateSubmission submission);
        Task<IEnumerable<ApprenticeshipStatusSummary>> GetAccountSummary(long accountId);
        Task<IEnumerable<Apprenticeship>> GetActiveApprenticeshipsForUln(long accountId, string uln);
        Task PutApprenticeshipStopDate(long accountId, long commitmentId, long apprenticeshipId, ApprenticeshipStopDate stopDate);
        Task ApproveCohort(long accountId, long commitmentId, CommitmentSubmission submission);
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;

namespace SFA.DAS.Commitments.Api.Orchestrators
{
    public interface IProviderOrchestrator
    {
        Task<IEnumerable<CommitmentListItem>> GetCommitments(long providerId);
        Task<IEnumerable<CommitmentAgreement>> GetCommitmentAgreements(long providerId);
        Task<CommitmentView> GetCommitment(long providerId, long commitmentId);
        Task<IEnumerable<Apprenticeship>> GetApprenticeships(long providerId);
        Task<ApprenticeshipSearchResponse> GetApprenticeships(long providerId, ApprenticeshipSearchQuery query);
        Task<Apprenticeship> GetApprenticeship(long providerId, long apprenticeshipId);
        Task CreateApprenticeships(long providerId, long commitmentId, BulkApprenticeshipRequest bulkRequest);
        Task PatchCommitment(long providerId, long commitmentId, CommitmentSubmission submission);
        Task ApproveCohort(long providerId, long commitmentId, CommitmentSubmission submission);
        Task DeleteApprenticeship(long providerId, long apprenticeshipId, string userId, string userName);
        Task DeleteCommitment(long providerId, long commitmentId, string userId, string userName);
        Task<Types.Apprenticeship.ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long providerId, long apprenticeshipId);
        Task CreateApprenticeshipUpdate(long providerId, ApprenticeshipUpdateRequest updateRequest);
        Task PatchApprenticeshipUpdate(long providerId, long apprenticeshipId, ApprenticeshipUpdateSubmission submission);
        Task<long> PostBulkUploadFile(long providerId, BulkUploadFileRequest bulkUploadFile);
        Task<string> GetBulkUploadFile(long providerId, long bulkUploadFileId);
        Task<GetProviderResponse> GetProvider(long providerId);
    }
}
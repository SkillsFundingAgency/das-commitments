using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Api.Client.Interfaces
{
    public interface IHttpCommitmentHelper
    {
        Task<CommitmentView> PostCommitment(string url, CommitmentRequest commitment);

        Task PatchCommitment(string url, CommitmentSubmission submision);

        Task PatchApprenticeship(string url, ApprenticeshipSubmission apprenticeshipSubmission);

        Task<List<CommitmentListItem>> GetCommitments(string url);

        Task<List<CommitmentAgreement>> GetCommitmentAgreements(string url);

        Task<CommitmentView> GetCommitment(string url);

        Task<List<Apprenticeship>> GetApprenticeships(string url);

        Task<ApprenticeshipSearchResponse> GetApprenticeships(string url, ApprenticeshipSearchQuery apprenticeshipQuery);

        Task<Apprenticeship> GetApprenticeship(string url);

        Task PutApprenticeship(string url, ApprenticeshipRequest apprenticeship);

        Task<Apprenticeship> PostApprenticeship(string url, ApprenticeshipRequest apprenticeship);

        Task<Apprenticeship> PostApprenticeships(string url, BulkApprenticeshipRequest bulkRequest);

        Task DeleteApprenticeship(string url, DeleteRequest deleteRequest);

        Task DeleteCommitment(string url, DeleteRequest deleteRequest);

        Task PostApprenticeshipUpdate(string url, ApprenticeshipUpdateRequest apprenticeshipUpdate);

        Task<ApprenticeshipUpdate> GetApprenticeshipUpdate(string url);

        Task PatchApprenticeshipUpdate(string url, ApprenticeshipUpdateSubmission submission);

        Task<long> PostBulkuploadFile(string url, BulkUploadFileRequest bulkUploadFileRequest);

        Task<string> GetBulkuploadFile(string url);

        Task<List<ApprenticeshipStatusSummary>> GetEmployerAccountSummary(string url);

        Task<List<TransferRequestSummary>> GetTransferRequests(string url);

        Task<TransferRequest> GetTransferRequest(string url);
        Task<T> GetUrlResult<T>(string url);

    }
}
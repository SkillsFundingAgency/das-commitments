using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Api.Types.DataLock;

namespace SFA.DAS.Commitments.Api.Client.Interfaces
{
    public interface IEmployerCommitmentApi
    {
        Task<List<CommitmentListItem>> GetEmployerCommitments(long employerAccountId);
        Task<CommitmentView> GetEmployerCommitment(long employerAccountId, long commitmentId);
        Task<CommitmentView> GetTransferSenderCommitment(long transferSenderAccountId, long commitmentId);
        Task<List<Apprenticeship>> GetEmployerApprenticeships(long employerAccountId);
        Task<IList<Apprenticeship>> GetActiveApprenticeshipsForUln(long employerAccountId, string uln);
        Task<ApprenticeshipSearchResponse> GetEmployerApprenticeships(long employerAccountId, ApprenticeshipSearchQuery apprenticeshipSearchQuery);
        Task<Apprenticeship> GetEmployerApprenticeship(long employerAccountId, long apprenticeshipId);
        Task<List<ApprenticeshipStatusSummary>> GetEmployerAccountSummary(long employerAccountId);
        Task PatchEmployerCommitment(long employerAccountId, long commitmentId, CommitmentSubmission submission);
        Task DeleteEmployerCommitment(long employerAccountId, long commitmentId, DeleteRequest deleteRequest);
        Task PatchEmployerApprenticeship(long employerAccountId, long apprenticeshipId, ApprenticeshipSubmission apprenticeshipSubmission);
        Task DeleteEmployerApprenticeship(long employerAccountId, long apprenticeshipId, DeleteRequest deleteRequest);
        Task CreateApprenticeshipUpdate(long employerAccountId, long apprenticeshipId, ApprenticeshipUpdateRequest apprenticeshipUpdateRequest);
        Task<ApprenticeshipUpdate> GetPendingApprenticeshipUpdate(long employerAccountId, long apprenticeshipId);
        Task PatchApprenticeshipUpdate(long employerAccountId, long apprenticeshipId, ApprenticeshipUpdateSubmission submission);
        Task<IEnumerable<PriceHistory>> GetPriceHistory(long employerAccountId, long apprenticeshipId);
        Task<List<DataLockStatus>> GetDataLocks(long employerAccountId, long apprenticeshipId);
        Task<DataLockSummary> GetDataLockSummary(long employerAccountId, long apprenticeshipId);
        Task PatchDataLocks(long employerAccountId, long apprenticeshipId, DataLocksTriageResolutionSubmission submission);
        Task PutApprenticeshipStopDate(long accountId, long commitmentId, long apprenticeshipId, ApprenticeshipStopDate stopDate);
        Task ApproveCohort(long employerAccountId, long commitmentId, CommitmentSubmission submission);
        Task PatchTransferApprovalStatus(long transferSenderId, long commitmentId, long transferRequestId, TransferApprovalRequest request);
        Task<List<TransferRequestSummary>> GetTransferRequests(string hashedAccountId);
        Task<TransferRequest> GetTransferRequestForSender(long transferSenderId, long transferRequestId);
        Task<TransferRequest> GetTransferRequestForReceiver(long transferSenderId, long transferRequestId);
        Task<IEnumerable<long>> GetAllEmployerAccountIds();
    }
}
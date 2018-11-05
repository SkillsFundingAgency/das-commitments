using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface ICommitmentRepository
    {
        Task<long> Create(Commitment commitment);
        Task<IList<CommitmentSummary>> GetCommitmentsByProvider(long providerId);
        Task<IList<CommitmentSummary>> GetCommitmentsByEmployer(long accountId);
        Task<IList<CommitmentAgreement>> GetCommitmentAgreementsForProvider(long providerId);
        Task<Commitment> GetCommitmentById(long id);
        Task DeleteCommitment(long commitmentId);
        Task UpdateCommitment(Commitment commitment);
        Task UpdateCommitmentReference(long commitmentId, string hashValue);
        Task SetPaymentOrder(long accountId);
        Task SetTransferRequestApproval(long transferRequestId, long commitmentId, TransferApprovalStatus transferApprovalStatus, string userId, string userName);
        Task<TransferRequest> GetTransferRequest(long transferRequestId);
        Task<IList<TransferRequestSummary>> GetTransferRequestsForSender(long transferSenderAccountId);
        Task<IList<TransferRequestSummary>> GetPendingTransferRequests();
        Task<IList<TransferRequestSummary>> GetTransferRequestsForReceiver(long transferReceiverAccountId);
        Task<long> StartTransferRequestApproval(long commitmentId, decimal cost, int fundingCap, List<TrainingCourseSummary> trainingCourses);
        Task ResetEditStatusToEmployer(long commitmentId);
        Task SaveMessage(long commitmentId, Message message);
    }
}

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
        Task<Commitment> GetCommitmentById(long id);
        Task DeleteCommitment(long commitmentId);
        Task UpdateCommitment(Commitment commitment);
        Task UpdateCommitmentReference(long commitmentId, string hashValue);
        Task SetPaymentOrder(long accountId);
        Task SetTransferApproval(long commitmentId, TransferApprovalStatus transferApprovalStatus, string userId, string userName);
        Task ResetEditStatusToEmployer(long commitmentId);
        Task<long> CreateRelationship(Relationship relationship);
        Task<Relationship> GetRelationship(long employerAccountId, long providerId, string legalEntityCode);
        Task VerifyRelationship(long employerAccountId, long providerId, string legalEntityCode, bool verified);
        Task SaveMessage(long commitmentId, Message message);
    }
}

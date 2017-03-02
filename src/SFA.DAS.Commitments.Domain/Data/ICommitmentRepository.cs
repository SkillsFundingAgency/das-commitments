using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface ICommitmentRepository
    {
        Task<long> Create(Commitment commitment, CallerType callerType, string userId);
        Task<IList<CommitmentSummary>> GetCommitmentsByProvider(long providerId);
        Task<IList<CommitmentSummary>> GetCommitmentsByEmployer(long accountId);
        Task<Commitment> GetCommitmentById(long id);
        Task DeleteCommitment(long commitmentId, CallerType callerType, string userId);
        Task UpdateCommitment(long commitmentId, CommitmentStatus commitmentStatus, EditStatus editStatus, LastUpdateAction lastAction);
        Task UpdateCommitmentReference(long commitmentId, string hashValue);
        Task SetPaymentOrder(long accountId);
        Task<long> CreateRelationship(Relationship relationship);
        Task<Relationship> GetRelationship(long employerAccountId, long providerId, string legalEntityCode);
        Task VerifyRelationship(long employerAccountId, long providerId, string legalEntityCode, bool verified);
    }
}

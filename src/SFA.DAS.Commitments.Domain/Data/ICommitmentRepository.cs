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
        Task<Commitment> GetCommitmentById(long id);
        Task<IList<Apprenticeship>> GetApprenticeshipsByProvider(long providerId);
        Task<IList<Apprenticeship>> GetApprenticeshipsByEmployer(long accountId);
        Task<long> CreateApprenticeship(Apprenticeship apprenticeship);
        Task UpdateApprenticeship(Apprenticeship apprenticeship, Caller caller);
        Task<Apprenticeship> GetApprenticeship(long apprenticeshipId);
        Task DeleteApprenticeship(long apprenticeshipId);
        Task DeleteCommitment(long commitmentId);
        Task UpdateCommitmentStatus(long commitmentId, CommitmentStatus commitmentStatus);
        Task UpdateEditStatus(long commitmentId, EditStatus editStatus);
        Task UpdateLastAction(long commitmentId, LastAction lastAction, Caller caller, string updatedByName, string updatedByEmailAddress);
        Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, PaymentStatus paymentStatus);
        Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, AgreementStatus agreementStatus);
        Task UpdateCommitmentReference(long commitmentId, string hashValue);
        Task<IList<Apprenticeship>> BulkUploadApprenticeships(long commitmentId, IEnumerable<Apprenticeship> apprenticeships);
        Task SetPaymentOrder(long accountId);
        Task<long> CreateRelationship(Relationship relationship);
        Task<Relationship> GetRelationship(long employerAccountId, long providerId, string legalEntityCode);
        Task VerifyRelationship(long employerAccountId, long providerId, string legalEntityCode);
    }
}

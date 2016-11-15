using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface ICommitmentRepository
    {
        Task<long> Create(Commitment commitment);
        Task<IList<Commitment>> GetByProvider(long providerId);
        Task<IList<Commitment>> GetByEmployer(long accountId);
        Task<Commitment> GetById(long id);
        Task<long> CreateApprenticeship(Apprenticeship apprenticeship);
        Task UpdateApprenticeship(Apprenticeship apprenticeship, Caller caller);
        Task<Apprenticeship> GetApprenticeship(long id);
        Task UpdateStatus(long commitmentId, CommitmentStatus commitmentStatus);
        Task UpdateApprenticeshipStatus(long commitmentId, long apprenticeshipId, PaymentStatus paymentStatus);
        Task UpdateReference(long commitmentId, string hashValue);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface ICommitmentRepository
    {
        Task<long> Create(Commitment commitment);

        Task<IList<Commitment>> GetByProvider(long providerId);

        Task<IList<Commitment>> GetByEmployer(long accountId);

        Task<Commitment> GetById(long id);

        Task<long> CreateApprenticeship(Apprenticeship apprenticeship);

        Task UpdateApprenticeship(Apprenticeship apprenticeship);

        Task<Apprenticeship> GetApprenticeship(long id);
    }
}
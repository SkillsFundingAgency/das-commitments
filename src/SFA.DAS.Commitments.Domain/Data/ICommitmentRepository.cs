using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface ICommitmentRepository
    {
        Task Create(Commitment commitment);

        Task<IList<Commitment>> GetByProvider(long providerId);

        Task<IList<Commitment>> GetByEmployer(long accountId);
    }
}
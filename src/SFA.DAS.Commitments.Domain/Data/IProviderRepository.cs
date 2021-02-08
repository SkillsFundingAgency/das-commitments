using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Domain.Data
{
    public interface IProviderRepository
    {
        Task<Domain.Entities.Provider> GetProvider(long ukPrn);
        Task<List<Domain.Entities.Provider>> GetProviders();
    }
}
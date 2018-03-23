using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public interface IProviderEmailServiceWrapper
    {
        Task<List<ProviderUser>> GetUsersAsync(long ukprn);
    }
}
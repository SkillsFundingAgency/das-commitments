using System.Threading.Tasks;

namespace SFA.DAS.ProviderApprenticeshipsService.Domain.Data
{
    public interface IStandardsRepository
    {
        Task<Standard[]> GetAllAsync();
        Task<Standard> GetByCodeAsync(int code);
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.ProviderApprenticeshipsService.Domain;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Cache;
using SFA.DAS.ProviderApprenticeshipsService.Domain.Data;

namespace SFA.DAS.ProviderApprenticeshipsService.Infrastructure.Data
{
    public class CachingStandardsRepository : IStandardsRepository
    {
        private readonly IStandardsRepository _standardsRepository;
        private readonly ICacheProvider _cacheProvider;

        public CachingStandardsRepository(IStandardsRepository standardsRepository, ICacheProvider cacheProvider)
        {
            if (standardsRepository == null)
                throw new ArgumentNullException(nameof(standardsRepository));
            if (cacheProvider == null)
                throw new ArgumentNullException(nameof(cacheProvider));
            _standardsRepository = standardsRepository;
            _cacheProvider = cacheProvider;
        }

        public async Task<Standard[]> GetAllAsync()
        {
            var standards = _cacheProvider.Get<Standard[]>(CacheKeys.Standards);
            if (standards == null)
            {
                standards = await _standardsRepository.GetAllAsync();
                if (standards.Length > 0)
                {
                    _cacheProvider.Set(CacheKeys.Standards, standards, DateTimeOffset.UtcNow.AddMinutes(5));
                }
            }
            return standards;
        }

        public async Task<Standard> GetByCodeAsync(int code)
        {
            var standards = await GetAllAsync();
            return standards.SingleOrDefault(s => s.Code == code);
        }
    }

    public static class CacheKeys
    {
        public const string Standards = "STANDARDS";
    }

}
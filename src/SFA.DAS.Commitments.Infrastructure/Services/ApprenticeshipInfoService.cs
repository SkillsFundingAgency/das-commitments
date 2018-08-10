using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipInfoService : IApprenticeshipInfoService
    {
        private const string StandardsKey = "Standards";
        private const string FrameworksKey = "Frameworks";

        private readonly ICache _cache;
        private readonly IApprenticeshipInfoServiceConfiguration _config;
        private readonly IApprenticeshipInfoServiceMapper _mapper;

        public ApprenticeshipInfoService(ICache cache,
            IApprenticeshipInfoServiceConfiguration config,
            IApprenticeshipInfoServiceMapper mapper)
        {
            _cache = cache;
            _config = config;
            _mapper = mapper;
        }

        public async Task<StandardsView> GetStandardsAsync(bool refreshCache = false)   
        {
            if (!await _cache.ExistsAsync(StandardsKey) || refreshCache)
            {
                var api = new StandardApiClient(_config.BaseUrl);

                var standards = (await api.GetAllAsync()).OrderBy(x => x.Title).ToList();

                await _cache.SetCustomValueAsync(StandardsKey, _mapper.MapFrom(standards));
            }

            return await _cache.GetCustomValueAsync<StandardsView>(StandardsKey);
        }

        public async Task<FrameworksView> GetFrameworksAsync(bool refreshCache = false)
        {
            if (!await _cache.ExistsAsync(FrameworksKey) || refreshCache)
            {
                var api = new FrameworkApiClient(_config.BaseUrl);

                var frameworks = (await api.GetAllAsync()).OrderBy(x => x.Title).ToList();

                await _cache.SetCustomValueAsync(FrameworksKey, _mapper.MapFrom(frameworks));
            }

            return await _cache.GetCustomValueAsync<FrameworksView>(FrameworksKey);
        }

        public async Task<ITrainingProgramme> GetTrainingProgramAsync(string id, bool refreshCache = false)
        {
            var standardsTask = GetStandardsAsync();
            var frameworksTask = GetFrameworksAsync();

            await Task.WhenAll(standardsTask, frameworksTask);

            var programmes = standardsTask.Result.Standards.Union(frameworksTask.Result.Frameworks.Cast<ITrainingProgramme>())
                .OrderBy(m => m.Title)
                .ToList();

            return programmes.FirstOrDefault(m => m.Id == id);
        }
    }
}

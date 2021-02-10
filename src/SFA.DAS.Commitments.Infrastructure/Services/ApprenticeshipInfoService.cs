using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipInfoService : IApprenticeshipInfoService
    {
        private const string StandardsKey = "Standards";
        private const string FrameworksKey = "Frameworks";

        private readonly ICache _cache;
        private readonly IApprenticeshipInfoServiceMapper _mapper;
        private readonly ITrainingProgrammeRepository _repository;

        public ApprenticeshipInfoService(ICache cache,
            IApprenticeshipInfoServiceMapper mapper,
            ITrainingProgrammeRepository repository)
        {
            _cache = cache;
            _mapper = mapper;
            _repository = repository;
        }

        public async Task<StandardsView> GetStandards(bool refreshCache = false)
        {
            if (!await _cache.ExistsAsync(StandardsKey) || refreshCache)
            {
                var standards = await _repository.GetAllStandards();

                await _cache.SetCustomValueAsync(StandardsKey, _mapper.MapFrom(standards.OrderBy(c=>c.Title).ToList()));
            }

            return await _cache.GetCustomValueAsync<StandardsView>(StandardsKey);
        }

        public async Task<FrameworksView> GetFrameworks(bool refreshCache = false)
        {
            if (!await _cache.ExistsAsync(FrameworksKey) || refreshCache)
            {
                var frameworks = await _repository.GetAllFrameworks();

                await _cache.SetCustomValueAsync(FrameworksKey, _mapper.MapFrom(frameworks.OrderBy(c=>c.Title).ToList()));
            }

            return await _cache.GetCustomValueAsync<FrameworksView>(FrameworksKey);
        }

        public async Task<ITrainingProgramme> GetTrainingProgram(string id)
        {
            var standardsTask = GetStandards();
            var frameworksTask = GetFrameworks();

            var program = (await standardsTask).Standards.FirstOrDefault(m => m.Id == id);
            if (program != null)
                return program;

            return (await frameworksTask).Frameworks.FirstOrDefault(m => m.Id == id);
        }
    }
}
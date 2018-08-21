using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;

using Framework = SFA.DAS.Commitments.Domain.Entities.TrainingProgramme.Framework;
using Standard = SFA.DAS.Commitments.Domain.Entities.TrainingProgramme.Standard;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipInfoServiceWrapper : IApprenticeshipInfoServiceWrapper
    {
        private const string StandardsKey = "Standards";
        private const string FrameworksKey = "Frameworks";

        private readonly ICache _cache;
        private readonly IApprenticeshipInfoServiceConfiguration _config;

        public ApprenticeshipInfoServiceWrapper(ICache cache, IApprenticeshipInfoServiceConfiguration config)
        {
            _cache = cache;
            _config = config;
        }

        public async Task<StandardsView> GetStandardsAsync(bool refreshCache = false)   
        {
            if (!await _cache.ExistsAsync(StandardsKey) || refreshCache)
            {
                var api = new StandardApiClient(_config.BaseUrl);

                var standards = api.FindAll().OrderBy(x => x.Title).ToList();

                await _cache.SetCustomValueAsync(StandardsKey, MapFrom(standards));
            }

            return await _cache.GetCustomValueAsync<StandardsView>(StandardsKey);
        }

        public async Task<FrameworksView> GetFrameworksAsync(bool refreshCache = false)
        {
            if (!await _cache.ExistsAsync(FrameworksKey) || refreshCache)
            {
                var api = new FrameworkApiClient(_config.BaseUrl);

                var frameworks = api.FindAll().OrderBy(x => x.Title).ToList();

                await _cache.SetCustomValueAsync(FrameworksKey, MapFrom(frameworks));
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

   // temporarily disable warning whilst we determine what breaks the deployment
#pragma warning disable CS0618
        private static FrameworksView MapFrom(List<FrameworkSummary> frameworks)
        {
            return new FrameworksView
            {
                CreatedDate = DateTime.UtcNow,
                Frameworks = frameworks.Select(x => new Framework
                {
                    Id = x.Id,
                    Title = GetTitle(x.FrameworkName.Trim() == x.PathwayName.Trim() ? x.FrameworkName : x.Title, x.Level),
                    FrameworkCode = x.FrameworkCode,
                    FrameworkName = x.FrameworkName,
                    ProgrammeType = x.ProgType,
                    Level = x.Level,
                    PathwayCode = x.PathwayCode,
                    PathwayName = x.PathwayName,
                    Duration = x.Duration,
                    MaxFunding = x.MaxFunding
                }).ToList()
            };
        }

        private static StandardsView MapFrom(List<StandardSummary> standards)
        {
            return new StandardsView
            {
                CreationDate = DateTime.UtcNow,
                Standards = standards.Select(x => new Standard
                {
                    Id = x.Id,
                    Code = long.Parse(x.Id),
                    Level = x.Level,
                    Title = GetTitle(x.Title, x.Level) + " (Standard)",
                    CourseName = x.Title,
                    Duration = x.Duration,
                    MaxFunding = x.MaxFunding
                }).ToList()
            };
        }
#pragma warning restore CS0618

        private static string GetTitle(string title, int level)
        {
            return $"{title}, Level: {level}";
        }
    }
}

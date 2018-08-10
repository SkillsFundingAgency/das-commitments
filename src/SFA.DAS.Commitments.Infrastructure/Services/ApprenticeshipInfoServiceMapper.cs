using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;
using Framework = SFA.DAS.Commitments.Domain.Entities.TrainingProgramme.Framework;
using Standard = SFA.DAS.Commitments.Domain.Entities.TrainingProgramme.Standard;
using FundingPeriod = SFA.DAS.Commitments.Domain.Entities.TrainingProgramme.FundingPeriod;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipInfoServiceMapper : IApprenticeshipInfoServiceMapper
    {
        public FrameworksView MapFrom(List<FrameworkSummary> frameworks)
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
                    MaxFunding = x.CurrentFundingCap,
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    FundingPeriods = MapFundingPeriods(x.FundingPeriods)
                }).ToList()
            };
        }

        public StandardsView MapFrom(List<StandardSummary> standards)
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
                    MaxFunding = x.CurrentFundingCap,
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.LastDateForNewStarts,
                    FundingPeriods = MapFundingPeriods(x.FundingPeriods)
                }).ToList()
            };
        }
        private static IEnumerable<FundingPeriod> MapFundingPeriods(IEnumerable<Apprenticeships.Api.Types.FundingPeriod> source)
        {
            if (source == null)
            {
                return Enumerable.Empty<FundingPeriod>();
            }

            return source.Select(x => new FundingPeriod
            {
                EffectiveFrom = x.EffectiveFrom,
                EffectiveTo = x.EffectiveTo,
                FundingCap = x.FundingCap
            });
        }

        private static string GetTitle(string title, int level)
        {
            return $"{title}, Level: {level}";
        }
    }
}

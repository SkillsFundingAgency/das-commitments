using System;
using System.Collections.Generic;
using System.Linq;
using SFA.DAS.Commitments.Domain.Api.Types;
using SFA.DAS.Commitments.Domain.Entities.TrainingProgramme;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipInfoServiceMapper : IApprenticeshipInfoServiceMapper
    {
        public FrameworksView MapFrom(List<Framework> frameworks)
        {
            return new FrameworksView
            {
                CreatedDate = DateTime.UtcNow,
                Frameworks = frameworks.Select(x => new Framework
                {
                    LarsCode = x.LarsCode,
                    Title = GetTitle(x.FrameworkName.Trim() == x.PathwayName.Trim() ? x.FrameworkName : x.Title, x.Level) + " (Framework)",
                    FrameworkCode = x.FrameworkCode,
                    FrameworkName = x.FrameworkName,
                    ProgrammeType = x.ProgrammeType,
                    Level = x.Level,
                    PathwayCode = x.PathwayCode,
                    PathwayName = x.PathwayName,
                    Duration = x.Duration,
                    MaxFunding = x.MaxFunding,
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    FundingPeriods = x.FundingPeriods
                }).ToList()
            };
        }

        public StandardsView MapFrom(List<Standard> standards)
        {
            return new StandardsView
            {
                CreationDate = DateTime.UtcNow,
                Standards = standards.Select(x => new Standard
                {
                    LarsCode = x.LarsCode.ToString(),
                    Code = Convert.ToInt64(x.LarsCode),
                    Level = x.Level,
                    Title = GetTitle(x.Title, x.Level),
                    CourseName = x.Title,
                    Duration = x.Duration,
                    MaxFunding = x.MaxFunding,
                    EffectiveFrom = x.EffectiveFrom,
                    EffectiveTo = x.EffectiveTo,
                    FundingPeriods = x.FundingPeriods
                }).ToList()
            };
        }
        

        private static string GetTitle(string title, int level)
        {
            return $"{title}, Level: {level}";
        }
    }
}
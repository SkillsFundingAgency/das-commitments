using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class FundingCapService : IFundingCapService
    {
        private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;

        public FundingCapService(ITrainingProgrammeLookup trainingProgrammeLookup)
        {
            _trainingProgrammeLookup = trainingProgrammeLookup;
        }

        public async Task<IReadOnlyCollection<FundingCapCourseSummary>> FundingCourseSummary(IEnumerable<ApprenticeshipBase> apprenticeships)
        {
            decimal MaximumCappedCost(decimal? cost, int cap)
            {
                if (cost.HasValue)
                {
                    return cost.Value > cap ? cap : cost.Value;
                }
                return 0;
            }

            async Task<Dictionary<long, int>> GetFundingBandCaps(IEnumerable<ApprenticeshipBase> apprenticeships)
            {
                var result = new Dictionary<long, int>();
                foreach (var apprenticeship in apprenticeships)
                {
                    var fundingBandCap = (await _trainingProgrammeLookup.GetTrainingProgramme(apprenticeship.CourseCode))
                            .FundingCapOn(apprenticeship.StartDate ?? throw new InvalidOperationException("Start Date cannot be null"));

                    result.Add(apprenticeship.Id, fundingBandCap);
                }

                return result;
            }

            var fundingBandCapForApprentice = await GetFundingBandCaps(apprenticeships);

            var apprenticeWithFundingBandCap = apprenticeships.Select(x => new
            {
                x.Id,
                x.CourseCode,
                x.CourseName,
                x.Cost,
                Cap = fundingBandCapForApprentice[x.Id]
            });

            var courseSummary = apprenticeWithFundingBandCap.GroupBy(a => new { a.CourseCode, a.CourseName })
                .OrderBy(course => course.Key.CourseName)
                .Select(course => new FundingCapCourseSummary
                {
                    CourseTitle = course.Key.CourseName,
                    ApprenticeshipCount = course.Count(),
                    ActualCap = course.Sum(a => a.Cap),
                    CappedCost = course.Sum(a => MaximumCappedCost(a.Cost, a.Cap))
                });

            return courseSummary.ToList();
        }
    }
}
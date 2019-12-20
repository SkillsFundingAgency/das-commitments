using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class FundingCapService : IFundingCapService
    {
        private readonly ITrainingProgrammeApiClient _trainingProgrammeApiClient;

        public FundingCapService(ITrainingProgrammeApiClient trainingProgrammeApiClient)
        {
            _trainingProgrammeApiClient = trainingProgrammeApiClient;
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

            var fundingBandCapForApprentice = await Task.WhenAll(apprenticeships.Select(async x => new
            {
                x.Id,
                x.CourseCode,
                x.CourseName,
                x.Cost,
                Cap = (await _trainingProgrammeApiClient.GetTrainingProgramme(x.CourseCode)).FundingCapOn(x.StartDate ?? throw new InvalidOperationException("Start Date cannot be null")),
            }));

            var courseSummary = fundingBandCapForApprentice.GroupBy(a => new {a.CourseCode, a.CourseName})
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
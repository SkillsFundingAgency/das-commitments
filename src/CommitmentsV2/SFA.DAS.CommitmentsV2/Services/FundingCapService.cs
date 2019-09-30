using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
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

        public Task<IReadOnlyCollection<ApprenticeFundingCap>> GetFundingCapsFor(IEnumerable<Apprenticeship> list)
        {
            throw new NotImplementedException();
        }

        private async Task<(string JsonList, decimal Cost, int Cap)> GetApprenticeshipSummaries(Cohort cohort)
        {
            var fundingBandCapForApprentice = await Task.WhenAll(cohort.Apprenticeships.Select(async x => new
            {
                x.Id,
                x.CourseCode,
                x.CourseName,
                x.Cost,
                Cap = (await _trainingProgrammeApiClient.GetTrainingProgramme(x.CourseCode)).FundingCapOn(x.StartDate.Value),
            }));

            var courseSummary = fundingBandCapForApprentice.GroupBy(a => new {a.CourseCode, a.CourseName})
                .OrderBy(course => course.Key.CourseName)
                .Select(course => new
                {
                    course.Key.CourseName,
                    ApprenticeshipCount = course.Count(),
                    Cap = course.Sum(a => a.Cap),
                    Cost = course.Sum(a => a.Cost > a.Cap ? a.Cap : a.Cost)
                }).ToList();




            return await Task.FromResult((JsonConvert.SerializeObject(course), cost, 2));
        }


    }
}
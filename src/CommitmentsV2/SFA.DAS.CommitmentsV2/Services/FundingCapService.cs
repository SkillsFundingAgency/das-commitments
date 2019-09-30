using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Apprenticeships.Api.Client;
using SFA.DAS.Apprenticeships.Api.Types;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
//using TrainingProgrammeStatus = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgrammeStatus;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class FundingCapService : IFundingCapService
    {
        private readonly ITrainingProgrammeApiClient _trainingProgrammeApiClient;

        public FundingCapService(ITrainingProgrammeApiClient trainingProgrammeApiClient)
        {
            _trainingProgrammeApiClient = trainingProgrammeApiClient;
        }

        public async Task<IReadOnlyCollection<FundingCapCourseSummary>> FundingCourseSummary(IEnumerable<Apprenticeship> apprenticeships)
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

    public static class ITrainingProgrammeExtensions2
  {
    public static bool IsActiveOn(this ITrainingProgramme course, DateTime date)
    {
      return ITrainingProgrammeExtensions2.GetStatusOn(course.EffectiveFrom, course.EffectiveTo, date) == SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus.Active;
    }

    public static SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus GetStatusOn(
      this ITrainingProgramme course,
      DateTime date)
    {
      return ITrainingProgrammeExtensions2.GetStatusOn(course.EffectiveFrom, course.EffectiveTo, date);
    }

    public static int FundingCapOn(this ITrainingProgramme course, DateTime date)
    {
      if (!course.IsActiveOn(date))
        return 0;
      IFundingPeriod fundingPeriod = course.FundingPeriods.FirstOrDefault<IFundingPeriod>((Func<IFundingPeriod, bool>) (x => ITrainingProgrammeExtensions2.GetStatusOn(x.EffectiveFrom, x.EffectiveTo, date) == SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus.Active));
      if (fundingPeriod == null)
        return 0;
      return fundingPeriod.FundingCap;
    }

    private static SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus GetStatusOn(
      DateTime? effectiveFrom,
      DateTime? effectiveTo,
      DateTime date)
    {
      DateTime date1 = date.Date;
      if (effectiveFrom.HasValue && effectiveFrom.Value.FirstOfMonth() > date1)
        return SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus.Pending;
      return !effectiveTo.HasValue || effectiveTo.Value >= date1 ? SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus.Active : SFA.DAS.Apprenticeships.Api.Types.TrainingProgrammeStatus.Expired;
    }

    private static DateTime FirstOfMonth(this DateTime value)
    {
      return new DateTime(value.Year, value.Month, 1);
    }
  }

}
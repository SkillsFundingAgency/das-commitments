using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Extensions;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class TrainingProgrammeExtensions
{
    public static bool IsActiveOn(this TrainingProgramme course, DateTime date)
    {
        return GetStatusOn(course.EffectiveFrom, course.EffectiveTo, date) == TrainingProgrammeStatus.Active;
    }

    public static TrainingProgrammeStatus GetStatusOn(this TrainingProgramme course, DateTime date)
    {
        return GetStatusOn(course.EffectiveFrom, course.EffectiveTo, date);
    }

    public static int FundingCapOn(this TrainingProgramme course, DateTime date)
    {
        //todo: would probably be better to return int? null or throw if out of range
        if (!course.IsActiveOn(date))
        {
            return 0;
        }

        var applicableFundingPeriod = course.FundingPeriods.Find(x => GetStatusOn(x.EffectiveFrom, x.EffectiveTo, date) == TrainingProgrammeStatus.Active);

        return applicableFundingPeriod?.FundingCap ?? 0;
    }

    /// <remarks>
    /// we make use of the same logic to determine ActiveOn and FundingBandOn so that if the programme is active, it should fall within a funding band
    /// </remarks>
    private static TrainingProgrammeStatus GetStatusOn(DateTime? effectiveFrom, DateTime? effectiveTo, DateTime date)
    {
        var dateOnly = date.Date;

        if (effectiveFrom.HasValue && effectiveFrom.Value.FirstOfMonth() > dateOnly)
        {
            return TrainingProgrammeStatus.Pending;
        }

        if (!effectiveTo.HasValue || effectiveTo.Value >= dateOnly)
        {
            return TrainingProgrammeStatus.Active;
        }

        return TrainingProgrammeStatus.Expired;
    }
}
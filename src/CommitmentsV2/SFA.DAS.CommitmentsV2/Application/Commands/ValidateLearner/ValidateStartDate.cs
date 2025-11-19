using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private IEnumerable<LearnerError> ValidateStartDate(LearnerDataEnhanced record)
    {
        var domainErrors = new List<LearnerError>();

        if (IsBeforeMay2017(record.StartDate))
        {
            domainErrors.Add(new LearnerError("StartDate", "The start date must not be earlier than May 2017"));
        }

        if (record.StartDate > academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
        {
            domainErrors.Add(new LearnerError("StartDate", "The start date must be no later than one year after the end of the current teaching year"));
        }

        var standard = GetStandardDetails(record.CourseCode);
        if (standard == null)
        {
            return domainErrors;
        }

        if (standard.EffectiveFrom.HasValue && record.StartDate < standard.EffectiveFrom.Value)
        {
            var prevMonth = standard.EffectiveFrom.Value.AddMonths(-1);
            domainErrors.Add(new LearnerError("StartDate", $"This training course is only available to apprentices with a start date after {prevMonth.Month}  {prevMonth.Year}"));
        }

        if (standard.EffectiveTo.HasValue && record.StartDate > standard.EffectiveTo.Value)
        {
            var nextMonth = standard.EffectiveTo.Value.AddMonths(1);
            domainErrors.Add(new LearnerError("StartDate", $"This training course is only available to apprentices with a start date before {nextMonth.Month}  {nextMonth.Year}"));
        }

        return domainErrors;
    }

    private static bool IsBeforeMay2017(DateTime startDateAsString)
    {
        return startDateAsString < Constants.DasStartDate;
    }
}
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private IEnumerable<LearnerError> ValidateUln(LearnerData record)
    {
        var domainErrors = new List<LearnerError>();

        var checkResult = ulnValidator.Validate(record.Uln.ToString());

        if (checkResult == UlnValidationResult.IsEmptyUlnNumber)
        {
            domainErrors.Add(new LearnerError("Uln", "Enter a 10-digit unique learner number"));
        }
        else
        {
            if (checkResult == UlnValidationResult.IsInValidTenDigitUlnNumber)
            {
                domainErrors.Add(new LearnerError("Uln", "Enter a 10-digit unique learner number"));
            }
            else if (checkResult == UlnValidationResult.IsInvalidUln)
            {
                domainErrors.Add(new LearnerError("Uln", $"The unique learner number of {record.Uln} isn't valid"));
            }
            else
            {
                var overlapResult = OverlapCheck(record);
                if (overlapResult.HasOverlappingStartDate)
                {
                    domainErrors.Add(new LearnerError("Uln", $"The start date overlaps with existing training dates for the same apprentice"));
                }
                if (overlapResult.HasOverlappingEndDate)
                {
                    domainErrors.Add(new LearnerError("Uln", $"The end date overlaps with existing training dates for the same apprentice"));
                }
            }

        }
        return domainErrors;
    }

    private OverlapCheckResult OverlapCheck(LearnerData record)
    {
        var learnerStartDate = record.StartDate;
        var learnerEndDate = record.PlannedEndDate;

        return overlapService.CheckForOverlaps(record.Uln.ToString(), new DateRange(learnerStartDate, learnerEndDate), null, CancellationToken.None).Result;
    }
}
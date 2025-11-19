using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.EmailValidationService;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private IEnumerable<LearnerError> ValidateEmailAddress(LearnerDataEnhanced record)
    {
        var domainErrors = new List<LearnerError>();

        if (string.IsNullOrEmpty(record.Email))
        {
            domainErrors.Add(new LearnerError("EmailAddress", "Email address is required"));
        }
        else
        {
            if (!IsAValidEmailAddress(record.Email))
            {
                domainErrors.Add(new LearnerError("EmailAddress", $"Enter a valid email address"));
            }
            if (record.Email.Length > 200)
            {
                domainErrors.Add(new LearnerError("EmailAddress", "Enter an email address that is not longer than 200 characters"));
            }
            else
            {
                var overlapResult = OverlapCheckEmail(record);
                if (overlapResult != null)
                {
                    switch (overlapResult.OverlapStatus)
                    {
                        case OverlapStatus.DateEmbrace:
                        case OverlapStatus.DateWithin:
                            domainErrors.Add(new LearnerError("EmailAddress", $"The start date overlaps with existing training dates for an apprentice with the same email address"));
                            domainErrors.Add(new LearnerError("EmailAddress", $"The end date overlaps with existing training dates for an apprentice with the same email address"));
                            break;
                        case OverlapStatus.OverlappingEndDate:
                            domainErrors.Add(new LearnerError("EmailAddress", $"The end date overlaps with existing training dates for an apprentice with the same email address"));
                            break;
                        case OverlapStatus.OverlappingStartDate:
                            domainErrors.Add(new LearnerError("EmailAddress", $"The start date overlaps with existing training dates for an apprentice with the same email address"));
                            break;
                    }
                }
            }
        }

        return domainErrors;
    }

    private static bool IsAValidEmailAddress(string emailAsString)
    {
        try
        {
            return emailAsString.IsAValidEmailAddress();
        }
        catch
        {
            return false;
        }
    }

    private EmailOverlapCheckResult OverlapCheckEmail(LearnerDataEnhanced record)
    {
        var learnerStartDate = record.StartDate;
        var learnerEndDate = record.PlannedEndDate;
        return overlapService.CheckForEmailOverlaps(record.Email, new DateRange(learnerStartDate, learnerEndDate), null, null, CancellationToken.None).Result;
    }
}
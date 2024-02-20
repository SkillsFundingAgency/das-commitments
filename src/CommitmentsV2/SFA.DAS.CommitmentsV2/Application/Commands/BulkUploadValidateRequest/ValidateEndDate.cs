using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Text.RegularExpressions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private static IEnumerable<Error> ValidateEndDate(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        var domainErrors = new List<Error>();
        if (string.IsNullOrEmpty(csvRecord.EndDateAsString))
        {
            domainErrors.Add(new Error("EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02"));
        }
        else if (!Regex.IsMatch(csvRecord.EndDateAsString, "^\\d\\d\\d\\d-\\d\\d$"))
        {
            domainErrors.Add(new Error("EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02"));
        }
        else
        {
            var endDate = csvRecord.EndDate;
            if (endDate == null)
            {
                domainErrors.Add(new Error("EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02"));
            }
            else
            {
                var startDate = csvRecord.StartDate;
                if (startDate != null && endDate.Value < startDate.Value)
                {
                    domainErrors.Add(new Error("EndDate", "Enter an <b>end date</b> that is after the start date"));
                }
            }
        }

        return domainErrors;
    }
}
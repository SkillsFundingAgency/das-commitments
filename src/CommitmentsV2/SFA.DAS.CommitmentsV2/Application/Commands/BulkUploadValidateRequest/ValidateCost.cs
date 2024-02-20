using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Text.RegularExpressions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private static IEnumerable<Error> ValidateCost(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        var domainErrors = new List<Error>();
        if (string.IsNullOrEmpty(csvRecord.CostAsString))
        {
            domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
        }
        else if (csvRecord.Cost == null)
        {
            domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
        }
        else if (csvRecord.Cost.Value == 0)
        {
            domainErrors.Add(new Error("TotalPrice", "The <b>total cost</b> must be more than £0"));
        }
        else if (csvRecord.Cost.Value > 100000)
        {
            domainErrors.Add(new Error("TotalPrice", "The <b>total cost</b> must be £100,000 or less"));
        }
        else if (!Regex.IsMatch(csvRecord.CostAsString, "^([1-9]{1}([0-9]{1,2})?)+(,[0-9]{3})*$|^[1-9]{1}[0-9]*$"))
        {
            domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
        }

        return domainErrors;
    }
}
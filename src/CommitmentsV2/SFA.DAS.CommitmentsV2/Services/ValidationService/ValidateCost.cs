using System.Text.RegularExpressions;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Services.ValidationService;

public partial class ValidationService
{
    public IEnumerable<Error> ValidateCost(string costAsString, int? cost)
    {
        var domainErrors = new List<Error>();
        if (string.IsNullOrEmpty(costAsString))
        {
            domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
        }
        else if (cost == null)
        {
            domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
        }
        else if (cost.Value == 0)
        {
            domainErrors.Add(new Error("TotalPrice", "The <b>total cost</b> must be more than £0"));
        }
        else if (cost.Value > 100000)
        {
            domainErrors.Add(new Error("TotalPrice", "The <b>total cost</b> must be £100,000 or less"));
        }
        else if (!Regex.IsMatch(costAsString, "^([1-9]{1}([0-9]{1,2})?)+(,[0-9]{3})*$|^[1-9]{1}[0-9]*$", RegexOptions.None, new TimeSpan(0, 0, 0, 1)))
        {
            domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
        }

        return domainErrors;
    }
}
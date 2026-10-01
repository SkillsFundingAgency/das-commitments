using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Services.ValidationService;

public partial class ValidationService
{
    public IEnumerable<Error> ValidateLastName(string lastName)
    {
        var domainErrors = new List<Error>();
        if (string.IsNullOrEmpty(lastName))
        {
            domainErrors.Add(new Error("FamilyName", "<b>Last name</b> must be entered"));
        }
        else if (lastName.Length > 100)
        {
            domainErrors.Add(new Error("FamilyName", "Enter a <b>last name</b> that is not longer than 100 characters"));
        }

        return domainErrors;
    }
}
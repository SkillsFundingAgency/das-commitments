using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Services.ValidationService;

public partial class ValidationService
{
    public IEnumerable<Error> ValidateFirstName(string firstName)
    {
        var domainErrors = new List<Error>();
        if (string.IsNullOrEmpty(firstName))
        {
            domainErrors.Add(new Error("GivenName", "<b>First name</b> must be entered"));
        }
        else if (firstName.Length > 100)
        {
            domainErrors.Add(new Error("GivenName", "Enter a <b>first name</b> that is not longer than 100 characters"));
        }

        return domainErrors;
    }
}
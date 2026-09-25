using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Services.ValidationService;

public partial class ValidationService
{
    public IEnumerable<Error> ValidateProviderRef(string providerRef)
    {
        var domainErrors = new List<Error>();
        if (!string.IsNullOrEmpty(providerRef) && providerRef.Length > 20)
        {
            domainErrors.Add(new Error("ProviderRef", "The <b>Provider Ref</b> must not be longer than 20 characters"));
        }

        return domainErrors;
    }
}
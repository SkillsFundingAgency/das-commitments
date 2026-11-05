using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Services.ValidationService;

public partial class ValidationService
{
    public IEnumerable<Error> ValidateEPAOrgId(string EPAOrgId)
    {
        var domainErrors = new List<Error>();
        if (!string.IsNullOrWhiteSpace(EPAOrgId) && EPAOrgId.Length > 7)
        {
            domainErrors.Add(new Error("EPAOrgId", "The <b>EPAO ID</b> must not be longer than 7 characters"));
        }

        return domainErrors;
    }
}
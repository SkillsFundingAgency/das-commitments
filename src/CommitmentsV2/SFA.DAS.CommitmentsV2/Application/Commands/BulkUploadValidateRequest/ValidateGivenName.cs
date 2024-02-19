using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler
    {
        private List<Error> ValidateGivenName(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var domainErrors = new List<Error>();
            if (string.IsNullOrEmpty(csvRecord.FirstName))
            {
                domainErrors.Add(new Error("GivenName", "<b>First name</b> must be entered"));
            }
            else if (csvRecord.FirstName.Length > 100)
            {
                domainErrors.Add(new Error("GivenName", "Enter a <b>first name</b> that is not longer than 100 characters"));
            }

            return domainErrors;
        }
    }
}

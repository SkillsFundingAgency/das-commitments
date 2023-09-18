using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler
    {
        private List<Error> ValidateFamilyName(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var domainErrors = new List<Error>();
            if (string.IsNullOrEmpty(csvRecord.LastName))
            {
                domainErrors.Add(new Error("FamilyName", "<b>Last name</b> must be entered"));
            }
            else if (csvRecord.LastName.Length > 100)
            {
                domainErrors.Add(new Error("FamilyName", "Enter a <b>last name</b> that is not longer than 100 characters"));
            }

            return domainErrors;
        }
    }
}

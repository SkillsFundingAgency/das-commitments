using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler
    {
        private List<Error> ValidateEPAOrgId(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var domainErrors = new List<Error>();
            if (!string.IsNullOrWhiteSpace(csvRecord.EPAOrgId) && csvRecord.EPAOrgId.Length > 7)
            {
                domainErrors.Add(new Error("EPAOrgId", "The <b>EPAO ID</b> must not be longer than 7 characters"));
            }

            return domainErrors;
        }
    }
}

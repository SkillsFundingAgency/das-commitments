using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler
    {
        private IEnumerable<Error> ValidateDurationReducedBy(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            yield break;
        }
    }
}
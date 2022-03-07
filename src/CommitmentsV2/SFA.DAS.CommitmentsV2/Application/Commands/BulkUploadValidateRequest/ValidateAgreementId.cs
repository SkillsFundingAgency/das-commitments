using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private List<Error> ValidateAgreementId(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            List<Error> errors = new List<Error>();
            if (string.IsNullOrEmpty(csvRecord.AgreementId))
            {
                errors.Add(new Error("AgreementId", "<b>Agreement ID</b> must be entered"));
            }
            else if (!csvRecord.AgreementId.All(char.IsLetterOrDigit))
            {
                errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
            }
            else if (csvRecord.AgreementId.Length > 6)
            {
                errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
            }
            else if (string.IsNullOrWhiteSpace(GetEmployerName(csvRecord.AgreementId)))
            {
                errors.Add(new Error("AgreementId", $"Enter a valid <b>Agreement ID</b>"));
            }
            else if (!IsLevy(csvRecord.AgreementId).Value)
            {
                errors.Add(new Error("AgreementId", $"You cannot add apprentices via file on behalf of <b>non-levy employers</b> yet. "));
            }

            return errors;
        }
    }
}

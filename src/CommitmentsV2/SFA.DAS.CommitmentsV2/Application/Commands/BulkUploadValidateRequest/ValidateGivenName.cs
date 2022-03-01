using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private List<Error> ValidateGivenName(CsvRecord csvRecord)
        {
            var domainErrors = new List<Error>();
            if (string.IsNullOrEmpty(csvRecord.GivenNames))
            {
                domainErrors.Add(new Error("GivenName", "<b>First name</b> must be entered"));
            }
            else if (csvRecord.GivenNames.Length > 100)
            {
                domainErrors.Add(new Error("GivenName", "Enter a <b>first name</b> that is not longer than 100 characters"));
            }

            return domainErrors;
        }
    }
}

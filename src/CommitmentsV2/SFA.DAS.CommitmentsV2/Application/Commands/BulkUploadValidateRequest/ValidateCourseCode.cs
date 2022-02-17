using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private List<Error> ValidateCourseCode(CsvRecord csvRecord)
        {
            var domainErrors = new List<Error>();
            if (string.IsNullOrEmpty(csvRecord.StdCode))
            {
                domainErrors.Add(new Error("StdCode", "<b>Standard code</b> must be entered"));
            }
            else if (!csvRecord.StdCode.All(char.IsDigit) && int.TryParse(csvRecord.StdCode, out _))
            {
                domainErrors.Add(new Error("StdCode", "Enter a valid <b>standard code</b>"));
            }
            else if (csvRecord.StdCode.Length > 5)
            {
                domainErrors.Add(new Error("StdCode", "Enter a valid <b>standard code</b>"));
            }
            else if (GetStandardDetails(csvRecord.StdCode) == null)
            {
                domainErrors.Add(new Error("StdCode", "Enter a valid <b>standard code</b>"));
            }

            return domainErrors;
        }
    }
}

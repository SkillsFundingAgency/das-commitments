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
        private List<Error> ValidateCourseCode(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var domainErrors = new List<Error>();
            if (string.IsNullOrEmpty(csvRecord.CourseCode))
            {
                domainErrors.Add(new Error("CourseCode", "<b>Standard code</b> must be entered"));
            }
            else if (!csvRecord.CourseCode.All(char.IsDigit) && !int.TryParse(csvRecord.CourseCode, out _))
            {
                domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code</b>"));
            }
            else if (csvRecord.CourseCode.Length > 5)
            {
                domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code</b>"));
            }
            else if (GetStandardDetails(csvRecord.CourseCode) == null)
            {
                domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code</b>"));
            }

            return domainErrors;
        }
    }
}

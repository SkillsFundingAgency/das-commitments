using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private List<Error> ValidateEndDate(CsvRecord csvRecord)
        {
            var domainErrors = new List<Error>();
            if (string.IsNullOrEmpty(csvRecord.EndDate))
            {
                domainErrors.Add(new Error("EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02"));
            }
            else if (!Regex.IsMatch(csvRecord.EndDate, "^\\d\\d\\d\\d-\\d\\d$"))
            {
                domainErrors.Add(new Error("EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02"));
            }
            else
            {
                var endDate = GetValidDate(csvRecord.EndDate, "yyyy-MM");
                if (endDate == null)
                {
                    domainErrors.Add(new Error("EndDate", "Enter the <b>end date</b> using the format yyyy-mm, for example 2019-02"));
                }
                else
                {
                    var startDate = GetValidDate(csvRecord.StartDate, "yyyy-MM-dd");
                    if (startDate != null && endDate.Value < startDate.Value)
                    {
                        domainErrors.Add(new Error("EndDate", "Enter an <b>end date</b> that is after the start date"));
                    }
                }
            }

            return domainErrors;
        }
    }
}

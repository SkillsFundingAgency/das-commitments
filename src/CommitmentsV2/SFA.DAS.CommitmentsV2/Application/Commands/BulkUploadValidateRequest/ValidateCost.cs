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
        private List<Error> ValidateCost(CsvRecord csvRecord)
        {
            var domainErrors = new List<Error>();
            if (string.IsNullOrEmpty(csvRecord.TotalPrice))
            {
                domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
            }
            else if (!int.TryParse(csvRecord.TotalPrice, out var price))
            {
                domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
            }
            else if (price == 0)
            {
                domainErrors.Add(new Error("TotalPrice", "The <b>total cost</b> must be more than £0"));
            }
            else if (price > 100000)
            {
                domainErrors.Add(new Error("TotalPrice", "The <b>total cost</b> must be £100,000 or less"));
            }
            else if (!Regex.IsMatch(csvRecord.TotalPrice, "^([1-9]{1}([0-9]{1,2})?)+(,[0-9]{3})*$|^[1-9]{1}[0-9]*$"))
            {
                domainErrors.Add(new Error("TotalPrice", "Enter the <b>total cost</b> of training in whole pounds using numbers only"));
            }

            return domainErrors;
        }
    }
}

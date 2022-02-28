using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private List<Error> ValidateUln(CsvRecord csvRecord)
        {
            var domainErrors = new List<Error>();
           
            if (string.IsNullOrEmpty(csvRecord.ULN))
            {
                domainErrors.Add(new Error("ULN", "Enter a 10-digit <b>unique learner number</b>"));
            }
            else
            {
                if (csvRecord.ULN == "9999999999")
                {
                    domainErrors.Add(new Error("ULN", $"The <b>unique learner number</b> of 9999999999 isn't valid"));
                }
                else if (csvRecord.ULN.Length != 10)
                {
                    domainErrors.Add(new Error("ULN", "Enter a 10-digit <b>unique learner number</b>"));
                }
                else if (!Regex.IsMatch(csvRecord.ULN, "^[1-9]{1}[0-9]{9}$"))
                {
                    domainErrors.Add(new Error("ULN", $"Enter a 10-digit <b>unique learner number</b>"));
                }
                else
                {
                    var overlapResult = OverlapCheck(csvRecord);
                    if (overlapResult.HasOverlappingStartDate)
                    {
                        domainErrors.Add(new Error("ULN", $"The <b>start date</b> overlaps with existing training dates for the same apprentice"));
                    }
                    if (overlapResult.HasOverlappingEndDate)
                    {
                        domainErrors.Add(new Error("ULN", $"The <b>end date</b> overlaps with existing training dates for the same apprentice"));
                    }
                }

                if (_csvRecords.Any(x => x.ULN == csvRecord.ULN && csvRecord.RowNumber > x.RowNumber))
                {
                    domainErrors.Add(new Error("ULN", $"The <b>unique learner number</b> has already been used for an apprentice in this file"));
                }
            }
            return domainErrors;
        }

        private OverlapCheckResult OverlapCheck(CsvRecord csvRecord)
        {
            var learnerStartDate = GetValidDate(csvRecord.StartDate, "yyyy-MM-dd");
            var learnerEndDate = GetValidDate(csvRecord.EndDate, "yyyy-MM");
            if (learnerStartDate.HasValue && learnerEndDate.HasValue)
            {
                return _overlapService.CheckForOverlaps(csvRecord.ULN, new Domain.Entities.DateRange(learnerStartDate.Value, learnerEndDate.Value), null, CancellationToken.None).Result;
            }

            return new OverlapCheckResult(false, false);
        }
    }
}

using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler
    {
        private List<Error> ValidateUln(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var domainErrors = new List<Error>();

            var checkResult = _ulnValidator.Validate(csvRecord.Uln);

            if (checkResult == UlnValidationResult.IsEmptyUlnNumber)
            {
                domainErrors.Add(new Error("Uln", "Enter a 10-digit <b>unique learner number</b>"));
            }
            else
            {
                if (checkResult == UlnValidationResult.IsInValidTenDigitUlnNumber)
                {
                    domainErrors.Add(new Error("Uln", "Enter a 10-digit <b>unique learner number</b>"));
                }
                else if (checkResult == UlnValidationResult.IsInvalidUln)
                {
                    domainErrors.Add(new Error("Uln", $"The <b>unique learner number</b> of {csvRecord.Uln} isn't valid"));
                }
                else
                {
                    var overlapResult = OverlapCheck(csvRecord);
                    if (overlapResult.HasOverlappingStartDate)
                    {
                        domainErrors.Add(new Error("Uln", $"The <b>start date</b> overlaps with existing training dates for the same apprentice"));
                    }
                    if (overlapResult.HasOverlappingEndDate)
                    {
                        domainErrors.Add(new Error("Uln", $"The <b>end date</b> overlaps with existing training dates for the same apprentice"));
                    }
                }

                if (_csvRecords.Any(x => x.Uln == csvRecord.Uln && csvRecord.RowNumber > x.RowNumber))
                {
                    domainErrors.Add(new Error("Uln", $"The <b>unique learner number</b> has already been used for an apprentice in this file"));
                }
            }
            return domainErrors;
        }

        private OverlapCheckResult OverlapCheck(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var learnerStartDate = csvRecord.StartDate;
            var learnerEndDate = csvRecord.EndDate;
            if (learnerStartDate.HasValue && learnerEndDate.HasValue)
            {
                return _overlapService.CheckForOverlaps(csvRecord.Uln, new DateRange(learnerStartDate.Value, learnerEndDate.Value), null, CancellationToken.None).Result;
            }

            return new OverlapCheckResult(false, false);
        }
    }
}

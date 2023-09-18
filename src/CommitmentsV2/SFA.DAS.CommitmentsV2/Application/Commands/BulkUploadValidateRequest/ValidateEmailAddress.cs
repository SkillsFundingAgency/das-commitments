using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using SFA.DAS.EmailValidationService;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler
    {
        private List<Error> ValidateEmailAddress(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var domainErrors = new List<Error>();

            if (string.IsNullOrEmpty(csvRecord.Email))
            {
                domainErrors.Add(new Error("EmailAddress", "<b>Email address</b> must be entered"));
            }
            else
            {
                if (!IsAValidEmailAddress(csvRecord.Email))
                {
                    domainErrors.Add(new Error("EmailAddress", $"Enter a valid <b>email address</b>"));
                }
                if (csvRecord.Email.Length > 200)
                {
                    domainErrors.Add(new Error("EmailAddress", "Enter an <b>email address</b> that is not longer than 200 characters"));
                }
                else
                {
                    var overlapResult = OverlapCheckEmail(csvRecord);
                    if (overlapResult != null)
                    {
                        switch (overlapResult.OverlapStatus)
                        {
                            case OverlapStatus.DateEmbrace:
                            case OverlapStatus.DateWithin:
                                domainErrors.Add(new Error("EmailAddress", $"The <b>start date</b> overlaps with existing training dates for an apprentice with the same email address"));
                                domainErrors.Add(new Error("EmailAddress", $"The <b>end date</b> overlaps with existing training dates for an apprentice with the same email address"));
                                break;
                            case OverlapStatus.OverlappingEndDate:
                                domainErrors.Add(new Error("EmailAddress", $"The <b>end date</b> overlaps with existing training dates for an apprentice with the same email address"));
                                break;
                            case OverlapStatus.OverlappingStartDate:
                                domainErrors.Add(new Error("EmailAddress", $"The <b>start date</b> overlaps with existing training dates for an apprentice with the same email address"));
                                break;
                        }
                    }
                }

                if (_csvRecords.Any(x => x.Email == csvRecord.Email && csvRecord.RowNumber > x.RowNumber))
                {
                    domainErrors.Add(new Error("EmailAddress", $"The <b>email address</b> has already been used for an apprentice in this file"));
                }
            }

            return domainErrors;
        }

        private bool IsAValidEmailAddress(string emailAsString)
        {
            try
            {
               return emailAsString.IsAValidEmailAddress();
            }
            catch
            {
                return false;
            }
        }

        private EmailOverlapCheckResult OverlapCheckEmail(BulkUploadAddDraftApprenticeshipRequest csvRecord)
        {
            var learnerStartDate = csvRecord.StartDate;
            var learnerEndDate = csvRecord.EndDate;
            if (learnerStartDate.HasValue && learnerEndDate.HasValue)
            {
                return _overlapService.CheckForEmailOverlaps(csvRecord.Email, new DateRange(learnerStartDate.Value, learnerEndDate.Value), null, null, CancellationToken.None).Result;
            }

            return new EmailOverlapCheckResult(csvRecord.RowNumber, OverlapStatus.None, false);
        }
    }
}

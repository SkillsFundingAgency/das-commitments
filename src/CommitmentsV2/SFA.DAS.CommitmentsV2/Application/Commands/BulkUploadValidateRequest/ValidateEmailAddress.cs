using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private List<Error> ValidateEmailAddress(CsvRecord csvRecord)
        {
            var domainErrors = new List<Error>();

            if (string.IsNullOrEmpty(csvRecord.EmailAddress))
            {
                domainErrors.Add(new Error("EmailAddress", "<b>Email address</b> must be entered"));
            }
            else
            {
                if (!IsAValidEmailAddress(csvRecord.EmailAddress))
                {
                    domainErrors.Add(new Error("EmailAddress", $"Enter a valid <b>email address</b>"));
                }
                if (csvRecord.EmailAddress.Length > 200)
                {
                    domainErrors.Add(new Error("EmailAddress", "Enter an <b>email address</b> that is not longer than 200 characters"));
                }
                else
                {
                    var overlapResult = OverlapCheckEmail(csvRecord);
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

                if (_csvRecords.Any(x => x.EmailAddress == csvRecord.EmailAddress && csvRecord.RowNumber > x.RowNumber))
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
                var email = new System.Net.Mail.MailAddress(emailAsString);
                var parts = email.Address.Split('@');
                if (!parts[1].Contains(".") || parts[1].EndsWith("."))
                {
                    return false;
                }
                return email.Address == emailAsString;
            }
            catch
            {
                return false;
            }
        }

        private EmailOverlapCheckResult OverlapCheckEmail(CsvRecord csvRecord)
        {
            var learnerStartDate = GetValidDate(csvRecord.StartDate, "yyyy-MM-dd");
            var learnerEndDate = GetValidDate(csvRecord.EndDate, "yyyy-MM");
            if (learnerStartDate.HasValue && learnerEndDate.HasValue)
            {
                return _overlapService.CheckForEmailOverlaps(csvRecord.EmailAddress, new Domain.Entities.DateRange(learnerStartDate.Value, learnerEndDate.Value), null, null, CancellationToken.None).Result;
            }

            return new EmailOverlapCheckResult(csvRecord.RowNumber, OverlapStatus.None, false);
        }
    }
}

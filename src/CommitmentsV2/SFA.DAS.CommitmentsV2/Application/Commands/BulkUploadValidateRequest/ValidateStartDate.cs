using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private List<Error> ValidateStartDate(CsvRecord csvRecord)
        {
            var domainErrors = new List<Error>();

            if (string.IsNullOrEmpty(csvRecord.StartDate))
            {
                domainErrors.Add(new Error("StartDate", "Enter the <b>start date</b> using the format yyyy-mm-dd, for example 2017-09-01"));
                return domainErrors;
            }
            else if (!Regex.IsMatch(csvRecord.StartDate, "^\\d\\d\\d\\d-\\d\\d-\\d\\d$"))
            {
                domainErrors.Add(new Error("StartDate", "Enter the <b>start date</b> using the format yyyy-mm-dd, for example 2017-09-01"));
                return domainErrors;
            }
            else
            {
                var startDate = GetValidDate(csvRecord.StartDate, "yyyy-MM-dd");
                if (startDate != null)
                {
                    if (IsBeforeMay2017(startDate.Value))
                    {
                        domainErrors.Add(new Error("StartDate", "The <b>start date</b> must not be earlier than May 2017"));
                    }
                    else if (IsBeforeMay2018AndIsCohortIsTransferFunded(startDate.Value, csvRecord.CohortRef))
                    {
                        domainErrors.Add(new Error("StartDate", "The <b>start date</b> for apprenticeships funded through a transfer must not be earlier than May 2018"));
                    }
                    if (startDate > _academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
                    {
                        domainErrors.Add(new Error("StartDate", "The <b>start date</b> must be no later than one year after the end of the current teaching year"));
                    }

                    var standard = GetStandardDetails(csvRecord.StdCode);
                    if (standard != null)
                    {
                        if (standard.EffectiveFrom.HasValue &&
                            startDate < standard.EffectiveFrom.Value)
                        {
                            domainErrors.Add(new Error("StartDate", $"This training course is only available to apprentices with a <b>start date</b> after {standard.EffectiveFrom.Value.Month}  {standard.EffectiveFrom.Value.Year}"));
                        }

                        if (standard.EffectiveTo.HasValue &&
                          startDate > standard.EffectiveTo.Value)
                        {
                            domainErrors.Add(new Error("StartDate", $"This training course is only available to apprentices with a <b>start date</b> before {standard.EffectiveTo.Value.Month}  {standard.EffectiveTo.Value.Year}"));
                        }
                    }
                }
                else
                {
                    domainErrors.Add(new Error("StartDate", "You must enter the <b>start date</b>, for example 2017-09-01"));
                }
            }

            return domainErrors;
        }

        private bool IsBeforeMay2017(DateTime startDate)
        {
            return startDate < Constants.DasStartDate;
        }

        private bool IsBeforeMay2018AndIsCohortIsTransferFunded(DateTime startDate, string cohortRef)
        {
            var cohortDetails = GetCohortDetails(cohortRef);
            if (cohortDetails != null)
            {
                return cohortDetails.TransferSenderId.HasValue && startDate < Constants.TransferFeatureStartDate;
            }

            return false;
        }
    }
}

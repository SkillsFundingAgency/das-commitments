﻿using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain;
using System.Text.RegularExpressions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private IEnumerable<Error> ValidateStartDate(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        var domainErrors = new List<Error>();

        if (string.IsNullOrEmpty(csvRecord.StartDateAsString))
        {
            domainErrors.Add(new Error("StartDate", "Enter the <b>start date</b> using the format yyyy-mm-dd, for example 2017-09-01"));
            return domainErrors;
        }

        if (!Regex.IsMatch(csvRecord.StartDateAsString, "^\\d\\d\\d\\d-\\d\\d-\\d\\d$", RegexOptions.None, new TimeSpan(0, 0, 0, 1)))
        {
            domainErrors.Add(new Error("StartDate", "Enter the <b>start date</b> using the format yyyy-mm-dd, for example 2017-09-01"));
            return domainErrors;
        }
        var startDate = csvRecord.StartDate;
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
            if (startDate > academicYearDateProvider.CurrentAcademicYearEndDate.AddYears(1))
            {
                domainErrors.Add(new Error("StartDate", "The <b>start date</b> must be no later than one year after the end of the current teaching year"));
            }

            var standard = GetStandardDetails(csvRecord.CourseCode);
            if (standard == null)
            {
                return domainErrors;
            }
                
            if (standard.EffectiveFrom.HasValue &&
                startDate < standard.EffectiveFrom.Value)
            {
                var prevMonth = standard.EffectiveFrom.Value.AddMonths(-1);
                domainErrors.Add(new Error("StartDate", $"This training course is only available to apprentices with a <b>start date</b> after {prevMonth.Month}  {prevMonth.Year}"));
            }

            if (standard.EffectiveTo.HasValue &&
                startDate > standard.EffectiveTo.Value)
            {
                var nextMonth = standard.EffectiveTo.Value.AddMonths(1);
                domainErrors.Add(new Error("StartDate", $"This training course is only available to apprentices with a <b>start date</b> before {nextMonth.Month}  {nextMonth.Year}"));
            }
        }
        else
        {
            domainErrors.Add(new Error("StartDate", "You must enter the <b>start date</b>, for example 2017-09-01"));
        }

        return domainErrors;
    }

    private static bool IsBeforeMay2017(DateTime startDateAsString)
    {
        return startDateAsString < Constants.DasStartDate;
    }

    private bool IsBeforeMay2018AndIsCohortIsTransferFunded(DateTime startDateAsString, string cohortRef)
    {
        var cohortDetails = GetCohortDetails(cohortRef);
        if (cohortDetails != null)
        {
            return cohortDetails.TransferSenderId.HasValue && startDateAsString < Constants.TransferFeatureStartDate;
        }

        return false;
    }
}
﻿using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Text.RegularExpressions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private static IEnumerable<Error> ValidateDateOfBirth(BulkUploadAddDraftApprenticeshipRequest csvRecord)
    {
        var domainErrors = new List<Error>();
           
        if (string.IsNullOrEmpty(csvRecord.DateOfBirthAsString))
        {
            domainErrors.Add(new Error("DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23"));
        }
        else if (!Regex.IsMatch(csvRecord.DateOfBirthAsString, "^\\d\\d\\d\\d-\\d\\d-\\d\\d$", RegexOptions.None, new TimeSpan(0, 0, 0, 1)))
        {
            domainErrors.Add(new Error("DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23"));

        }
        else
        {
            var dateOfBirth = csvRecord.DateOfBirth;
            if (dateOfBirth == null)
            {
                domainErrors.Add(new Error("DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23"));
            }
            else if (!WillApprenticeBeAtLeast15AtStartOfTraining(csvRecord.StartDate, dateOfBirth.Value))
            {
                domainErrors.Add(new Error("DateOfBirth", "The apprentice's <b>date of birth</b> must show that they are at least 15 years old at the start of their training"));
            }
            else if (!ApprenticeAgeMustBeLessThen115AtStartOfTraining(csvRecord.StartDate, dateOfBirth.Value))
            {
                domainErrors.Add(new Error("DateOfBirth", "The apprentice's <b>date of birth</b> must show that they are not older than 115 years old at the start of their training"));
            }
        }

        return domainErrors;
    }

    private static bool WillApprenticeBeAtLeast15AtStartOfTraining(DateTime? startDate, DateTime dobDate)
    {
        if (startDate == null) return true; // Don't fail validation if both fields not set

        var age = startDate.Value.Year - dobDate.Year;
        if (startDate < dobDate.AddYears(age)) age--;

        return age >= 15;
    }

    private static bool ApprenticeAgeMustBeLessThen115AtStartOfTraining(DateTime? startDate, DateTime dobDate)
    {
        if (startDate == null)
        {
            // Don't fail validation if both fields not set
            return true;
        }

        var age = startDate.Value.Year - dobDate.Year;
        if (startDate < dobDate.AddYears(age)) age--;

        return age < 115;
    }
}
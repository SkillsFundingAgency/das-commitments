using System.Text.RegularExpressions;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

public partial class BulkUploadValidateCommandHandler
{
    private static IEnumerable<Error> ValidateDateOfBirth(BulkUploadAddDraftApprenticeshipRequest csvRecord, ProviderStandardResults providerStandardResults)
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
            else
            {
                var courseCode = csvRecord.CourseCode;
                int? courseLevel = null;
                if (!string.IsNullOrEmpty(courseCode))
                {
                    courseLevel = providerStandardResults?.Standards?.FirstOrDefault(x => x.CourseCode == courseCode)?.Level;
                }

                if (!WillApprenticeBeAtLeastMinAgeAtStartOfTraining(csvRecord.StartDate, dateOfBirth.Value, csvRecord.MinimumAgeAtApprenticeshipStart ?? Constants.MinimumAgeAtApprenticeshipStart))
                {
                    domainErrors.Add(new Error("DateOfBirth", $"The apprentice's <b>date of birth</b> must show that they are at least {csvRecord.MinimumAgeAtApprenticeshipStart ?? Constants.MinimumAgeAtApprenticeshipStart} years old at the start of their training"));
                }
                else if (courseLevel == 7 && csvRecord.StartDate >= new DateTime(2026, 01, 01) && !ApprenticeAgeMustBeLessThenMaxAgeAtStartOfTraining(csvRecord.StartDate, dateOfBirth.Value, Constants.MaximumAgeAtApprenticeshipStartForLevel7))
                {
                    domainErrors.Add(new Error("DateOfBirth", $"The apprentice's <b>date of birth</b> must show that they are not older than {Constants.MaximumAgeAtApprenticeshipStartForLevel7} years old at the start of their training"));
                }
                else if (!ApprenticeAgeMustBeLessThenMaxAgeAtStartOfTraining(csvRecord.StartDate, dateOfBirth.Value, csvRecord.MaximumAgeAtApprenticeshipStart ?? Constants.MaximumAgeAtApprenticeshipStart))
                {
                    domainErrors.Add(new Error("DateOfBirth", $"The apprentice's <b>date of birth</b> must show that they are not older than {csvRecord.MaximumAgeAtApprenticeshipStart ?? Constants.MaximumAgeAtApprenticeshipStart} years old at the start of their training"));
                }
            }
        }

        return domainErrors;
    }

    private static bool WillApprenticeBeAtLeastMinAgeAtStartOfTraining(DateTime? startDate, DateTime dobDate, int minAge)
    {
        if (startDate == null) return true; // Don't fail validation if both fields not set

        var age = startDate.Value.Year - dobDate.Year;
        if (startDate < dobDate.AddYears(age)) age--;

        return age >= minAge;
    }

    private static bool ApprenticeAgeMustBeLessThenMaxAgeAtStartOfTraining(DateTime? startDate, DateTime dobDate, int maxAge)
    {
        if (startDate == null)
        {
            // Don't fail validation if both fields not set
            return true;
        }

        var age = startDate.Value.Year - dobDate.Year;
        if (startDate < dobDate.AddYears(age)) age--;

        return age < maxAge;
    }
}
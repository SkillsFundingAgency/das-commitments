using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Domain;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private static IEnumerable<LearnerError> ValidateDateOfBirth(LearnerDataEnhanced record)
    {
        var domainErrors = new List<LearnerError>();
           
        if (!WillApprenticeBeAtLeastMinAgeAtStartOfTraining(record.StartDate, record.Dob, record.MinimumAgeAtApprenticeshipStart ?? Constants.MinimumAgeAtApprenticeshipStart))
        {
            domainErrors.Add(new LearnerError("DateOfBirth", $"The apprentice's date of birth must show that they are at least {record.MinimumAgeAtApprenticeshipStart ?? Constants.MinimumAgeAtApprenticeshipStart} years old at the start of their training"));
        }
        else if (!ApprenticeAgeMustBeLessThenMaxAgeAtStartOfTraining(record.StartDate, record.Dob, record.MaximumAgeAtApprenticeshipStart ?? Constants.MaximumAgeAtApprenticeshipStart))
        {
            domainErrors.Add(new LearnerError("DateOfBirth", $"The apprentice's date of birth must show that they are not older than {record.MaximumAgeAtApprenticeshipStart ?? Constants.MaximumAgeAtApprenticeshipStart} years old at the start of their training"));
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
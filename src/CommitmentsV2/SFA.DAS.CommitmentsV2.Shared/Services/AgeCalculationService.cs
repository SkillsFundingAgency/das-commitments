using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Shared.Services;

public class AgeCalculationService : IAgeCalculationService
{
    public int? CalculateLearnerAgeComparedToASpecificDate(DateTime? startDate, DateTime? dateOfBirth)
    {
        if (startDate.HasValue && dateOfBirth.HasValue)
        {
            var age = startDate.Value.Year - dateOfBirth.Value.Year;

            if ((dateOfBirth.Value.Month > startDate.Value.Month) ||
                (dateOfBirth.Value.Month == startDate.Value.Month &&
                 dateOfBirth.Value.Day > startDate.Value.Day))
                age--;

            return age;
        }

        return null;
    }

    public bool WillLearnerBeAtLeastMinAgeAtStartOfTraining(DateTime? startDate, DateTime? dateOfBirth, int minAge)
    {
        if (startDate == null || dateOfBirth == null)
        {
            return false;
        }

        var age = startDate.Value.Year - dateOfBirth.Value.Year;
        if (startDate < dateOfBirth.Value.AddYears(age)) age--;

        return age >= minAge;
    }

    public bool LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(DateTime? startDate, DateTime? dateOfBirth, int maxAge)
    {
        if (startDate == null || dateOfBirth == null)
        {
            return false;
        }

        var age = startDate.Value.Year - dateOfBirth.Value.Year;
        if (startDate < dateOfBirth.Value.AddYears(age)) age--;

        return age < maxAge;
    }
}
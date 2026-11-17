namespace SFA.DAS.CommitmentsV2.Shared.Interfaces;

public interface IAgeCalculationService
{
    int? CalculateLearnerAgeComparedToASpecificDate(DateTime? startDate, DateTime? dateOfBirth);

    bool WillLearnerBeAtLeastMinAgeAtStartOfTraining(DateTime? startDate, DateTime? dateOfBirth, int minAge);

    bool LearnerAgeMustBeLessThenMaxAgeAtStartOfTraining(DateTime? startDate, DateTime? dateOfBirth, int maxAge);
}

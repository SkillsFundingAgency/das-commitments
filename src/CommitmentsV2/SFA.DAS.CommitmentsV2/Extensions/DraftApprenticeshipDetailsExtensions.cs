using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class DraftApprenticeshipDetailsExtensions
{
    public static DraftApprenticeshipDetails ApplyTrainingCourseVersion(this DraftApprenticeshipDetails details, string version, string standardUId, bool populateStandardVersion)
    {
        details.LearningType ??= details.TrainingProgramme?.LearningType;

        if (details.LearningType == Types.LearningType.ApprenticeshipUnit)
        {
            details.TrainingCourseVersion = version;
            details.StandardUId = standardUId;
            details.TrainingCourseVersionConfirmed = !string.IsNullOrEmpty(version);
        }
        else if (populateStandardVersion)
        {
            details.TrainingCourseVersion = details.TrainingProgramme?.Version;
            details.StandardUId = details.TrainingProgramme?.StandardUId;
            details.TrainingCourseVersionConfirmed = details.TrainingProgramme?.ProgrammeType == Types.ProgrammeType.Standard;
        }

        return details;
    }

    public static DateTime? GetStartDate(this DraftApprenticeshipDetails details)
    {
        return details.ActualStartDate ?? details.StartDate;
    }
}
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class DraftApprenticeshipDetailsExtensions
{
    public static DraftApprenticeshipDetails ApplyTrainingCourseVersion(this DraftApprenticeshipDetails details, string courseCode, string version, string standardUId, bool populateStandardVersion)
    {
        if (!int.TryParse(courseCode, out _) && details.TrainingProgramme?.ProgrammeType == Types.ProgrammeType.Standard)
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
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Extensions;

public static class DraftApprenticeshipDetailsExtensions
{
    public static void ApplyApprenticeshipUnitVersion(this DraftApprenticeshipDetails details, string courseCode, string version, string standardUId)
    {
        if (!int.TryParse(courseCode, out _) && details.TrainingProgramme?.ProgrammeType == Types.ProgrammeType.Standard)
        {
            details.TrainingCourseVersion = version;
            details.StandardUId = standardUId;
            details.TrainingCourseVersionConfirmed = !string.IsNullOrEmpty(version);
        }
    }

    public static DateTime? GetStartDate(this DraftApprenticeshipDetails details)
    {
        return details.ActualStartDate ?? details.StartDate;
    }
}
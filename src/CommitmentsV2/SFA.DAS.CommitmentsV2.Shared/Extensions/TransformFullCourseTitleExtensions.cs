using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Shared.Extensions
{
    public static class TransformFullCourseTitleExtensions
    {
        public static string TransformFullCourseTitle(this string title, ProgrammeType programmeType)
        {
            if (programmeType == ProgrammeType.Standard)
            {
                title = title.Replace(" (Standard)", "");
            }
            else if (programmeType == ProgrammeType.Framework && !title.Contains("(Framework)"))
            {
                title = title.Trim() + " (Framework)";
            }

            return title;
        }
    }
}

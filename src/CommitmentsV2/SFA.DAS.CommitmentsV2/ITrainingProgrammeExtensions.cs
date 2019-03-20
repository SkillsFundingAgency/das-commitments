using System;
using System.Collections.Generic;
using System.Text;
using SFA.DAS.Apprenticeships.Api.Types;

namespace SFA.DAS.CommitmentsV2
{
    public static class ITrainingProgrammeExtensions
    {
        public static string ExtendedTitle(this ITrainingProgramme course)
        {
            switch (course)
            {
                case Framework framework:
                    return GetTitle(string.Equals(framework.FrameworkName.Trim(), framework.PathwayName.Trim(), StringComparison.OrdinalIgnoreCase) ? framework.FrameworkName : framework.Title,
                        framework.Level);
                case Standard standard:
                    return GetTitle(standard.Title, standard.Level) + " (Standard)";
                default:
                    return course.Title;
            }
        }

        private static string GetTitle(string title, int level)
        {
            return $"{title}, Level: {level}";
        }
    }
}

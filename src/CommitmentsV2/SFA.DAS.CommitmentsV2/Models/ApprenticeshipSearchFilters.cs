using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipSearchFilters
    {
        public string EmployerName { get; set; }
        public string CourseName { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool HasFilters =>
            !string.IsNullOrEmpty(EmployerName) ||
            !string.IsNullOrEmpty(CourseName) ||
            !string.IsNullOrEmpty(Status) ||
            StartDate.HasValue ||
            EndDate.HasValue;
    }
}

using System;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipSearchFilters
    {
        public string SearchTerm { get; set; }
        public string EmployerName { get; set; }
        public string CourseName { get; set; }
        public string Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}

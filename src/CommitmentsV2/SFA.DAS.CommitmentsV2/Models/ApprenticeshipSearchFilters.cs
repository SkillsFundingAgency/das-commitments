﻿using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Models
{
    public class ApprenticeshipSearchFilters
    {
        public string EmployerName { get; set; }
        public string CourseName { get; set; }
        public ApprenticeshipStatus? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public bool HasFilters =>
            !string.IsNullOrEmpty(EmployerName) ||
            !string.IsNullOrEmpty(CourseName) ||
            Status.HasValue ||
            StartDate.HasValue ||
            EndDate.HasValue;
    }
}

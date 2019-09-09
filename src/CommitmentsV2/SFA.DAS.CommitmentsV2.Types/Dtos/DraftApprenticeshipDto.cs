﻿using System;

namespace SFA.DAS.CommitmentsV2.Types.Dtos
{
    public class DraftApprenticeshipDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Uln { get; set; }
        public string CourseCode { get; set; }
        public string CourseName { get; set; }
    }
}

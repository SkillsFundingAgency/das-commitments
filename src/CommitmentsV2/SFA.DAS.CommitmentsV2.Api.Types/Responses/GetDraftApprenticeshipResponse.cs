﻿using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public sealed class GetDraftApprenticeshipResponse
    {
        public long Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Uln { get; set; }
        public string CourseCode { get; set; }
        public DeliveryModel DeliveryModel { get; set; }
        public string TrainingCourseName { get; set; }
        public string TrainingCourseVersion { get; set; }
        public string TrainingCourseOption { get; set; }
        public bool TrainingCourseVersionConfirmed { get; set; }
        public string StandardUId { get; set; }
        public int? Cost { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Reference { get; set; }
        public Guid? ReservationId { get; set; }
        public bool IsContinuation { get; set; }
        public DateTime? OriginalStartDate { get; set; }
        public bool HasStandardOptions { get; set; }
    }
}
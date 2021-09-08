﻿using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetAllLearnersResponse
    {
        public int BatchNumber { get; set; }
        public int BatchSize { get; set; }
        public int TotalNumberOfBatches { get; set; }
        public List<Learner> Learners { get; set; }
    }

    public class Learner
    {
        public long ApprenticeshipId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ULN { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingCourseVersion { get; set; }
        public bool TrainingCourseVersionConfirmed { get; set; }
        public string TrainingCourseOption { get; set; }
        public string StandardUId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime? UpdatedOn { get; set; }
        public DateTime? StopDate { get; set; }
        public DateTime? PauseDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public long UKPRN { get; set; }
        public string LearnRefNumber { get; set; }
        public short PaymentStatus { get; set; }
    }
}

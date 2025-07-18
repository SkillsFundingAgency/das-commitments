﻿using System;
using System.Collections.Generic;
using SFA.DAS.Learning.Types.Enums;

namespace SFA.DAS.Learning.Types.Models
{
    public class LearningEpisode
    {
        public Guid Key { get; set; }
        public long Ukprn { get; set; }
        public long EmployerAccountId { get; set; }
        public FundingType FundingType { get; set; }
        public FundingPlatform? FundingPlatform { get; set; }
        public long? FundingEmployerAccountId { get; set; }
        public string LegalEntityName { get; set; }
        public long? AccountLegalEntityId { get; set; }
        public int AgeAtStartOfApprenticeship { get; set; }
        public string TrainingCode { get; set; }
        public string TrainingCourseVersion { get; set; }
        public bool PaymentsFrozen { get; set; }
        public List<LearningEpisodePrice> Prices { get; set; }
    }
}
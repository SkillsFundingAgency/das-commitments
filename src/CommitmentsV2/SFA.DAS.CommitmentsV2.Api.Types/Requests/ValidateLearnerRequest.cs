using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Requests
{
    public class ValidateLearnerRequest : SaveDataRequest
    {
        public long ProviderId { get; set; }
        public long LearnerDataId { get; set; }
        public LearnerData Learner { get; set; }
        public ProviderStandardResults ProviderStandardsData { get; set; }
        public Dictionary<string, int?> OtjTrainingHours { get; set; }
    }

    public class LearnerData
    {
        public long Uln { get; set; }
        public long Ukprn { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime Dob { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public int EpaoPrice { get; set; }
        public int TrainingPrice { get; set; }
        public int StandardCode { get; set; }
        public bool IsFlexiJob { get; set; }
        public int PlannedOTJTrainingHours { get; set; }

    }

}



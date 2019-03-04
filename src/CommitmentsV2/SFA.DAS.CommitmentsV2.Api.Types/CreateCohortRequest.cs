using System;

// Comment. I think there is a benefit and simplicity in keeping models within Request and Response types as inner classes. It does duplicate the interfaces, 
// but it makes it clear that the request only requires these explicit fields

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    public sealed class CreateCohortRequest
    {
        public string UserId { get; set; }

        public NewCohort Cohort { get; set; }

        public NewDraftApprenticeship DraftApprenticeship { get; set; }

        // Does this need teh Names? 
        public sealed class NewCohort
        {
            public long EmployerAccountId { get; set; }
            public string LegalEntityId { get; set; }
            public long ProviderId { get; set; }
        }

        public sealed class NewDraftApprenticeship
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string ULN { get; set; }
            public string TrainingCode { get; set; }
            public string TrainingName { get; set; }
            public int? Cost { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string ProviderReference { get; set; }
        }
    }

}

using System;

// Comment. I think there is a benefit and simplicity in keeping models within Request and Response types as inner classes. It does duplicate the interfaces, 
// but it makes it clear that the request only requires these explicit fields

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    public sealed class NewCohortWithSingleApprenticeRequest
    {
        public string UserId { get; set; }

        public NewCohortDetails CohortDetails { get; set; }

        public NewApprenticeshipDetails ApprenticeshipDetails { get; set; }

        // Does this need teh Names? 
        public sealed class NewCohortDetails
        {
            public string CohortReference { get; set; }
            public long EmployerAccountId { get; set; }
            public string LegalEntityId { get; set; }
            public long? ProviderId { get; set; }

            // Not sure whether we need to pass in the names here? 

        }

        public sealed class NewApprenticeshipDetails
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string ULN { get; set; }

            // I assume this will be needed (can't see it in provider commitments)
            //public TrainingType TrainingType { get; set; }
            public string TrainingCode { get; set; }
            public string TrainingName { get; set; }
            public decimal? Cost { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string ProviderReference { get; set; }
        }
    }

}

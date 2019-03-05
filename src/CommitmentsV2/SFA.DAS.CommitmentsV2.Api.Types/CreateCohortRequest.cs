using System;

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    public sealed class CreateCohortRequest
    {
        public string UserId { get; set; }

        public NewCohort Cohort { get; set; }

        public NewDraftApprenticeship DraftApprenticeship { get; set; }

        public sealed class NewCohort
        {
            public long EmployerAccountId { get; set; }
            public string LegalEntityId { get; set; }
            public int ProviderId { get; set; }
        }

        public sealed class NewDraftApprenticeship
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public DateTime? DateOfBirth { get; set; }
            public string ULN { get; set; }
            public string CourseCode { get; set; }
            public int? Cost { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public string OriginatorReference { get; set; }
            public Guid? ReservationId { get; set; }
        }
    }

}

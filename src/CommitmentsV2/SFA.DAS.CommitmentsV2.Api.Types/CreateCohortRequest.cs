using System;

namespace SFA.DAS.CommitmentsV2.Api.Types
{
    public sealed class CreateCohortRequest
    {
        public string UserId { get; set; }
        public long AccountLegalEntityId { get; set; }
        public int ProviderId { get; set; }

        public DraftApprenticeshipDetails DraftApprenticeship { get; set; }

        public sealed class DraftApprenticeshipDetails
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
            public Guid ReservationId { get; set; }
        }
    }

}

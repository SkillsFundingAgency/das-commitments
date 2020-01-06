using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetApprenticeshipUpdateResponse
    {
        public ApprenticeshipUpdate PendingApprenticeshipUpdate { get; set; }

        public class ApprenticeshipUpdate
        {
            public long Id { get; set; }
            public long ApprenticeshipId { get; set; }
            public Originator Originator { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public ProgrammeType? TrainingType { get; set; }
            public string TrainingCode { get; set; }
            public string TrainingName { get; set; }
            public decimal? Cost { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public DateTime? DateOfBirth { get; set; }
        }
    }
}

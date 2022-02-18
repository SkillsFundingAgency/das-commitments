using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetApprenticeshipUpdatesResponse
    {
        public IReadOnlyCollection<ApprenticeshipUpdate> ApprenticeshipUpdates { get; set; }

        public class ApprenticeshipUpdate
        {
            public long Id { get; set; }
            public long ApprenticeshipId { get; set; }
            public Party OriginatingParty { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string Email { get; set; }
            public DeliveryModelDto DeliveryModel { get; set; }
            public ProgrammeType? TrainingType { get; set; }
            public string TrainingCode { get; set; }
            public string Version { get; set; }
            public string Option { get; set; }
            public string TrainingName { get; set; }
            public decimal? Cost { get; set; }
            public DateTime? StartDate { get; set; }
            public DateTime? EndDate { get; set; }
            public DateTime? DateOfBirth { get; set; }
        }
    }
}

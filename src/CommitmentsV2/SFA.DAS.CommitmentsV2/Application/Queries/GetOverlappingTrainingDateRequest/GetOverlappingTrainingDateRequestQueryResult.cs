using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest
{
    public class GetOverlappingTrainingDateRequestQueryResult
    {
        public long Id { get; set; }
        public long DraftApprenticeshipId { get; set; }
        public long PreviousApprenticeshipId { get; set; }
        public OverlappingTrainingDateRequestResolutionType? ResolutionType { get; set; }
        public OverlappingTrainingDateRequestStatus Status { get; set; }
        public DateTime? ActionedOn { get; set; }
    }
}
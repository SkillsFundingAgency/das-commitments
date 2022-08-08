using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses
{
    public class GetOverlappingTrainingDateRequestResponce
    {
        public long Id { get; set; }
        public long DraftApprenticeshipId { get; set; }
        public long PreviousApprenticeshipId { get; set; }
        public OverlappingTrainingDateRequestResolutionType? ResolutionType { get; set; }
        public OverlappingTrainingDateRequestStatus Status { get; set; }
        public OverlappingTrainingDateRequestEmployerAction? EmployerAction { get; set; }
        public DateTime? ActionedOn { get; set; }
    }
}
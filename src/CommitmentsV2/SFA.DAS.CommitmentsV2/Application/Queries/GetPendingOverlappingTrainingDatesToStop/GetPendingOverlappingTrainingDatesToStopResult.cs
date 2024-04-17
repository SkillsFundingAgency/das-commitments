using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetPendingOverlappingTrainingDatesToStop
{
    public class GetPendingOverlappingTrainingDatesToStopResult
    {
        public List<Models.OverlappingTrainingDateRequest> OverlappingTrainingDateRequests { get; set; }
    }
}

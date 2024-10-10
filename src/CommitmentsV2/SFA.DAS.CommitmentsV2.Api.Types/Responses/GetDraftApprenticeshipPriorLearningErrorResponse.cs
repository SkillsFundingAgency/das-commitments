using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public sealed class GetCohortPriorLearningErrorResponse
{
    public IEnumerable<long> DraftApprenticeshipIds { get; set; }
}
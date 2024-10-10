using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetDraftApprenticeshipsResponse
{
    public IReadOnlyCollection<DraftApprenticeshipDto> DraftApprenticeships { get; set; }
}
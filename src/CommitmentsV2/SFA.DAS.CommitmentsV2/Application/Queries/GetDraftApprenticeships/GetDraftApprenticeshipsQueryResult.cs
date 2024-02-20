using SFA.DAS.CommitmentsV2.Types.Dtos;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeships
{
    public class GetDraftApprenticeshipsQueryResult
    {
        public IReadOnlyCollection<DraftApprenticeshipDto> DraftApprenticeships { get; set; }
    }
}

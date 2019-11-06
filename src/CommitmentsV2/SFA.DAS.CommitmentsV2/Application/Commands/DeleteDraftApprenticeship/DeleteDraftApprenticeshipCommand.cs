using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.DeleteDraftApprenticeship
{
    public class DeleteDraftApprenticeshipCommand : IRequest<DeleteDraftApprenticeshipResponse>
    {
        public long CohortId { get; set; }
        public long ApprenticeshipId { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}

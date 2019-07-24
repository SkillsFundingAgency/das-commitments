using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort
{
    public class AddCohortWithOtherPartyCommand : IRequest<AddCohortResponse>
    {
        public long AccountLegalEntityId { get; set; }
        public long ProviderId { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}

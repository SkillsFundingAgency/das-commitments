using MediatR;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.RejectApprenticeshipUpdates
{
    public class RejectApprenticeshipUpdatesCommand : IRequest
    {
        public long AccountId { get; set; }
        public long ApprenticeshipId { get; set; }
        public UserInfo UserInfo { get; set; }
    }
}

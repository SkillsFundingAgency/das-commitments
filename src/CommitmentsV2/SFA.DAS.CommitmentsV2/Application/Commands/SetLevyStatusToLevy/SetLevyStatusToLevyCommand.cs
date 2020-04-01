using MediatR;

namespace SFA.DAS.CommitmentsV2.Application.Commands.SetLevyStatusToLevy
{
    public class SetLevyStatusToLevyCommand : IRequest
    {
        public long AccountId { get; set; }
    }
}

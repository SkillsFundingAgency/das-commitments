using FluentValidation.Attributes;
using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.RejectApprenticeshipChange
{
    [Validator(typeof(RejectApprenticeshipChangeValidator))]
    public class RejectApprenticeshipChangeCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }

        public string UserId { get; set; }

        public Caller Caller { get; set; }
        public string UserName { get; set; }
    }
}

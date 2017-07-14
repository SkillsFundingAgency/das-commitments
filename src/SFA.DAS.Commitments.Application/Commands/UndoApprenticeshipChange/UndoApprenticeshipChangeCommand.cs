using FluentValidation.Attributes;
using MediatR;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.UndoApprenticeshipChange
{
    [Validator(typeof(UndoApprenticeshipChangeValidator))]
    public class UndoApprenticeshipChangeCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }

        public string UserId { get; set; }

        public Caller Caller { get; set; }
        public string UserName { get; set; }
    }
}

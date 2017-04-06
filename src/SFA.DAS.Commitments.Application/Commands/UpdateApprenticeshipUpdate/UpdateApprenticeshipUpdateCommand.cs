using FluentValidation.Attributes;

using MediatR;

using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.UpdateApprenticeshipUpdate
{
    [Validator(typeof(UpdateApprenticeshipUpdateValidator))]
    public class UpdateApprenticeshipUpdateCommand : IAsyncRequest
    {
        public long ApprenticeshipId { get; set; }

        public ApprenticeshipUpdateStatus UpdateStatus { get; set; }

        public string UserId { get; set; }

        public Caller Caller { get; set; }
    }
}
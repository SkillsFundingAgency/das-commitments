using FluentValidation.Attributes;
using MediatR;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate
{
    [Validator(typeof(CreateApprenticeshipUpdateValidator))]
    public class CreateApprenticeshipUpdateCommand: IAsyncRequest
    {
        public Caller Caller { get; set; }
        public ApprenticeshipUpdate ApprenticeshipUpdate { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
    }
}

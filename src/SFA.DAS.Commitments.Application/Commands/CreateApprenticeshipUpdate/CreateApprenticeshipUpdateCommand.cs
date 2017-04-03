using FluentValidation.Attributes;
using MediatR;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Application.Commands.CreateApprenticeshipUpdate
{
    [Validator(typeof(CreateApprenticeshipUpdateValidator))]
    public class CreateApprenticeshipUpdateCommand: IAsyncRequest
    {
        public Caller Caller { get; set; }
        public ApprenticeshipUpdate ApprenticeshipUpdate { get; set; }
        public string UserId { get; set; }
    }
}

using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest
{
    public class CreateChangeOfPartyRequestCommandHandler : IRequestHandler<CreateChangeOfPartyRequestCommand>
    {
        private readonly IChangeOfPartyRequestDomainService _changeOfPartyRequestDomainService;

        public CreateChangeOfPartyRequestCommandHandler(IChangeOfPartyRequestDomainService changeOfPartyRequestDomainService)
        {
            _changeOfPartyRequestDomainService = changeOfPartyRequestDomainService;
        }

        public Task Handle(CreateChangeOfPartyRequestCommand command, CancellationToken cancellationToken)
        {
            return _changeOfPartyRequestDomainService.CreateChangeOfPartyRequest(command.ApprenticeshipId,
                command.ChangeOfPartyRequestType, command.NewPartyId, command.NewPrice, command.NewStartDate, command.NewEndDate,
                command.UserInfo, command.NewEmploymentPrice, command.NewEmploymentEndDate, command.DeliveryModel, cancellationToken);
        }
    }
}
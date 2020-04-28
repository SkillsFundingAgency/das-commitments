using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest
{
    public class CreateChangeOfPartyRequestCommandHandler : AsyncRequestHandler<CreateChangeOfPartyRequestCommand>
    {
        private readonly IChangeOfPartyRequestDomainService _changeOfPartyRequestDomainService;

        public CreateChangeOfPartyRequestCommandHandler(IChangeOfPartyRequestDomainService changeOfPartyRequestDomainService)
        {
            _changeOfPartyRequestDomainService = changeOfPartyRequestDomainService;
        }

        protected override Task Handle(CreateChangeOfPartyRequestCommand command, CancellationToken cancellationToken)
        {
            return _changeOfPartyRequestDomainService.CreateChangeOfPartyRequest(command.ApprenticeshipId,
                command.ChangeOfPartyRequestType, command.NewPartyId, command.NewPrice, command.NewStartDate, null,
                command.UserInfo, cancellationToken);
        }
    }
}
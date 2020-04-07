using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ChangeOfPartyRequest
{
    public class ChangeOfPartyRequestCommandHandler : AsyncRequestHandler<ChangeOfPartyRequestCommand>
    {
        private readonly IChangeOfPartyRequestDomainService _changeOfPartyRequestDomainService;

        public ChangeOfPartyRequestCommandHandler(IChangeOfPartyRequestDomainService changeOfPartyRequestDomainService, ILogger<ChangeOfPartyRequestCommandHandler> logger)
        {
            _changeOfPartyRequestDomainService = changeOfPartyRequestDomainService;
        }

        protected override Task Handle(ChangeOfPartyRequestCommand command, CancellationToken cancellationToken)
        {
            return _changeOfPartyRequestDomainService.CreateChangeOfPartyRequest(command.ApprenticeshipId,
                command.ChangeOfPartyRequestType, command.NewPartyId, command.NewPrice.Value, command.NewStartDate.Value, null,
                command.UserInfo, cancellationToken);
        }
    }
}
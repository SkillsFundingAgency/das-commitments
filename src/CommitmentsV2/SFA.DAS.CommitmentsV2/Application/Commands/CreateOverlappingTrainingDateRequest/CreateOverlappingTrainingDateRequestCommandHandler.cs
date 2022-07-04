using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.CreateOverlappingTrainingDateRequest
{
    internal class CreateOverlappingTrainingDateRequestCommandHandler : AsyncRequestHandler<CreateOverlappingTrainingDateRequestCommand>
    {
        private readonly IChangeOfPartyRequestDomainService _changeOfPartyRequestDomainService;

        public CreateOverlappingTrainingDateRequestCommandHandler(IChangeOfPartyRequestDomainService changeOfPartyRequestDomainService)
        {
            _changeOfPartyRequestDomainService = changeOfPartyRequestDomainService;
        }

        protected override Task Handle(CreateOverlappingTrainingDateRequestCommand request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}

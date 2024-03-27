using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateChangeOfEmployerOverlap
{
    public class ValidateChangeOfEmployerOverlapCommandHandler : IRequestHandler<ValidateChangeOfEmployerOverlapCommand>
    {
        private readonly IChangeOfPartyRequestDomainService _changeOfPartyRequestDomainService;

        public ValidateChangeOfEmployerOverlapCommandHandler(
           IChangeOfPartyRequestDomainService changeOfPartyRequestDomainService)
        {
            _changeOfPartyRequestDomainService = changeOfPartyRequestDomainService;
        }

        public async Task Handle(ValidateChangeOfEmployerOverlapCommand request, CancellationToken cancellationToken)
        {
            var stDate = System.DateTime.ParseExact(request.StartDate, "dd-MM-yyyy", null);
            var edDate = System.DateTime.ParseExact(request.EndDate, "dd-MM-yyyy", null);

            await _changeOfPartyRequestDomainService.ValidateChangeOfEmployerOverlap(request.Uln, stDate, edDate, cancellationToken);
        }
    }
}
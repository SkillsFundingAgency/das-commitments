using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateChangeOfEmployerOverlap;

public class ValidateChangeOfEmployerOverlapCommandHandler(IChangeOfPartyRequestDomainService changeOfPartyRequestDomainService)
    : IRequestHandler<ValidateChangeOfEmployerOverlapCommand>
{
    public async Task Handle(ValidateChangeOfEmployerOverlapCommand request, CancellationToken cancellationToken)
    {
        var stDate = DateTime.ParseExact(request.StartDate, "dd-MM-yyyy", null);
        var edDate = DateTime.ParseExact(request.EndDate, "dd-MM-yyyy", null);

        await changeOfPartyRequestDomainService.ValidateChangeOfEmployerOverlap(request.Uln, stDate, edDate, cancellationToken);
    }
}
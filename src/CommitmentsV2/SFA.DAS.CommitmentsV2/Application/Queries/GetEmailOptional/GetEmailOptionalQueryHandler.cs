using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOptional;

public class GetEmailOptionalQueryHandler(IEmailOptionalService emailService) : IRequestHandler<GetEmailOptionalQuery, bool>
{
    public Task<bool> Handle(GetEmailOptionalQuery request, CancellationToken cancellationToken)
    {
        var res = emailService.ApprenticeEmailIsOptionalFor(request.EmployerId, request.ProviderId);

        return Task.FromResult(res);
    }
}
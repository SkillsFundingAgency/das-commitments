using MediatR;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOptional
{
    public class GetEmailOptionalQueryHandler : IRequestHandler<GetEmailOptionalQuery, bool>
    {
        private readonly IEmailOptionalService _emailService;

        public GetEmailOptionalQueryHandler(IEmailOptionalService emailService)
            => _emailService = emailService;

        public Task<bool> Handle(GetEmailOptionalQuery request, CancellationToken cancellationToken)
        {
            var res = _emailService.ApprenticeEmailIsOptionalFor(request.EmployerId, request.ProviderId);

            return Task.FromResult(res);
        }
    }
}

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Types.Dtos;
using SFA.DAS.CommitmentsV2.Configuration;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetEmailOptional
{
    public class GetEmailOptionalQueryHandler : IRequestHandler<GetEmailOptionalQuery, bool>
    {
        private readonly EmailOptionalConfiguration _config;

        public GetEmailOptionalQueryHandler(EmailOptionalConfiguration config)
            => _config = config;

        public Task<bool> Handle(GetEmailOptionalQuery request, CancellationToken cancellationToken)
        {
            var res = _config.EmailOptionalEmployers.Any(x => x == request.EmployerId) ||
                            _config.EmailOptionalProviders.Any(x => x == request.ProviderId);

            return Task.FromResult(res);
        }
    }
}

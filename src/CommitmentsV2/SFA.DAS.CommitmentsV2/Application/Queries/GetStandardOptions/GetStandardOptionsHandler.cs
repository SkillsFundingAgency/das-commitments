using MediatR;
using SFA.DAS.CommitmentsV2.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetStandardOptions
{
    public class GetStandardOptionsHandler : IRequestHandler<GetStandardOptionsQuery, GetStandardOptionsResult>
    {
        private Lazy<ProviderCommitmentsDbContext> _dbContext;

        public GetStandardOptionsHandler(Lazy<ProviderCommitmentsDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<GetStandardOptionsResult> Handle(GetStandardOptionsQuery request, CancellationToken cancellationToken)
        {
            var options = _dbContext.Value.StandardOptions.Where(o => o.StandardUId == request.StandardUId);

            return Task.FromResult(new GetStandardOptionsResult
            {
                Options = options.Select(o => o.Option).OrderBy(o => o)
            });
        }
    }
}

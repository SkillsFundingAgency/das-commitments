using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetLastSubmissionEventId
{
    public class GetLastSubmissionEventIdQueryHandler : IRequestHandler<GetLastSubmissionEventIdQuery, long?>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<GetLastSubmissionEventIdQueryHandler> _logger;

        public GetLastSubmissionEventIdQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<GetLastSubmissionEventIdQueryHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<long?> Handle(GetLastSubmissionEventIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _dbContext.Value.JobProgress.Select(x => x.AddEpaLastSubmissionEventId).FirstOrDefaultAsync();
            _logger.LogInformation($"Last SubmissionEventId processed by previous job run is {result ?? 0}");
            return result;
        }
    }
}

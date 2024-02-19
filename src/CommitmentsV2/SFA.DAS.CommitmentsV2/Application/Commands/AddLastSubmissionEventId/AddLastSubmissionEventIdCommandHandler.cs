using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddLastSubmissionEventId
{
    public class AddLastSubmissionEventIdCommandHandler : IRequestHandler<AddLastSubmissionEventIdCommand>
    {
        private readonly ILogger<AddLastSubmissionEventIdCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;

        public AddLastSubmissionEventIdCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<AddLastSubmissionEventIdCommandHandler> logger)
        {
            _logger = logger;
            _dbContext = dbContext;
        }
        public async Task Handle(AddLastSubmissionEventIdCommand request, CancellationToken cancellationToken)
        {
            var jobProgress = _dbContext.Value.JobProgress.FirstOrDefault(x => x.Lock == "X");
            if (jobProgress != null)
            {
                jobProgress.AddEpaLastSubmissionEventId = request.LastSubmissionEventId;
            }
            else
            {
               _dbContext.Value.JobProgress.Add(new Models.JobProgress { AddEpaLastSubmissionEventId = request.LastSubmissionEventId, Lock = "X" });    
            }

            await _dbContext.Value.SaveChangesAsync(cancellationToken);
        }
    }
}

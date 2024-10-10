using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetLastSubmissionEventId;

public class GetLastSubmissionEventIdQueryHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<GetLastSubmissionEventIdQueryHandler> logger)
    : IRequestHandler<GetLastSubmissionEventIdQuery, long?>
{
    public async Task<long?> Handle(GetLastSubmissionEventIdQuery request, CancellationToken cancellationToken)
    {
        var result = await dbContext.Value.JobProgress.Select(x => x.AddEpaLastSubmissionEventId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        
        logger.LogInformation("Last SubmissionEventId processed by previous job run is {Result}", result ?? 0);
        
        return result;
    }
}
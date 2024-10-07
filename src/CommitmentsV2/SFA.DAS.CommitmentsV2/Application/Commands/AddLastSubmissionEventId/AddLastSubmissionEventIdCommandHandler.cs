using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddLastSubmissionEventId;

public class AddLastSubmissionEventIdCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<AddLastSubmissionEventIdCommand>
{
    public async Task Handle(AddLastSubmissionEventIdCommand request, CancellationToken cancellationToken)
    {
        var jobProgress = await dbContext.Value.JobProgress.FirstOrDefaultAsync(x => x.Lock == "X", cancellationToken);
        
        if (jobProgress != null)
        {
            jobProgress.AddEpaLastSubmissionEventId = request.LastSubmissionEventId;
        }
        else
        {
            dbContext.Value.JobProgress.Add(new Models.JobProgress { AddEpaLastSubmissionEventId = request.LastSubmissionEventId, Lock = "X" });    
        }

        await dbContext.Value.SaveChangesAsync(cancellationToken);
    }
}
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddLastSubmissionEventId;

public class AddLastSubmissionEventIdCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext) : IRequestHandler<AddLastSubmissionEventIdCommand>
{
    public async Task Handle(AddLastSubmissionEventIdCommand request, CancellationToken cancellationToken)
    {
        var jobProgress = dbContext.Value.JobProgress.FirstOrDefault(x => x.Lock == "X");
        
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
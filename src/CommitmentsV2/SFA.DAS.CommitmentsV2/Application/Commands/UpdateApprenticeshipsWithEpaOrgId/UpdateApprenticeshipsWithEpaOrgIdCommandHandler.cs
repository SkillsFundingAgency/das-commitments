using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipsWithEpaOrgId;

public class UpdateApprenticeshipsWithEpaOrgIdCommandHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    ILogger<UpdateApprenticeshipsWithEpaOrgIdCommandHandler> logger)
    : IRequestHandler<UpdateApprenticeshipsWithEpaOrgIdCommand, long?>
{
    public async Task<long?> Handle(UpdateApprenticeshipsWithEpaOrgIdCommand command, CancellationToken cancellationToken)
    {
        foreach (var submissionEvent in command.SubmissionEvents)
        {
            if (!submissionEvent.ApprenticeshipId.HasValue)
            {
                logger.LogWarning("Ignoring SubmissionEvent '{SubmissionEventId}' with no ApprenticeshipId", submissionEvent.Id);
            }
            else
            {
                try
                {
                    var apprenticeship = await dbContext.Value.Apprenticeships.FirstOrDefaultAsync(x => x.Id == submissionEvent.ApprenticeshipId.Value, cancellationToken);
                    if (apprenticeship != null)
                    {
                        apprenticeship.EpaOrgId = submissionEvent.EPAOrgId;
                    }
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Ignoring failed attempt to set EPAOrgId to '{EPAOrgId}' for apprenticeship with id '{ApprenticeshipId}'\r\n", submissionEvent.EPAOrgId, submissionEvent.ApprenticeshipId.Value);
                }
            }
        }

        await dbContext.Value.SaveChangesAsync(cancellationToken);
        
        return command.SubmissionEvents.LastOrDefault()?.Id;
    }
}
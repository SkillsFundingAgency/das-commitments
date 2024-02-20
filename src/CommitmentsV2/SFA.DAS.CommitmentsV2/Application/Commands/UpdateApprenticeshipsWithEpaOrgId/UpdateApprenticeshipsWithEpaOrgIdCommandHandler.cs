using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipsWithEpaOrgId;

public class UpdateApprenticeshipsWithEpaOrgIdCommandHandler : IRequestHandler<UpdateApprenticeshipsWithEpaOrgIdCommand, long?>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    private readonly ILogger<UpdateApprenticeshipsWithEpaOrgIdCommandHandler> _logger;

    public UpdateApprenticeshipsWithEpaOrgIdCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext, ILogger<UpdateApprenticeshipsWithEpaOrgIdCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<long?> Handle(UpdateApprenticeshipsWithEpaOrgIdCommand command, CancellationToken cancellationToken)
    {
        foreach (var submissionEvent in command.SubmissionEvents)
        {
            if (!submissionEvent.ApprenticeshipId.HasValue)
            {
                _logger.LogWarning("Ignoring SubmissionEvent '{SubmissionEventId}' with no ApprenticeshipId", submissionEvent.Id);
            }
            else
            {
                try
                {
                    var apprenticeship = _dbContext.Value.Apprenticeships.FirstOrDefault(x => x.Id == submissionEvent.ApprenticeshipId.Value);
                    if (apprenticeship != null)
                    {
                        apprenticeship.EpaOrgId = submissionEvent.EPAOrgId;
                    }
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Ignoring failed attempt to set EPAOrgId to '{EPAOrgId}' for apprenticeship with id '{ApprenticeshipId}'\r\n", submissionEvent.EPAOrgId, submissionEvent.ApprenticeshipId.Value);
                }
            }
        }

        await _dbContext.Value.SaveChangesAsync(cancellationToken);
        
        return command.SubmissionEvents.LastOrDefault()?.Id;
    }
}
using System;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipsWithEpaOrgId
{
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
                    _logger.LogWarning($"Ignoring SubmissionEvent '{submissionEvent.Id}' with no ApprenticheshipId");
                }
                else
                {
                    try
                    {
                        var apprenticeship = _dbContext.Value.Apprenticeships.FirstOrDefault(x => x.Id == submissionEvent.ApprenticeshipId.Value);
                        apprenticeship.EpaOrgId = submissionEvent.EPAOrgId;
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Ignoring failed attempt to set EPAOrgId to '{submissionEvent.EPAOrgId}' for apprenticeship with id '{submissionEvent.ApprenticeshipId.Value}'\r\n");
                    }
                }
            }
            await _dbContext.Value.SaveChangesAsync();
            return command.SubmissionEvents.LastOrDefault()?.Id;
        }
    }
}

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers
{
    public class ProviderApproveCohortCommandHandler : IHandleMessages<ProviderApproveCohortCommand>
    {
        private readonly ILogger<ProviderApproveCohortCommandHandler> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IApprenticeEmailFeatureService _apprenticeEmailFeatureService;

        public ProviderApproveCohortCommandHandler(
            ILogger<ProviderApproveCohortCommandHandler> logger,
            Lazy<ProviderCommitmentsDbContext> dbContext,
            IApprenticeEmailFeatureService apprenticeEmailFeatureService)
        {
            _logger = logger;
            _dbContext = dbContext;
            _apprenticeEmailFeatureService = apprenticeEmailFeatureService;
        }

        public async Task Handle(ProviderApproveCohortCommand message, IMessageHandlerContext context)
        {
            try
            {
                _logger.LogInformation($"Handling {nameof(ProviderApproveCohortCommand)} with MessageId '{context.MessageId}'");

                var cohort = await _dbContext.Value.GetCohortAggregate(message.CohortId, default);
                var apprenticeEmailIsRequired = _apprenticeEmailFeatureService.IsEnabled &&
                                                _apprenticeEmailFeatureService.ApprenticeEmailIsRequiredFor(cohort.EmployerAccountId, cohort.ProviderId);
                if (cohort.Approvals.HasFlag(Party.Provider))
                {
                    _logger.LogWarning($"Cohort {message.CohortId} has already been approved by the Provider");
                    return;
                }

                cohort.Approve(Party.Provider, message.Message, message.UserInfo, DateTime.UtcNow, apprenticeEmailIsRequired);

                await _dbContext.Value.SaveChangesAsync();
            }
            catch (Exception e)
            {
                _logger.LogError($"Error processing {nameof(ProviderApproveCohortCommand)}", e);
                throw;
            }
        }
    }
}

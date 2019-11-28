using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.DeleteCohort
{
    public class DeleteCohortHandler : AsyncRequestHandler<DeleteCohortCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<DeleteCohortHandler> _logger;
        private readonly IAuthenticationService _authenticationService;

        public DeleteCohortHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<DeleteCohortHandler> logger,
            IAuthenticationService authenticationService)
        {
            _dbContext = dbContext;
            _logger = logger;
            _authenticationService = authenticationService;
        }

        protected override async Task Handle(DeleteCohortCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var cohort = await _dbContext.Value.GetCohortWithDraftApprenticeships(command.CohortId, cancellationToken: cancellationToken);

                cohort.Delete(_authenticationService.GetUserParty(), command.UserInfo);

                _logger.LogInformation($"Cohort marked as deleted. Cohort-Id:{command.CohortId}");
            }
            catch(Exception e)
            {
                _logger.LogError("Error Deleting Cohort", e);
                throw;
            }
        }
    }
}

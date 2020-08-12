using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.NServiceBus.Services;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship
{
    public class StopApprenticeshipCommandHandler : AsyncRequestHandler<StopApprenticeshipCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDate;
        private readonly ILogger<StopApprenticeshipCommandHandler> _logger;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventPublisher _eventPublisher;

        public StopApprenticeshipCommandHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ICurrentDateTime currentDate,
            IEventPublisher eventPublisher,
            IAuthenticationService authenticationService,
            ILogger<StopApprenticeshipCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentDate = currentDate;
            _eventPublisher = eventPublisher;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        protected async override Task Handle(StopApprenticeshipCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Begin stopping apprenticeShip. Apprenticeship-Id:{command.ApprenticeshipId}");

                var apprenticeship = await _dbContext.Value.Apprenticeships
                    .Include(s => s.Cohort)
                    .Include(t => t.DataLockStatus)
                    .SingleOrDefaultAsync(x => x.Id == command.ApprenticeshipId);

                apprenticeship.ValidateApprenticeshipForStop(command.StopDate, command.AccountId, _currentDate);

                var party = _authenticationService.GetUserParty();
                apprenticeship.StopApprenticeship(command.StopDate, command.MadeRedundant, command.UserInfo, party);

                await _eventPublisher.Publish(new ApprenticeshipStoppedEvent
                {
                    AppliedOn = _currentDate.UtcNow,
                    ApprenticeshipId = command.ApprenticeshipId,
                    StopDate = command.StopDate
                });

                _logger.LogInformation($"Stopped apprenticeShip. Apprenticeship-Id:{command.ApprenticeshipId}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Stopping Apprenticeship with id {command.ApprenticeshipId}", e);
                throw;
            }
        }
    }
}

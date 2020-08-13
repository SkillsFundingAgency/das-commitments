using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.NServiceBus.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship
{
    public class StopApprenticeshipCommandHandler : AsyncRequestHandler<StopApprenticeshipCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDate;
        private readonly IAuthenticationService _authenticationService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IMessageHandlerContext _nserviceBusContext;
        private readonly ILogger<StopApprenticeshipCommandHandler> _logger;
        private const string StopNotificationEmailTemplate = "ProviderApprenticeshipStopNotification";

        public StopApprenticeshipCommandHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ICurrentDateTime currentDate,
            IEventPublisher eventPublisher,
            IAuthenticationService authenticationService,
            IMessageHandlerContext nserviceBusContext,

            ILogger<StopApprenticeshipCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentDate = currentDate;
            _eventPublisher = eventPublisher;
            _authenticationService = authenticationService;
            _nserviceBusContext = nserviceBusContext;
            _logger = logger;
        }

        protected async override Task Handle(StopApprenticeshipCommand command, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Begin stopping apprenticeShip. Apprenticeship-Id:{command.ApprenticeshipId}");

                var party = _authenticationService.GetUserParty();
                CheckPartyIsValid(party);

                var apprenticeship = await _dbContext.Value.Apprenticeships
                    .Include(t => t.DataLockStatus)
                    .Include(s => s.Cohort).ThenInclude(c => c.AccountLegalEntity)
                    .SingleOrDefaultAsync(x => x.Id == command.ApprenticeshipId);

                apprenticeship.ValidateApprenticeshipForStop(command.StopDate, command.AccountId, _currentDate);

                apprenticeship.StopApprenticeship(command.StopDate, command.MadeRedundant, command.UserInfo, party);

                _logger.LogInformation($"Stopped apprenticeShip. Apprenticeship-Id:{command.ApprenticeshipId}");

                await _eventPublisher.Publish(new ApprenticeshipStoppedEvent
                {
                    AppliedOn = _currentDate.UtcNow,
                    ApprenticeshipId = command.ApprenticeshipId,
                    StopDate = command.StopDate
                });

                _logger.LogInformation($"Sending email to Provider {apprenticeship.Cohort.ProviderId}, template {StopNotificationEmailTemplate}");

                await NotifyProvider(_nserviceBusContext, apprenticeship.Cohort.ProviderId, apprenticeship.Id, apprenticeship.Cohort.AccountLegalEntity.Name, apprenticeship.ApprenticeName, command.StopDate);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Stopping Apprenticeship with id {command.ApprenticeshipId}", e);
                throw;
            }
        }

        private async Task NotifyProvider(IMessageHandlerContext nserviceBusContext, long providerId, long apprenticeshipId, string employerName, string apprenticeName, DateTime stopDate)
        {
            var sendEmailToProviderCommand = new SendEmailToProviderCommand(providerId, StopNotificationEmailTemplate,
                new Dictionary<string, string>
                {
                        {"EMPLOYER", employerName},
                        {"APPRENTICE", apprenticeName },
                        {"DATE", stopDate.ToString("dd/MM/yyyy") }
                        //{"URL", $"{providerId}/apprentices/manage/{HashingService.HashValue(apprenticeshipId)}/details" }
                });

            await _nserviceBusContext.Send(sendEmailToProviderCommand);
        }

        private void CheckPartyIsValid(Party party)
        {
            if (party != Party.Employer)
            {
                throw new DomainException(nameof(party), $"StopApprenticeship is restricted to Employers only - {party} is invalid");
            }
        }
    }
}

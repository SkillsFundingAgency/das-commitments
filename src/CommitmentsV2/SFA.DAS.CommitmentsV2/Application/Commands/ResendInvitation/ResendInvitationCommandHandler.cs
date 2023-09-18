using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResendInvitation
{
    public class ResendInvitationCommandHandler : IRequestHandler<ResendInvitationCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDate;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessageSession _messageSession;
        private readonly ILogger<ResendInvitationCommandHandler> _logger;

        public ResendInvitationCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ICurrentDateTime currentDate,
            IAuthenticationService authenticationService,
            IMessageSession messageSession,
            ILogger<ResendInvitationCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentDate = currentDate;
            _authenticationService = authenticationService;
            _messageSession = messageSession;
            _logger = logger;
        }

        public async Task Handle(ResendInvitationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var party = _authenticationService.GetUserParty();
                var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);

                if (apprenticeship.Email == null)
                {
                    throw new DomainException("Email", "Invitation cannot be sent as there is no email associated with apprenticeship");
                }

                if (apprenticeship.EmailAddressConfirmed == true)
                {
                    throw new DomainException("Email", "Email address has been confirmed");
                }

                await _messageSession.Send(new ApprenticeshipResendInvitationCommand
                {
                    ApprenticeshipId = apprenticeship.Id,
                    ResendOn = _currentDate.UtcNow
                });
                _logger.LogInformation($"Resending Invitation for Apprenticeship id {command.ApprenticeshipId}, initiated by {party} : userId {command.UserInfo.UserId}");
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Resending Invitation for Apprenticeship id {command.ApprenticeshipId}", e);
                throw;
            }
        }
    }
}

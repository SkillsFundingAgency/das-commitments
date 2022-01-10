using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.ResumeApprenticeship;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResendInvitation
{
    public class ResendInvitationCommandHandler : AsyncRequestHandler<ResendInvitationCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDate;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<ResendInvitationCommandHandler> _logger;

        public ResendInvitationCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ICurrentDateTime currentDate,
            IAuthenticationService authenticationService,
            ILogger<ResendInvitationCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentDate = currentDate;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        protected override async Task Handle(ResendInvitationCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var party = _authenticationService.GetUserParty();
                var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
                apprenticeship.ResendInvitation(_currentDate);
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

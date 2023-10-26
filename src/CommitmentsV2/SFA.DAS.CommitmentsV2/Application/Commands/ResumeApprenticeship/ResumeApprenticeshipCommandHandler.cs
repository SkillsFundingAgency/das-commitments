using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ResumeApprenticeship
{
    public class ResumeApprenticeshipCommandHandler : IRequestHandler<ResumeApprenticeshipCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDate;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<ResumeApprenticeshipCommandHandler> _logger;

        public ResumeApprenticeshipCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ICurrentDateTime currentDate,
            IAuthenticationService authenticationService,
            ILogger<ResumeApprenticeshipCommandHandler> logger)
        {
            _dbContext = dbContext;
            _currentDate = currentDate;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        public async Task Handle(ResumeApprenticeshipCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var party = _authenticationService.GetUserParty();
                CheckPartyIsValid(party);

                var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
                apprenticeship.ResumeApprenticeship(_currentDate, party, command.UserInfo);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Resuming Apprenticeship with id {command.ApprenticeshipId}", e);
                throw;
            }
        }

        private static void CheckPartyIsValid(Party party)
        {
            if (party != Party.Employer)
            {
                throw new DomainException(nameof(party), $"Only employers are allowed to edit the end of completed records - {party} is invalid");
            }
        }
    }
}

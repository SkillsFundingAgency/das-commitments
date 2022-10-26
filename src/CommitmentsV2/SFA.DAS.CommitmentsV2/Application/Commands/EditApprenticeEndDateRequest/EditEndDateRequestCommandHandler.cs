using MediatR;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Application.Commands.EditApprenticeEndDateRequest
{
    public class EditEndDateRequestCommandHandler : AsyncRequestHandler<EditEndDateRequestCommand>
    {

        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDate;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<EditEndDateRequestCommandHandler> _logger;
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;
        public EditEndDateRequestCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ICurrentDateTime currentDate,
            IAuthenticationService authenticationService,
            ILogger<EditEndDateRequestCommandHandler> logger,
            IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {
            _dbContext = dbContext;
            _currentDate = currentDate;
            _authenticationService = authenticationService;
            _logger = logger;
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        protected override async Task Handle(EditEndDateRequestCommand command, CancellationToken cancellationToken)
        {
            var party = _authenticationService.GetUserParty();
            CheckPartyIsValid(party);

            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);
            apprenticeship.EditEndDateOfCompletedRecord(command.EndDate.Value, _currentDate, party, command.UserInfo);

            await _resolveOverlappingTrainingDateRequestService.Resolve(command.ApprenticeshipId, null, Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipEndDateUpdate);
        }

        private void CheckPartyIsValid(Party party)
        {
            if (party != Party.Employer)
            {
                throw new DomainException(nameof(party), $"Only employers are allowed to edit the end of completed records - {party} is invalid");
            }
        }
    }
}

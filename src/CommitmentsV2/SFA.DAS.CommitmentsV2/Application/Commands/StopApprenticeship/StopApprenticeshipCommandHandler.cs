using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.NServiceBus.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.StopApprenticeship
{
    public class StopApprenticeshipCommandHandler : IRequestHandler<StopApprenticeshipCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ICurrentDateTime _currentDate;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessageSession _nserviceBusContext;
        private readonly IEncodingService _encodingService;
        private readonly ILogger<StopApprenticeshipCommandHandler> _logger;
        private readonly CommitmentsV2Configuration _commitmentsV2Configuration;
        private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;
        private const string StopNotificationEmailTemplate = "ProviderApprenticeshipStopNotification";

        public StopApprenticeshipCommandHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ICurrentDateTime currentDate,
            IAuthenticationService authenticationService,
            IMessageSession nserviceBusContext,
            IEncodingService encodingService,
            ILogger<StopApprenticeshipCommandHandler> logger,
            CommitmentsV2Configuration commitmentsV2Configuration,
            IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
        {
            _dbContext = dbContext;
            _currentDate = currentDate;
            _authenticationService = authenticationService;
            _nserviceBusContext = nserviceBusContext;
            _encodingService = encodingService;
            _logger = logger;
            _commitmentsV2Configuration = commitmentsV2Configuration;
            _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
        }

        public async Task Handle(StopApprenticeshipCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation($"Begin stopping apprenticeShip. Apprenticeship-Id:{request.ApprenticeshipId}");

                var party = _authenticationService.GetUserParty();
                CheckPartyIsValid(party);

                var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(request.ApprenticeshipId, cancellationToken);

                apprenticeship.StopApprenticeship(request.StopDate, request.AccountId, request.MadeRedundant, request.UserInfo, _currentDate, party);
                _dbContext.Value.SaveChanges();

                _logger.LogInformation($"Stopped apprenticeship. Apprenticeship-Id:{request.ApprenticeshipId}");

                await _resolveOverlappingTrainingDateRequestService.Resolve(request.ApprenticeshipId, null, Types.OverlappingTrainingDateRequestResolutionType.ApprenticeshipStopped);

                _logger.LogInformation($"Sending email to Provider {apprenticeship.Cohort.ProviderId}, template {StopNotificationEmailTemplate}");
                await NotifyProvider(apprenticeship.Cohort.ProviderId, apprenticeship.Id, apprenticeship.Cohort.AccountLegalEntity.Name, apprenticeship.ApprenticeName, request.StopDate);
            }
            catch (Exception e)
            {
                _logger.LogError($"Error Stopping Apprenticeship with id {request.ApprenticeshipId}", e);
                throw;
            }
        }

        private async Task NotifyProvider(long providerId, long apprenticeshipId, string employerName, string apprenticeName, DateTime stopDate)
        {
            var sendEmailToProviderCommand = new SendEmailToProviderCommand(providerId, StopNotificationEmailTemplate,
                new Dictionary<string, string>
                {
                        {"EMPLOYER", employerName},
                        {"APPRENTICE", apprenticeName },
                        {"DATE", stopDate.ToString("dd/MM/yyyy") },
                        {"URL", $"{_commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{providerId}/apprentices/{_encodingService.Encode(apprenticeshipId, EncodingType.ApprenticeshipId)}"}
                });

            await _nserviceBusContext.Send(sendEmailToProviderCommand);
        }

        private static void CheckPartyIsValid(Party party)
        {
            if (party != Party.Employer)
            {
                throw new DomainException(nameof(party), $"StopApprenticeship is restricted to Employers only - {party} is invalid");
            }
        }
    }
}
using MediatR;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipStopDate
{
    public class UpdateApprenticeshipStopDateCommandHandler : AsyncRequestHandler<UpdateApprenticeshipStopDateCommand>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<UpdateApprenticeshipStopDateCommandHandler> _logger;        
        private readonly ICurrentDateTime _currentDate;
        private readonly IAuthenticationService _authenticationService;
        private readonly IMessageSession _nserviceBusContext;
        private readonly IEncodingService _encodingService;
        private readonly IOverlapCheckService _overlapCheckService;
        private readonly CommitmentsV2Configuration _commitmentsV2Configuration;
        private const string StopEditNotificationEmailTemplate = "ProviderApprenticeshipStopEditNotification";

        public UpdateApprenticeshipStopDateCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<UpdateApprenticeshipStopDateCommandHandler> logger,            
            ICurrentDateTime currentDate,
            IAuthenticationService authenticationService,
            IMessageSession nserviceBusContext,
            IEncodingService encodingService,
            IOverlapCheckService overlapCheckService,
            CommitmentsV2Configuration commitmentsV2Configuration)
        {
            _dbContext = dbContext;
            _logger = logger;            
            _currentDate = currentDate;
            _authenticationService = authenticationService;
            _nserviceBusContext = nserviceBusContext;
            _encodingService = encodingService;
            _overlapCheckService = overlapCheckService;
            _commitmentsV2Configuration = commitmentsV2Configuration;
        }

        
        protected override async Task Handle(UpdateApprenticeshipStopDateCommand command, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Employer: {command.AccountId} has called UpdateApprenticeshipStopDateCommand ApprenticeshipId : {command.ApprenticeshipId} ");

            var party = _authenticationService.GetUserParty();
            CheckPartyIsValid(party);

            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);

            CheckAuthorization(command, apprenticeship);

            ValidateChangeDateForStop(command.StopDate, apprenticeship);

            ValidateEndDateOverlap(command, apprenticeship, cancellationToken); 

            apprenticeship.ApprenticeshipStopDate(command, _currentDate, party);

            _logger.LogInformation($"Update apprenticeship stop date. Apprenticeship-Id:{command.ApprenticeshipId}");

            _logger.LogInformation($"Sending email to Provider {apprenticeship.Cohort.ProviderId}, template {StopEditNotificationEmailTemplate}");

            await NotifyProvider(apprenticeship, command.StopDate);
        }    

        private static void CheckAuthorization(UpdateApprenticeshipStopDateCommand message, Apprenticeship apprenticeship)
        {
            if (apprenticeship.Cohort.EmployerAccountId != message.AccountId)
                throw new DomainException(nameof(apprenticeship),  $"Employer {message.AccountId} not authorised to access commitment {apprenticeship.Cohort.Id}, expected employer {apprenticeship.Cohort.EmployerAccountId}");
        }

        private void ValidateChangeDateForStop(DateTime newStopDate, Apprenticeship apprenticeship)
        {
            if (apprenticeship == null) throw new ArgumentException(nameof(apprenticeship));

            if (apprenticeship.PaymentStatus != PaymentStatus.Withdrawn)
            {
                throw new DomainException(nameof(newStopDate), "Apprenticeship must be stopped in order to update stop date");
            }

            if (newStopDate.Date > _currentDate.UtcNow.Date)
                throw new DomainException(nameof(newStopDate), "Invalid Date of Change. Date cannot be in the future.");

            if (newStopDate.Date == apprenticeship.StopDate.Value.Date)
                throw new DomainException(nameof(newStopDate), "Enter a date that is different to the current stopped date");

            if (newStopDate.Date < apprenticeship.StartDate.Value.Date)
                throw new DomainException(nameof(newStopDate), "The stop month cannot be before the apprenticeship started");
        }

        public void ValidateEndDateOverlap(UpdateApprenticeshipStopDateCommand command, Apprenticeship apprenticeship, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(apprenticeship.Uln) || !apprenticeship.StartDate.HasValue) return;

            var overlapResult =  _overlapCheckService.CheckForOverlaps(apprenticeship.Uln, apprenticeship.StartDate.Value.To(command.StopDate), apprenticeship.Id, cancellationToken);

            if (!overlapResult.Result.HasOverlaps) return;

            var errorMessage = "The date overlaps with existing dates for the same apprentice";

            var errors = new List<DomainError>();        

            if (overlapResult.Result.HasOverlappingEndDate)
            {
                errors.Add(new DomainError("newStopDate", errorMessage));
            }

            throw new DomainException(errors);
        }

        private void CheckPartyIsValid(Party party)
        {
            if (party != Party.Employer)
            {
                throw new DomainException(nameof(party), $"UpdateApprenticeshipStopDate is restricted to Employers only - {party} is invalid");
            }
        }      

        private async Task NotifyProvider(Apprenticeship apprenticeship, DateTime newStopDate)
        {
            var sendEmailToProviderCommand = new SendEmailToProviderCommand(apprenticeship.Cohort.ProviderId, StopEditNotificationEmailTemplate,
                new Dictionary<string, string>
                {
                       {"EMPLOYER", apprenticeship.Cohort.AccountLegalEntity.Name},
                       {"APPRENTICE", apprenticeship.ApprenticeName },
                       {"OLDDATE", apprenticeship.StopDate.Value.ToString("dd/MM/yyyy") },
                       {"NEWDATE", newStopDate.ToString("dd/MM/yyyy") },
                       {"URL", $"{_commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{apprenticeship.Cohort.ProviderId}/apprentices/{_encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}"}
                });

            await _nserviceBusContext.Send(sendEmailToProviderCommand);
        }
    }
}

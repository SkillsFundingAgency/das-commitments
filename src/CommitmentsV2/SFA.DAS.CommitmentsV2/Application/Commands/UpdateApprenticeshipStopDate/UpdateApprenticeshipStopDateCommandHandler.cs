﻿using Microsoft.Extensions.Logging;
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

namespace SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipStopDate;

public class UpdateApprenticeshipStopDateCommandHandler : IRequestHandler<UpdateApprenticeshipStopDateCommand>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    private readonly ILogger<UpdateApprenticeshipStopDateCommandHandler> _logger;
    private readonly ICurrentDateTime _currentDate;
    private readonly IAuthenticationService _authenticationService;
    private readonly IMessageSession _nserviceBusContext;
    private readonly IEncodingService _encodingService;
    private readonly IOverlapCheckService _overlapCheckService;
    private readonly CommitmentsV2Configuration _commitmentsV2Configuration;
    private readonly IResolveOverlappingTrainingDateRequestService _resolveOverlappingTrainingDateRequestService;
    private const string StopEditNotificationEmailTemplate = "ProviderApprenticeshipStopEditNotification";

    public UpdateApprenticeshipStopDateCommandHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
        ILogger<UpdateApprenticeshipStopDateCommandHandler> logger,
        ICurrentDateTime currentDate,
        IAuthenticationService authenticationService,
        IMessageSession nserviceBusContext,
        IEncodingService encodingService,
        IOverlapCheckService overlapCheckService,
        CommitmentsV2Configuration commitmentsV2Configuration,
        IResolveOverlappingTrainingDateRequestService resolveOverlappingTrainingDateRequestService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _currentDate = currentDate;
        _authenticationService = authenticationService;
        _nserviceBusContext = nserviceBusContext;
        _encodingService = encodingService;
        _overlapCheckService = overlapCheckService;
        _commitmentsV2Configuration = commitmentsV2Configuration;
        _resolveOverlappingTrainingDateRequestService = resolveOverlappingTrainingDateRequestService;
    }

    public async Task Handle(UpdateApprenticeshipStopDateCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Employer: {AccountId} has called UpdateApprenticeshipStopDateCommand ApprenticeshipId : {ApprenticeshipId} ", command.AccountId, command.ApprenticeshipId);

        var party = _authenticationService.GetUserParty();
        CheckPartyIsValid(party);

        var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(command.ApprenticeshipId, cancellationToken);

        CheckAuthorization(command, apprenticeship);

        ValidateChangeDateForStop(command.StopDate, apprenticeship);

        ValidateEndDateOverlap(command, apprenticeship, cancellationToken);

        var oldStopDate = apprenticeship.StopDate;
        apprenticeship.ApprenticeshipStopDate(command, _currentDate, party);
        await _dbContext.Value.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Update apprenticeship stop date. Apprenticeship-Id:{ApprenticeshipId}", command.ApprenticeshipId);

        await _resolveOverlappingTrainingDateRequestService.Resolve(command.ApprenticeshipId, null, OverlappingTrainingDateRequestResolutionType.StopDateUpdate);

        _logger.LogInformation("Sending email to Provider {CohortProviderId}, template {StopEditNotificationEmailTemplate}", apprenticeship.Cohort.ProviderId, StopEditNotificationEmailTemplate);
        await NotifyProvider(apprenticeship, oldStopDate, command.StopDate);
    }

    private static void CheckAuthorization(UpdateApprenticeshipStopDateCommand message, Apprenticeship apprenticeship)
    {
        if (apprenticeship.Cohort.EmployerAccountId != message.AccountId)
        {
            throw new DomainException(nameof(apprenticeship), $"Employer {message.AccountId} not authorised to access commitment {apprenticeship.Cohort.Id}, expected employer {apprenticeship.Cohort.EmployerAccountId}");
        }
    }

    private void ValidateChangeDateForStop(DateTime newStopDate, Apprenticeship apprenticeship)
    {
        if (apprenticeship == null)
        {
            throw new ArgumentException(null, nameof(apprenticeship));
        }

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

    private void ValidateEndDateOverlap(UpdateApprenticeshipStopDateCommand command, Apprenticeship apprenticeship, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(apprenticeship.Uln) || !apprenticeship.StartDate.HasValue) return;

        var overlapResult = _overlapCheckService.CheckForOverlaps(apprenticeship.Uln, apprenticeship.StartDate.Value.To(command.StopDate), apprenticeship.Id, cancellationToken);

        if (!overlapResult.Result.HasOverlaps) return;

        const string errorMessage = "The date overlaps with existing dates for the same apprentice";

        var errors = new List<DomainError> { new("newStopDate", errorMessage) };

        throw new DomainException(errors);
    }

    private static void CheckPartyIsValid(Party party)
    {
        if (party != Party.Employer)
        {
            throw new DomainException(nameof(party), $"UpdateApprenticeshipStopDate is restricted to Employers only - {party} is invalid");
        }
    }

    private async Task NotifyProvider(Apprenticeship apprenticeship, DateTime? oldStopDate, DateTime newStopDate)
    {
        var sendEmailToProviderCommand = new SendEmailToProviderCommand(apprenticeship.Cohort.ProviderId, StopEditNotificationEmailTemplate,
            new Dictionary<string, string>
            {
                   {"EMPLOYER", apprenticeship.Cohort.AccountLegalEntity.Name},
                   {"APPRENTICE", apprenticeship.ApprenticeName },
                   {"OLDDATE", oldStopDate.Value.ToString("dd/MM/yyyy") },
                   {"NEWDATE", newStopDate.ToString("dd/MM/yyyy") },
                   {"URL", $"{_commitmentsV2Configuration.ProviderCommitmentsBaseUrl}/{apprenticeship.Cohort.ProviderId}/apprentices/{_encodingService.Encode(apprenticeship.Id, EncodingType.ApprenticeshipId)}"}
            });

        await _nserviceBusContext.Send(sendEmailToProviderCommand);
    }
}
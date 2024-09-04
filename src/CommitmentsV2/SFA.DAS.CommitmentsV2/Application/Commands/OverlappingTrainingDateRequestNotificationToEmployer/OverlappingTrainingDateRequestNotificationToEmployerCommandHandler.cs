using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.Encoding;

namespace SFA.DAS.CommitmentsV2.Application.Commands.OverlappingTrainingDateRequestNotificationToEmployer;

public class OverlappingTrainingDateRequestNotificationToEmployerCommandHandler : IRequestHandler<OverlappingTrainingDateRequestNotificationToEmployerCommand>
{
    public const string TemplateId = "ChaseEmployerForOverlappingTrainingDateRequest";
    private readonly ICurrentDateTime _currentDateTime;
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    private readonly IMessageSession _messageSession;
    private readonly CommitmentsV2Configuration _configuration;
    private readonly ILogger<OverlappingTrainingDateRequestNotificationToEmployerCommandHandler> _logger;
    private readonly IEncodingService _encodingService;

    public OverlappingTrainingDateRequestNotificationToEmployerCommandHandler(Lazy<ProviderCommitmentsDbContext> commitmentsDbContext,
        ICurrentDateTime currentDateTime,
        IMessageSession messageSession,
        CommitmentsV2Configuration configuration,
        IEncodingService encodingService,
        ILogger<OverlappingTrainingDateRequestNotificationToEmployerCommandHandler> logger)
    {
        _dbContext = commitmentsDbContext;
        _currentDateTime = currentDateTime;
        _messageSession = messageSession;
        _configuration = configuration;
        _logger = logger;
        _encodingService = encodingService;
    }
    public async Task Handle(OverlappingTrainingDateRequestNotificationToEmployerCommand request, CancellationToken cancellationToken)
    {
        var currentDate = _currentDateTime.UtcNow;

        var pendingRecords = _dbContext.Value.OverlappingTrainingDateRequests
            .Include(oltd => oltd.DraftApprenticeship)
            .ThenInclude(draftApprenticeship => draftApprenticeship.Cohort)
            .Include(oltd => oltd.PreviousApprenticeship)
            .ThenInclude(previousApprenticeship => previousApprenticeship.Cohort)
             .Where(x => x.NotifiedServiceDeskOn == null
                            && x.NotifiedEmployerOn == null
                            && x.Status == Types.OverlappingTrainingDateRequestStatus.Pending
                            &&  x.CreatedOn < currentDate.AddDays(-7).Date
                            && x.PreviousApprenticeship.Cohort != null)                            
                .ToList();

        _logger.LogInformation("Found {count} records which chaser email to employer", pendingRecords.Count);

        foreach (var pendingRecord in pendingRecords)
        {
            _logger.LogInformation("Sending chaser email to employer - with cohort ref:{PreviousApprenticeshipCohortRef} for apprentice with ULN:{PreviousApprenticeshipUln}", pendingRecord.PreviousApprenticeship.Cohort.Reference, pendingRecord.PreviousApprenticeship.Uln);

            if (pendingRecord.DraftApprenticeship == null)
            {
                continue;
            }
                
            var tokens = new Dictionary<string, string>
            {
                { "Cohort", pendingRecord.PreviousApprenticeship.Cohort.Reference},
                { "RequestRaisedDate", pendingRecord.CreatedOn.ToString("dd-MM-yyyy") },
                { "Apprentice", pendingRecord.PreviousApprenticeship.FirstName + " " + pendingRecord.PreviousApprenticeship.LastName },
                { "ULN", pendingRecord.PreviousApprenticeship.Uln },
                { "URL", $"{_configuration.EmployerCommitmentsBaseUrl}/{_encodingService.Encode(pendingRecord.PreviousApprenticeship.Cohort.EmployerAccountId,EncodingType.AccountId)}/apprentices/{_encodingService.Encode(pendingRecord.PreviousApprenticeshipId, EncodingType.ApprenticeshipId)}/details"}
            };

            var emailCommand = new SendEmailToEmployerCommand(pendingRecord.PreviousApprenticeship.Cohort.EmployerAccountId,  TemplateId, tokens, null, "NAME");
                
            await _messageSession.Send(emailCommand);

            pendingRecord.NotifiedEmployerOn = _currentDateTime.UtcNow;
        }

        await _dbContext.Value.SaveChangesAsync(cancellationToken);
    }    
}
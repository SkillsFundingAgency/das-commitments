using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Mapping;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddCohort;

public class AddCohortHandler : IRequestHandler<AddCohortCommand, AddCohortResult>
{
    private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
    private readonly ILogger<AddCohortHandler> _logger;
    private readonly IEncodingService _encodingService;

    private readonly IOldMapper<AddCohortCommand, DraftApprenticeshipDetails> _draftApprenticeshipDetailsMapper;
    private readonly ICohortDomainService _cohortDomainService;
    private readonly IReservationsApiClient _reservationsClient;

    public AddCohortHandler(
        Lazy<ProviderCommitmentsDbContext> dbContext,
        IEncodingService encodingService,
        ILogger<AddCohortHandler> logger,
        IOldMapper<AddCohortCommand, DraftApprenticeshipDetails> draftApprenticeshipDetailsMapper,
        ICohortDomainService cohortDomainService,
        IReservationsApiClient reservationsClient)
    {
        _dbContext = dbContext;
        _logger = logger;
        _draftApprenticeshipDetailsMapper = draftApprenticeshipDetailsMapper;
        _cohortDomainService = cohortDomainService;
        _reservationsClient = reservationsClient;
        _encodingService = encodingService;
    }

    public async Task<AddCohortResult> Handle(AddCohortCommand command, CancellationToken cancellationToken)
    {
        var db = _dbContext.Value;

        var draftApprenticeshipDetails = await _draftApprenticeshipDetailsMapper.Map(command);

        var cohort = await _cohortDomainService.CreateCohort(command.ProviderId, 
            command.AccountId,
            command.AccountLegalEntityId,
            command.TransferSenderId,
            command.PledgeApplicationId,
            draftApprenticeshipDetails,
            command.UserInfo,
            command.RequestingParty,
            cancellationToken);

        if (command.ReservationId == null)
        {
            await CreateAutoReservationAndAdd(command, cohort, cancellationToken);
        }

        db.Cohorts.Add(cohort);
        await db.SaveChangesAsync(cancellationToken);

        //this encoding and re-save could be removed and put elsewhere
        cohort.Reference = _encodingService.Encode(cohort.Id, EncodingType.CohortReference);
        await db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Saved cohort. Provider: {ProviderId} Account-Legal-Entity:{AccountLegalEntityId} Reservation-Id:{ReservationId} Commitment-Id:{Id} Apprenticeship:{ApprenticeshipId}",
            command.ProviderId, command.AccountLegalEntityId, command.ReservationId, cohort.Id, cohort.Apprenticeships?.FirstOrDefault()?.Id);

        var response = new AddCohortResult
        {
            Id = cohort.Id,
            Reference = cohort.Reference
        };

        return response;
    }

    private async Task CreateAutoReservationAndAdd(AddCohortCommand command, Cohort cohort, CancellationToken cancellationToken)
    {
        Guid.TryParse(command.UserInfo.UserId, out Guid userId);

        var accountLegalEntity = await _dbContext.Value.AccountLegalEntities.Where(x => x.Id == command.AccountLegalEntityId).FirstOrDefaultAsync();
        var request = new CreateAutoReservationRequest
        {
            AccountId = command.AccountId,
            AccountLegalEntityId = command.AccountLegalEntityId,
            CourseId = command.CourseCode,
            AccountLegalEntityName = accountLegalEntity?.Name,
            ProviderId = Convert.ToUInt32(command.ProviderId),
            StartDate = command.StartDate.Value,
            UserId = userId
        };

        var response = await _reservationsClient.CreateAutoReservation(request, cancellationToken);

        cohort.DraftApprenticeships.FirstOrDefault().ReservationId = response.Id;
    }
}
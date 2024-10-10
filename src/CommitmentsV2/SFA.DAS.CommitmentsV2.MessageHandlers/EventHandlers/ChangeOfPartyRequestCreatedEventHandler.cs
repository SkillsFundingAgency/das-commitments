using System.Threading;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers;

public class ChangeOfPartyRequestCreatedEventHandler(
    Lazy<ProviderCommitmentsDbContext> dbContext,
    IReservationsApiClient reservationsApiClient,
    ILogger<ChangeOfPartyRequestCreatedEventHandler> logger,
    IEncodingService encodingService,
    IOverlappingTrainingDateRequestDomainService overlappingTrainingDateRequestDomainService)
    : IHandleMessages<ChangeOfPartyRequestCreatedEvent>
{
    public async Task Handle(ChangeOfPartyRequestCreatedEvent message, IMessageHandlerContext context)
    {
        logger.LogInformation("ChangeOfPartyRequestCreatedEventHandler received ChangeOfPartyRequestId {Id}", message.ChangeOfPartyRequestId);

        var changeOfPartyRequest = await dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);
        var apprenticeship = await dbContext.Value.GetApprenticeshipAggregate(changeOfPartyRequest.ApprenticeshipId, default);

        var reservationId = await GetReservationId(changeOfPartyRequest, apprenticeship);

        var cohort = changeOfPartyRequest.CreateCohort(apprenticeship, reservationId, message.UserInfo, message.HasOverlappingTrainingDates);

        logger.LogInformation("ChangeOfPartyRequestCreatedEventHandler adding Cohort");
        dbContext.Value.Cohorts.Add(cohort);
        await dbContext.Value.SaveChangesAsync();

        //this encoding and re-save could be removed and put elsewhere
        cohort.Reference = encodingService.Encode(cohort.Id, EncodingType.CohortReference);
        await dbContext.Value.SaveChangesAsync();

        if (message.HasOverlappingTrainingDates)
        {
            logger.LogInformation("ChangeOfPartyRequestCreatedEventHandler {ChangeOfPartyRequestId} HasOverlappingTrainingDates. Creating new CreateOverlappingTrainingDatesRequest", message.ChangeOfPartyRequestId);

            await overlappingTrainingDateRequestDomainService.CreateOverlappingTrainingDateRequest(
                cohort.Apprenticeships.First().Id,
                changeOfPartyRequest.OriginatingParty,
                apprenticeship.Id,
                message.UserInfo,
                new CancellationToken()
            );
        }
    }

    private async Task<Guid?> GetReservationId(ChangeOfPartyRequest changeOfPartyRequest, Apprenticeship apprenticeship)
    {
        if (!apprenticeship.ReservationId.HasValue)
        {
            return null;
        }

        var createChangeOfPartyReservationRequest = new CreateChangeOfPartyReservationRequest
        {
            AccountLegalEntityId = changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                ? changeOfPartyRequest.AccountLegalEntityId
                : null,
            ProviderId = changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider
                ? changeOfPartyRequest.ProviderId
                : null
        };

        var result = await reservationsApiClient.CreateChangeOfPartyReservation(apprenticeship.ReservationId.Value, createChangeOfPartyReservationRequest, default);
        
        return result.ReservationId;
    }
}
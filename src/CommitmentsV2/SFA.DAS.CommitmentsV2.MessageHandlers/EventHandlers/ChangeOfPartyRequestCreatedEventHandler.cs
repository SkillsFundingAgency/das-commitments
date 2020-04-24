using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Encoding;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.EventHandlers
{
    public class ChangeOfPartyRequestCreatedEventHandler : IHandleMessages<ChangeOfPartyRequestCreatedEvent>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly IReservationsApiClient _reservationsApiClient;
        private readonly ILogger<ChangeOfPartyRequestCreatedEventHandler> _logger;
        private readonly IEncodingService _encodingService;

        public ChangeOfPartyRequestCreatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            IReservationsApiClient reservationsApiClient,
            ILogger<ChangeOfPartyRequestCreatedEventHandler> logger,
            IEncodingService encodingService)
        {
            _dbContext = dbContext;
            _reservationsApiClient = reservationsApiClient;
            _logger = logger;
            _encodingService = encodingService;
        }

        public async Task Handle(ChangeOfPartyRequestCreatedEvent message, IMessageHandlerContext context)
        {
            //get the copr
            var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);

            //get the details of the original apprenticeship to which it pertains
            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(changeOfPartyRequest.ApprenticeshipId, default);

            //obtain a reservation
            var createChangeOfPartyReservationRequest = new CreateChangeOfPartyReservationRequest
            {
                AccountLegalEntityId = changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeEmployer
                    ? changeOfPartyRequest.AccountLegalEntityId
                    : null,
                ProviderId = changeOfPartyRequest.ChangeOfPartyType == ChangeOfPartyRequestType.ChangeProvider
                    ? changeOfPartyRequest.ProviderId
                    : null
            };

            var reservationResult =
                await _reservationsApiClient.CreateChangeOfPartyReservation(apprenticeship.ReservationId.Value, createChangeOfPartyReservationRequest, default);

            //tell the copr to create a cohort
            var cohort = changeOfPartyRequest.CreateCohort(apprenticeship, reservationResult.ReservationId);

            //persist
            _dbContext.Value.Cohorts.Add(cohort);
            await _dbContext.Value.SaveChangesAsync();

            //this encoding and re-save could be removed and put elsewhere
            cohort.Reference = _encodingService.Encode(cohort.Id, EncodingType.CohortReference);
            await _dbContext.Value.SaveChangesAsync();
        }
    }
}

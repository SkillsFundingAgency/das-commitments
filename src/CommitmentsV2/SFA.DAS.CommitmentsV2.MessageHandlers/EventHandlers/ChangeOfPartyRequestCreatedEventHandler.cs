using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NServiceBus;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Data.Extensions;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
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
        private readonly IOverlappingTrainingDateRequestDomainService _overlappingTrainingDateRequestDomainService;

        public ChangeOfPartyRequestCreatedEventHandler(Lazy<ProviderCommitmentsDbContext> dbContext,
            IReservationsApiClient reservationsApiClient,
            ILogger<ChangeOfPartyRequestCreatedEventHandler> logger,
            IEncodingService encodingService,
            IOverlappingTrainingDateRequestDomainService overlappingTrainingDateRequestDomainService)
        {
            _dbContext = dbContext;
            _reservationsApiClient = reservationsApiClient;
            _logger = logger;
            _encodingService = encodingService;
            _overlappingTrainingDateRequestDomainService = overlappingTrainingDateRequestDomainService;
        }

        public async Task Handle(ChangeOfPartyRequestCreatedEvent message, IMessageHandlerContext context)
        {
            var changeOfPartyRequest = await _dbContext.Value.GetChangeOfPartyRequestAggregate(message.ChangeOfPartyRequestId, default);
            var apprenticeship = await _dbContext.Value.GetApprenticeshipAggregate(changeOfPartyRequest.ApprenticeshipId, default);

            var reservationId = await GetReservationId(changeOfPartyRequest, apprenticeship);
            
            var cohort = changeOfPartyRequest.CreateCohort(apprenticeship, reservationId, message.UserInfo, message.HasOltd);
            
            _dbContext.Value.Cohorts.Add(cohort);
            await _dbContext.Value.SaveChangesAsync();

            //this encoding and re-save could be removed and put elsewhere
            cohort.Reference = _encodingService.Encode(cohort.Id, EncodingType.CohortReference);
            await _dbContext.Value.SaveChangesAsync();

            if (message.HasOltd)
            {
                _logger.LogInformation($"ChangeOfPartyRequestCreatedEventHandler {message.ChangeOfPartyRequestId} hasOLTD. Creating new CreateOverlappingTrainingDatesRequest");

                await _overlappingTrainingDateRequestDomainService
                .CreateOverlappingTrainingDateRequest(cohort.Apprenticeships.First().Id, changeOfPartyRequest.OriginatingParty, apprenticeship.Id, message.UserInfo, new CancellationToken());
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

            var result = await _reservationsApiClient.CreateChangeOfPartyReservation(apprenticeship.ReservationId.Value, createChangeOfPartyReservationRequest, default);
            return result.ReservationId;
        }
    }
}

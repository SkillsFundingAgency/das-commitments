using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Commitments.Application.Services
{
    public class V2EventsPublisher : IV2EventsPublisher
    {
        private readonly IEndpointInstance _endpointInstance;
        private readonly ICommitmentsLogger _logger;
        private readonly ICurrentDateTime _currentDateTime;


        public V2EventsPublisher(IEndpointInstance endpointInstance, ICommitmentsLogger logger, ICurrentDateTime currentDateTime)
        {
            _endpointInstance = endpointInstance;
            _logger = logger;
            _currentDateTime = currentDateTime;
        }

        public async Task PublishApprenticeshipDeleted(Commitment commitment, Apprenticeship apprenticeship)
        {
            var logMessage = $"Publish IDraftApprenticeshipDeletedEvent message. Provider:{apprenticeship.ProviderId} Commitment:{commitment.Id} Apprenticeship:{apprenticeship.Id} ReservationId:{apprenticeship.ReservationId}";

            try
            {
                await _endpointInstance.Publish<IDraftApprenticeshipDeletedEvent>(ev =>
                {
                    ev.DraftApprenticeshipId = apprenticeship.Id;
                    ev.CohortId = commitment.Id;
                    ev.DeletedOn = DateTime.UtcNow;
                    ev.ReservationId = apprenticeship.ReservationId;
                    ev.Uln = apprenticeship.ULN;
                });

                _logger.Info($"{logMessage} successful");
            }
            catch(Exception e)
            {
                _logger.Error(e, $"{logMessage} failed");
            }
        }

        public async Task PublishApprenticeshipCreated(IApprenticeshipEvent apprenticeshipEvent)
        {
            var logMessage = $"Publish ApprenticeshipCreatedEvent message. Provider:{apprenticeshipEvent.Commitment.ProviderId} Commitment:{apprenticeshipEvent.Commitment.Id} Apprenticeship:{apprenticeshipEvent.Apprenticeship.Id}";

            try
            {
                var priceEpisodes = apprenticeshipEvent.Apprenticeship.PriceHistory
                    .Select(x => new PriceEpisode { FromDate = x.FromDate, ToDate = x.ToDate, Cost = x.Cost}).ToArray();

                await _endpointInstance.Publish<IApprenticeshipCreatedEvent>(ev =>
                {
                    ev.ApprenticeshipId = apprenticeshipEvent.Apprenticeship.Id;
                    ev.CreatedOn = _currentDateTime.Now;
                    ev.Uln = apprenticeshipEvent.Apprenticeship.ULN;
                    ev.ProviderId = apprenticeshipEvent.Apprenticeship.ProviderId;
                    ev.AccountId = apprenticeshipEvent.Apprenticeship.EmployerAccountId;
                    ev.AccountLegalEntityPublicHashedId =
                        apprenticeshipEvent.Apprenticeship.AccountLegalEntityPublicHashedId;
                    ev.LegalEntityName = apprenticeshipEvent.Commitment.LegalEntityName;
                    ev.StartDate = apprenticeshipEvent.Apprenticeship.StartDate.Value;
                    ev.EndDate = apprenticeshipEvent.Apprenticeship.EndDate.Value;
                    ev.PriceEpisodes = priceEpisodes;
                    ev.TrainingType = (CommitmentsV2.Types.TrainingType)apprenticeshipEvent.Apprenticeship.TrainingType;
                    ev.TrainingCode = apprenticeshipEvent.Apprenticeship.TrainingCode;
                    ev.TransferSenderId = apprenticeshipEvent.Apprenticeship.TransferSenderId;
                });

                _logger.Info($"{logMessage} successful");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{logMessage} failed");
            }
        }
    }
}
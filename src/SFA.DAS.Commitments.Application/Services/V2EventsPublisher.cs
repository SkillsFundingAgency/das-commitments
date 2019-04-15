using System;
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


        public V2EventsPublisher(IEndpointInstance endpointInstance, ICommitmentsLogger logger)
        {
            _endpointInstance = endpointInstance;
            _logger = logger;
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
                var priceEpisodes = new PriceEpisode[1];
                priceEpisodes[0] = new PriceEpisode
                {
                    FromDate = apprenticeshipEvent.EffectiveFrom.Value, // Is this correct
                    ToDate = apprenticeshipEvent.EffectiveTo,
                    Cost = apprenticeshipEvent.Apprenticeship.Cost.Value
                };

                await _endpointInstance.Publish<IApprenticeshipCreatedEvent>(ev =>
                {
                    ev.ApprenticeshipId = apprenticeshipEvent.Apprenticeship.Id;
                    ev.CreatedOn = DateTime.Now; // Get this from somewhere
                    ev.Uln = apprenticeshipEvent.Apprenticeship.ULN;
                    ev.ProviderId = apprenticeshipEvent.Apprenticeship.ProviderId;
                    ev.AccountId = apprenticeshipEvent.Apprenticeship.EmployerAccountId;
                    ev.AccountLegalEntityPublicHashedId =
                        apprenticeshipEvent.Apprenticeship.AccountLegalEntityPublicHashedId;
                    ev.LegalEntityName = apprenticeshipEvent.Apprenticeship.LegalEntityId; // This should be the Name
                    ev.StartDate = apprenticeshipEvent.Apprenticeship.StartDate.Value;
                    ev.EndDate = apprenticeshipEvent.Apprenticeship.EndDate.Value;
                    ev.PriceEpisodes = priceEpisodes;
                    ev.ProgrammeType = apprenticeshipEvent.Apprenticeship.TrainingType == TrainingType.Standard
                        ? "Standard"
                        : "Framework";
                    ev.StandardCode = apprenticeshipEvent.Apprenticeship.TrainingType == TrainingType.Standard ? apprenticeshipEvent.Apprenticeship.TrainingCode : null;
                    ev.FrameworkCode = apprenticeshipEvent.Apprenticeship.TrainingType == TrainingType.Framework ? apprenticeshipEvent.Apprenticeship.TrainingCode : null;
                    ev.PathwayCode = apprenticeshipEvent.Apprenticeship.TrainingCode.ToString(); // To string
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
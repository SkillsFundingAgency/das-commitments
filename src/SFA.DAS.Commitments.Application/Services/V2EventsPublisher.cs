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

        public async Task PublishApprenticeshipStopped(Commitment commitment, Apprenticeship apprenticeship)
        {
            if (apprenticeship.StopDate == null)
            {
                throw new InvalidOperationException(
                    $"Cannot publish stopped event for apprenticeship {apprenticeship.Id} because it does not have a stop date");
            }

            _logger.Info($"AQN: {typeof(IApprenticeshipStoppedEvent).AssemblyQualifiedName}");

            var logMessage = $"Publish {nameof(IApprenticeshipStoppedEvent)} message. Provider:{apprenticeship.ProviderId} Commitment:{commitment.Id} Apprenticeship:{apprenticeship.Id} ReservationId:{apprenticeship.ReservationId} StoppedDate:{apprenticeship.StopDate}";

            try
            {
                await _endpointInstance.Publish<IApprenticeshipStoppedEvent>(ev =>
                {
                    ev.AppliedOn = _currentDateTime.Now;
                    ev.ApprenticeshipId = apprenticeship.Id;
                    ev.StopDate = apprenticeship.StopDate.Value;
                });

                _logger.Info($"{logMessage} successful");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{logMessage} failed");
            }
        }

        public async Task PublishDataLockTriageApproved(IApprenticeshipEvent apprenticeshipEvent)
        {
            var logMessage = $"Publish {nameof(IDataLockTriageApprovedEvent)} message. Provider:{apprenticeshipEvent.Commitment.ProviderId} Commitment:{apprenticeshipEvent.Commitment.Id} Apprenticeship:{apprenticeshipEvent.Apprenticeship.Id}";

            try
            {
                var priceEpisodes = apprenticeshipEvent.Apprenticeship.PriceHistory
                    .Select(x => new PriceEpisode { FromDate = x.FromDate, ToDate = x.ToDate, Cost = x.Cost }).ToArray();

                await _endpointInstance.Publish<IDataLockTriageApprovedEvent>(ev =>
                {
                    ev.ApprenticeshipId = apprenticeshipEvent.Apprenticeship.Id;
                    ev.ApprovedOn = _currentDateTime.Now;
                    ev.PriceEpisodes = priceEpisodes;
                    ev.TrainingType = (CommitmentsV2.Types.TrainingType)apprenticeshipEvent.Apprenticeship.TrainingType;
                    ev.TrainingCode = apprenticeshipEvent.Apprenticeship.TrainingCode;
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
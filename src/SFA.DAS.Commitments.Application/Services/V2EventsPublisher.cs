using System;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;
using Apprenticeship = SFA.DAS.Commitments.Domain.Entities.Apprenticeship;

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

        public Task PublishApprenticeshipDeleted(Commitment commitment, Apprenticeship apprenticeship)
        {
            return PublishWithLog<IDraftApprenticeshipDeletedEvent>(ev =>
            {
                ev.DraftApprenticeshipId = apprenticeship.Id;
                ev.CohortId = commitment.Id;
                ev.DeletedOn = DateTime.UtcNow;
                ev.ReservationId = apprenticeship.ReservationId;
                ev.Uln = apprenticeship.ULN;
            }, GetLogMessage(commitment, apprenticeship));
        }

        public Task PublishApprenticeshipCreated(IApprenticeshipEvent apprenticeshipEvent)
        {
            return PublishWithLog<IApprenticeshipCreatedEvent>(ApprenticePreChecks.HasStartAndEndDate, ev =>
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
                ev.PriceEpisodes = GetPriceEpisodes(apprenticeshipEvent.Apprenticeship);
                ev.TrainingType = (CommitmentsV2.Types.TrainingType) apprenticeshipEvent.Apprenticeship.TrainingType;
                ev.TrainingCode = apprenticeshipEvent.Apprenticeship.TrainingCode;
                ev.TransferSenderId = apprenticeshipEvent.Apprenticeship.TransferSenderId;
            }, GetLogMessage(apprenticeshipEvent));
        }

        public Task PublishApprenticeshipStopped(Commitment commitment, Apprenticeship apprenticeship)
        {
            return PublishWithLog<IApprenticeshipStoppedEvent>(ApprenticePreChecks.HasStopDate, ev =>
            {
                ev.AppliedOn = _currentDateTime.Now;
                ev.ApprenticeshipId = apprenticeship.Id;
                ev.StopDate = apprenticeship.StopDate.Value;
            }, GetLogMessage(commitment, apprenticeship));
        }

        public Task PublishDataLockTriageApproved(IApprenticeshipEvent apprenticeshipEvent)
        {
            return PublishWithLog<IDataLockTriageApprovedEvent>(ev =>
            {
                ev.ApprenticeshipId = apprenticeshipEvent.Apprenticeship.Id;
                ev.ApprovedOn = _currentDateTime.Now;
                ev.PriceEpisodes = GetPriceEpisodes(apprenticeshipEvent.Apprenticeship);
                ev.TrainingType = (CommitmentsV2.Types.TrainingType) apprenticeshipEvent.Apprenticeship.TrainingType;
                ev.TrainingCode = apprenticeshipEvent.Apprenticeship.TrainingCode;
            }, GetLogMessage(apprenticeshipEvent));
        }

        public Task PublishApprenticeshipUpdatedApproved(Commitment commitment, Apprenticeship apprenticeship)
        {
            return PublishWithLog<IApprenticeshipUpdatedApprovedEvent>( ApprenticePreChecks.HasStartAndEndDate,ev =>
            {
                ev.ApprenticeshipId = apprenticeship.Id;
                ev.ApprovedOn = _currentDateTime.Now;
                ev.StartDate = apprenticeship.StartDate.Value;
                ev.EndDate = apprenticeship.EndDate.Value;
                ev.PriceEpisodes = GetPriceEpisodes(apprenticeship);
                ev.TrainingType = (CommitmentsV2.Types.TrainingType)apprenticeship.TrainingType;
                ev.TrainingCode = apprenticeship.TrainingCode;
            }, GetLogMessage(commitment, apprenticeship));
        }

        private enum ApprenticePreChecks
        {
            NotRequired = 1,
            HasStartDate = 2,
            HasEndDate = 4,
            HasStopDate = 8,
            HasStartAndEndDate = HasStartDate | HasEndDate
        }

        private Task PublishWithLog<TEvent>(Action<TEvent> messageConstructor, string message) where TEvent : class
        {
            return PublishWithLog(ApprenticePreChecks.NotRequired, messageConstructor, message);
        }

        /// <summary>
        ///     publish the specified message and log whether the publish was successful or not.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="checks">Specified properties will be checked for null value before calling the message constructor</param>
        /// <param name="messageConstructor">Sets values on an instance of the message</param>
        /// <param name="message">A log message that will be recorded with the success or failure message</param>
        /// <returns></returns>
        private async Task PublishWithLog<TEvent>(ApprenticePreChecks checks, Action<TEvent> messageConstructor, string message) where TEvent : class
        {
            var logMessage = $"Publish {typeof(TEvent).Name} message. {message}";
            try
            {
                await _endpointInstance.Publish<TEvent>(messageConstructor);

                _logger.Info($"{logMessage} successful");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{logMessage} failed");
                throw;
            }
        }

        private void DoPreChecks<TEvent>(ApprenticePreChecks checks, Apprenticeship apprenticeship) where TEvent : class
        {
            void DoCheckIf(ApprenticePreChecks qualifyingFlag, Action check)
            {
                if ((checks & qualifyingFlag) == qualifyingFlag)
                {
                    check();
                }
            }

            if (checks == ApprenticePreChecks.NotRequired)
            {
                return;
            }

            EnsureHasApprenticeship<TEvent>(apprenticeship);

            DoCheckIf(ApprenticePreChecks.HasStartDate, () => EnsureHasStartDate<TEvent>(apprenticeship));
            DoCheckIf(ApprenticePreChecks.HasEndDate, () => EnsureHasEndDate<TEvent>(apprenticeship));
            DoCheckIf(ApprenticePreChecks.HasStopDate, () => EnsureHasStopDate<TEvent>(apprenticeship));
        }

        private PriceEpisode[] GetPriceEpisodes(Apprenticeship apprenticeship)
        {
            return apprenticeship
                .PriceHistory
                .Select(x => new PriceEpisode
                {
                    FromDate = x.FromDate,
                    ToDate = x.ToDate,
                    Cost = x.Cost
                }).ToArray();
        }

        private void EnsureHasApprenticeship<TEvent>(Apprenticeship apprenticeship) where TEvent : class
        {
            if (apprenticeship == null)
            {
                throw new InvalidOperationException(
                    $"Cannot publish {typeof(TEvent)} event for apprenticeship because the apprenticeship is null");
            }
        }

        private void EnsureHasStopDate<TEvent>(Apprenticeship apprenticeship) where TEvent : class
        {
            if (apprenticeship.StopDate == null)
            {
                throw new InvalidOperationException(
                    $"Cannot publish {typeof(TEvent)} event for apprenticeship {apprenticeship.Id} because it does not have a stop date");
            }
        }

        private void EnsureHasStartDate<TEvent>(Apprenticeship apprenticeship) where TEvent : class
        {
            if (apprenticeship.StartDate == null)
            {
                throw new InvalidOperationException(
                    $"Cannot publish {typeof(TEvent)} event for apprenticeship {apprenticeship.Id} because it does not have a start date");
            }
        }

        private void EnsureHasEndDate<TEvent>(Apprenticeship apprenticeship) where TEvent : class
        {
            if (apprenticeship.EndDate == null)
            {
                throw new InvalidOperationException(
                    $"Cannot publish {typeof(TEvent)} event for apprenticeship {apprenticeship.Id} because it does not have a end date");
            }
        }

        private string GetLogMessage(Commitment commitment, Apprenticeship apprenticeship)
        {
            return "Provider:{commitment.ProviderId} Commitment:{commitment.Id} Apprenticeship:{apprenticeship.Id}";
        }

        private string GetLogMessage(IApprenticeshipEvent apprenticeshipEvent)
        {
            return
                $"Provider:{apprenticeshipEvent.Commitment.ProviderId} Commitment:{apprenticeshipEvent.Commitment.Id} Apprenticeship:{apprenticeshipEvent.Apprenticeship.Id}";
        }
    }
}
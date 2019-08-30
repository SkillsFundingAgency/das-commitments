﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NServiceBus;
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
            return PublishWithLog<DraftApprenticeshipDeletedEvent>(ApprenticePreChecks.NotRequired, apprenticeship, ev =>
            {
                ev.DraftApprenticeshipId = apprenticeship.Id;
                ev.CohortId = commitment.Id;
                ev.DeletedOn = DateTime.UtcNow;
                ev.ReservationId = apprenticeship.ReservationId;
                ev.Uln = apprenticeship.ULN;
            }, GetLogMessage(commitment, apprenticeship));
        }

        public Task PublishApprenticeshipStopDateChanged(Commitment commitment, Apprenticeship apprenticeship)
        {
            return PublishWithLog<ApprenticeshipStopDateChangedEvent>(ApprenticePreChecks.HasStopDate, apprenticeship, ev =>
                {
                    ev.ApprenticeshipId = apprenticeship.Id;
                    ev.StopDate = apprenticeship.StopDate.Value;
                    ev.ChangedOn = _currentDateTime.Now;
                }, GetLogMessage(commitment, apprenticeship));
        }

        public async Task PublishApprenticeshipCreated(IApprenticeshipEvent apprenticeshipEvent)
        {
            DateTime GetTransferApprovalOrAgreedOnDate()
            {
                if (apprenticeshipEvent.Commitment.TransferApprovalActionedOn.HasValue)
                {
                    return apprenticeshipEvent.Commitment.TransferApprovalActionedOn.Value;
                }

                return apprenticeshipEvent.Apprenticeship.AgreedOn.Value;
            }

            var logMessage = $"Publish {typeof(ApprenticeshipCreatedEvent).Name} message. {GetLogMessage(apprenticeshipEvent)}";

            try
            {
                DoPreChecks<ApprenticeshipCreatedEvent>(ApprenticePreChecks.HasStartAndEndDate, apprenticeshipEvent?.Apprenticeship);

                await _endpointInstance.Publish(new ApprenticeshipCreatedEvent
                {
                    ApprenticeshipId = apprenticeshipEvent.Apprenticeship.Id,
                    AgreedOn = apprenticeshipEvent.Apprenticeship.AgreedOn.Value,
                    CreatedOn = GetTransferApprovalOrAgreedOnDate(),
                    Uln = apprenticeshipEvent.Apprenticeship.ULN,
                    ProviderId = apprenticeshipEvent.Apprenticeship.ProviderId,
                    AccountId = apprenticeshipEvent.Apprenticeship.EmployerAccountId,
                    AccountLegalEntityPublicHashedId = apprenticeshipEvent.Apprenticeship.AccountLegalEntityPublicHashedId,
                    LegalEntityName = apprenticeshipEvent.Commitment.LegalEntityName,
                    StartDate = apprenticeshipEvent.Apprenticeship.StartDate.Value,
                    EndDate = apprenticeshipEvent.Apprenticeship.EndDate.Value,
                    PriceEpisodes = GetPriceEpisodes(apprenticeshipEvent.Apprenticeship),
                    TrainingType = (CommitmentsV2.Types.ProgrammeType)apprenticeshipEvent.Apprenticeship.TrainingType,
                    TrainingCode = apprenticeshipEvent.Apprenticeship.TrainingCode,
                    TransferSenderId = apprenticeshipEvent.Apprenticeship.TransferSenderId,
                    ApprenticeshipEmployerTypeOnApproval = apprenticeshipEvent.Commitment.ApprenticeshipEmployerTypeOnApproval
            });

                _logger.Info($"{logMessage} successful");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{logMessage} failed");
                throw;
            }
        }

        public Task PublishApprenticeshipStopped(Commitment commitment, Apprenticeship apprenticeship)
        {
            return PublishWithLog<ApprenticeshipStoppedEvent>(ApprenticePreChecks.HasStopDate, apprenticeship, ev =>
            {
                ev.AppliedOn = _currentDateTime.Now;
                ev.ApprenticeshipId = apprenticeship.Id;
                ev.StopDate = apprenticeship.StopDate.Value;
            }, GetLogMessage(commitment, apprenticeship));
        }

        public Task PublishDataLockTriageApproved(IApprenticeshipEvent apprenticeshipEvent)
        {
            return PublishWithLog<DataLockTriageApprovedEvent>(  ApprenticePreChecks.NotRequired, apprenticeshipEvent?.Apprenticeship, ev =>
            {
                ev.ApprenticeshipId = apprenticeshipEvent.Apprenticeship.Id;
                ev.ApprovedOn = _currentDateTime.Now;
                ev.PriceEpisodes = GetPriceEpisodes(apprenticeshipEvent.Apprenticeship);
                ev.TrainingType = (CommitmentsV2.Types.ProgrammeType) apprenticeshipEvent.Apprenticeship.TrainingType;
                ev.TrainingCode = apprenticeshipEvent.Apprenticeship.TrainingCode;
            }, GetLogMessage(apprenticeshipEvent));
        }

        public Task PublishApprenticeshipUpdatedApproved(Commitment commitment, Apprenticeship apprenticeship)
        {
            return PublishWithLog<ApprenticeshipUpdatedApprovedEvent>( ApprenticePreChecks.HasStartAndEndDate, apprenticeship, ev =>
            {
                ev.ApprenticeshipId = apprenticeship.Id;
                ev.ApprovedOn = _currentDateTime.Now;
                ev.StartDate = apprenticeship.StartDate.Value;
                ev.EndDate = apprenticeship.EndDate.Value;
                ev.PriceEpisodes = GetPriceEpisodes(apprenticeship);
                ev.TrainingType = (CommitmentsV2.Types.ProgrammeType)apprenticeship.TrainingType;
                ev.TrainingCode = apprenticeship.TrainingCode;
                ev.Uln = apprenticeship.ULN;
            }, GetLogMessage(commitment, apprenticeship));
        }

        public Task PublishApprenticeshipPaused(Commitment commitment, Apprenticeship apprenticeship)
        {
            return PublishWithLog<ApprenticeshipPausedEvent>(ApprenticePreChecks.HasPauseDate, apprenticeship, ev =>
            {
                ev.ApprenticeshipId = apprenticeship.Id;
                ev.PausedOn = apprenticeship.PauseDate.Value;
            }, GetLogMessage(commitment, apprenticeship));
        }

        public Task PublishApprenticeshipResumed(Commitment commitment, Apprenticeship apprenticeship)
        {
            return PublishWithLog<ApprenticeshipResumedEvent>(apprenticeship, ev =>
            {
                ev.ApprenticeshipId = apprenticeship.Id;
                ev.ResumedOn = _currentDateTime.Now;
            }, GetLogMessage(commitment, apprenticeship));
        }

        public async Task PublishPaymentOrderChanged(long employerAccountId, IEnumerable<int> paymentOrder)
        {
            var logMessage = $"Publish {typeof(PaymentOrderChangedEvent).Name} message. For EmployerAccountId : {employerAccountId}";

            try
            {
                if (employerAccountId == 0) throw new InvalidOperationException("EmployerAccountId cannot be 0");
                if (paymentOrder == null) throw new InvalidOperationException("Priorities are expected");

                await _endpointInstance.Publish(new PaymentOrderChangedEvent
                {
                    AccountId = employerAccountId,
                    PaymentOrder = paymentOrder.ToArray()
                } );

                _logger.Info($"{logMessage} successful");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{logMessage} failed");
                throw;
            }
        }

        public Task PublishBulkUploadIntoCohortCompleted(long providerId, long cohortId, uint numberOfApprentices)
        {
            var @event = new BulkUploadIntoCohortCompletedEvent
            {
                CohortId = cohortId,
                NumberOfApprentices = numberOfApprentices,
                UploadedOn = _currentDateTime.Now
            };

            return PublishWithLog(@event, $"Provider: {providerId} CohortId: {cohortId} Number of apprentices: {numberOfApprentices}");
        }

        private enum ApprenticePreChecks
        {
            NotRequired = 1,
            HasStartDate = 2,
            HasEndDate = 4,
            HasStopDate = 8,
            HasPauseDate = 16,
            HasStartAndEndDate = HasStartDate | HasEndDate
        }

        private Task PublishWithLog<TEvent>(Apprenticeship apprentice, Action<TEvent> messageConstructor, string message) where TEvent : class
        {
            return PublishWithLog(ApprenticePreChecks.NotRequired, apprentice, messageConstructor, message);
        }

        /// <summary>
        ///     publish the specified message and log whether the publish was successful or not.
        /// </summary>
        /// <typeparam name="TEvent"></typeparam>
        /// <param name="checks">Specified properties will be checked for null value before calling the message constructor</param>
        /// <param name="messageConstructor">Sets values on an instance of the message</param>
        /// <param name="apprenticeship"></param>
        /// <param name="message">A log message that will be recorded with the success or failure message</param>
        /// <returns></returns>
        private async Task PublishWithLog<TEvent>(ApprenticePreChecks checks, Apprenticeship apprenticeship, Action<TEvent> messageConstructor, string message) where TEvent : class
        {
            var logMessage = $"Publish {typeof(TEvent).Name} message. {message}";
            try
            {
                DoPreChecks<TEvent>(checks, apprenticeship);
                await _endpointInstance.Publish<TEvent>(messageConstructor);

                _logger.Info($"{logMessage} successful");
            }
            catch (Exception e)
            {
                _logger.Error(e, $"{logMessage} failed");
                throw;
            }
        }

        private async Task PublishWithLog<TEvent>(TEvent @event, string message) where TEvent : class
        {
            var logMessage = $"Publish {typeof(TEvent).Name} message. {message}";
            try
            {
                await _endpointInstance.Publish(@event);
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
            DoCheckIf(ApprenticePreChecks.HasPauseDate, () => EnsureHasPauseDate<TEvent>(apprenticeship));
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

        private void EnsureHasPauseDate<TEvent>(Apprenticeship apprenticeship) where TEvent : class
        {
            if (apprenticeship.PauseDate == null)
            {
                throw new InvalidOperationException(
                    $"Cannot publish {typeof(TEvent)} event for apprenticeship {apprenticeship.Id} because it does not have a pause date");
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
            return $"Provider:{commitment.ProviderId} Commitment:{commitment.Id} Apprenticeship:{apprenticeship.Id}";
        }

        private string GetLogMessage(IApprenticeshipEvent apprenticeshipEvent)
        {
            return
                $"Provider:{apprenticeshipEvent.Commitment.ProviderId} Commitment:{apprenticeshipEvent.Commitment.Id} Apprenticeship:{apprenticeshipEvent.Apprenticeship.Id}";
        }
    }
}
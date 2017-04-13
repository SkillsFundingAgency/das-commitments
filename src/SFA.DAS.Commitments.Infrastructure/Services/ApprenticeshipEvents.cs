using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;
using AgreementStatus = SFA.DAS.Events.Api.Types.AgreementStatus;
using PaymentStatus = SFA.DAS.Events.Api.Types.PaymentStatus;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipEvents : IApprenticeshipEvents
    {
        private readonly IEventsApi _eventsApi;
        private readonly ICommitmentsLogger _logger;

        public ApprenticeshipEvents(IEventsApi eventsApi, ICommitmentsLogger logger)
        {
            _eventsApi = eventsApi;
            _logger = logger;
        }

        public async Task PublishEvent(Commitment commitment, Apprenticeship apprenticeship, string @event)
        {
            ApprenticeshipEvent apprenticeshipEvent = CreateEvent(commitment, apprenticeship, @event, (PaymentStatus)apprenticeship.PaymentStatus);

            _logger.Info($"Create apprenticeship event: {apprenticeshipEvent.Event}", commitmentId: commitment.Id, apprenticeshipId: apprenticeship.Id);
            await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
        }

        public async Task BulkPublishEvent(Commitment commitment, IList<Apprenticeship> apprenticeships, string @event)
        {
            var eventsToPublish = new List<ApprenticeshipEvent>();

            foreach (var apprenticeship in apprenticeships)
            {
                eventsToPublish.Add(CreateEvent(commitment, apprenticeship, @event, (PaymentStatus)apprenticeship.PaymentStatus));
            }

            await BulkPublishEvent(eventsToPublish);
        }

        public async Task PublishDeletionEvent(Commitment commitment, Apprenticeship apprenticeship, string @event)
        {
            ApprenticeshipEvent apprenticeshipEvent = CreateEvent(commitment, apprenticeship, @event, PaymentStatus.Deleted);

            _logger.Info($"Create apprenticeship event: {apprenticeshipEvent.Event}", commitmentId: commitment.Id, apprenticeshipId: apprenticeship.Id);
            await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
        }

        public async Task BulkPublishDeletionEvent(Commitment commitment, IList<Apprenticeship> apprenticeships, string @event)
        {
            var eventsToPublish = new List<ApprenticeshipEvent>();

            foreach (var apprenticeship in apprenticeships)
            {
                eventsToPublish.Add(CreateEvent(commitment, apprenticeship, @event, PaymentStatus.Deleted));
            }

            await BulkPublishEvent(eventsToPublish);
        }

        public async Task PublishChangeApprenticeshipStatusEvent(Commitment commitment, Apprenticeship apprenticeship, Domain.Entities.PaymentStatus paymentStatus)
        {
            ApprenticeshipEvent apprenticeshipEvent = CreateEvent(commitment, apprenticeship, "APPRENTICESHIP-UPDATED", (PaymentStatus)paymentStatus);

            _logger.Info($"Create apprenticeship event: {apprenticeshipEvent.Event}", commitmentId: commitment.Id, apprenticeshipId: apprenticeship.Id);

            await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
        }

        private async Task BulkPublishEvent(List<ApprenticeshipEvent> eventsToPublish)
        {
            if (eventsToPublish.Count > 0)
            {
                _logger.Info($"Creating apprenticeship events");
                await _eventsApi.BulkCreateApprenticeshipEvent(eventsToPublish);
            }
        }

        private static ApprenticeshipEvent CreateEvent(Commitment commitment, Apprenticeship apprenticeship, string @event, PaymentStatus paymentStatus)
        {
            return new ApprenticeshipEvent
            {
                AgreementStatus = (AgreementStatus)apprenticeship.AgreementStatus,
                ApprenticeshipId = apprenticeship.Id,
                EmployerAccountId = commitment.EmployerAccountId.ToString(),
                LearnerId = apprenticeship.ULN ?? "NULL",
                TrainingId = apprenticeship.TrainingCode ?? string.Empty,
                Event = @event,
                PaymentStatus = paymentStatus,
                ProviderId = commitment.ProviderId != null ? commitment.ProviderId.ToString() : string.Empty,
                TrainingEndDate = apprenticeship.EndDate ?? DateTime.MaxValue,
                TrainingStartDate = apprenticeship.StartDate ?? DateTime.MaxValue,
                TrainingTotalCost = apprenticeship.Cost ?? -1,
                TrainingType = apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard,
                PaymentOrder = apprenticeship.PaymentOrder,
                LegalEntityId = commitment.LegalEntityId,
                LegalEntityName = commitment.LegalEntityName,
                LegalEntityOrganisationType = commitment.LegalEntityOrganisationType.ToString()
            };
        }
    }
}

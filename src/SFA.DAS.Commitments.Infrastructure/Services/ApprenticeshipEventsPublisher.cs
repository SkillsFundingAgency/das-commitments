using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;
using AgreementStatus = SFA.DAS.Events.Api.Types.AgreementStatus;
using PaymentStatus = SFA.DAS.Events.Api.Types.PaymentStatus;
using PriceHistory = SFA.DAS.Events.Api.Types.PriceHistory;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipEventsPublisher : IApprenticeshipEventsPublisher
    {
        private readonly IEventsApi _eventsApi;
        private readonly ICommitmentsLogger _logger;
        private readonly int _maxBatchSize = 1000;

        public ApprenticeshipEventsPublisher(IEventsApi eventsApi, ICommitmentsLogger logger)
        {
            _eventsApi = eventsApi;
            _logger = logger;
        }

        public async Task Publish(IApprenticeshipEventsList events)
        {
            _logger.Info($"Publishing {events.Events.Count} events");
            var apiEvents = events.Events.Select(x => CreateEvent(x.Commitment, x.Apprenticeship, x.Event, x.EffectiveFrom, x.EffectiveTo, x.PriceHistory));

            var batches = SplitList(apiEvents.ToList(), _maxBatchSize).ToList();

            if (batches.Count() > 1)
            {
                _logger.Info($"Splitting events into {batches.Count} batches of up to {_maxBatchSize}");
            }

            foreach (var batch in batches)
            {
                _logger.Info($"Calling events api to bulk create {batch.Count} events");
                await _eventsApi.BulkCreateApprenticeshipEvent(batch.ToList());
            }

            events.Clear();
        }

        private static Events.Api.Types.ApprenticeshipEvent CreateEvent(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom, DateTime? effectiveTo, IEnumerable<Domain.Entities.PriceHistory> priceHistory)
        {
            return new Events.Api.Types.ApprenticeshipEvent
            {
                AgreementStatus = (AgreementStatus)apprenticeship.AgreementStatus,
                ApprenticeshipId = apprenticeship.Id,
                EmployerAccountId = commitment.EmployerAccountId.ToString(),
                LearnerId = apprenticeship.ULN,
                TrainingId = apprenticeship.TrainingCode,
                Event = @event,
                PaymentStatus = (PaymentStatus)apprenticeship.PaymentStatus,
                ProviderId = commitment.ProviderId?.ToString(),
                TrainingEndDate = apprenticeship.EndDate,
                TrainingStartDate = apprenticeship.StartDate,
                TrainingTotalCost = apprenticeship.Cost, //todo: current or latest?
                TrainingType = apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard,
                PaymentOrder = apprenticeship.PaymentOrder,
                LegalEntityId = commitment.LegalEntityId,
                LegalEntityName = commitment.LegalEntityName,
                LegalEntityOrganisationType = commitment.LegalEntityOrganisationType.ToString(),
                DateOfBirth = apprenticeship.DateOfBirth,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveTo,
                PriceHistory = MapPriceHistory(apprenticeship.PriceHistory),
                TransferSenderId = commitment.TransferSenderId,
                TransferSenderName = commitment.TransferSenderName,
                TransferApprovalStatus = (Events.Api.Types.TransferApprovalStatus?) commitment.TransferApprovalStatus,
                TransferApprovalActionedOn = commitment.TransferApprovalActionedOn
            };
        }

        private static IEnumerable<PriceHistory> MapPriceHistory(IEnumerable<Domain.Entities.PriceHistory> source)
        {
            return source.Select(x => new PriceHistory
            {
                TotalCost = x.Cost, EffectiveFrom = x.FromDate, EffectiveTo = x.ToDate
            });
        }


        private static IEnumerable<List<T>> SplitList<T>(List<T> items, int chunkSize)
        {
            for (var i = 0; i < items.Count; i += chunkSize)
            {
                yield return items.GetRange(i, Math.Min(chunkSize, items.Count - i));
            }
        }
    }
}

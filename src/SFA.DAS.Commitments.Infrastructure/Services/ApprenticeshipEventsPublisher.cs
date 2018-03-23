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

        public ApprenticeshipEventsPublisher(IEventsApi eventsApi, ICommitmentsLogger logger)
        {
            _eventsApi = eventsApi;
            _logger = logger;
        }

        public async Task Publish(IApprenticeshipEventsList events)
        {
            var apiEvents = events.Events.Select(x => CreateEvent(x.Commitment, x.Apprenticeship, x.Event, x.EffectiveFrom, x.EffectiveTo, x.PriceHistory));
            await _eventsApi.BulkCreateApprenticeshipEvent(apiEvents.ToList());
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
                TransferSenderApproved = commitment.TransferApprovalStatus == TransferApprovalStatus.TransferApproved
            };
        }

        private static IEnumerable<PriceHistory> MapPriceHistory(IEnumerable<Domain.Entities.PriceHistory> source)
        {
            return source.Select(x => new PriceHistory
            {
                TotalCost = x.Cost, EffectiveFrom = x.FromDate, EffectiveTo = x.ToDate
            });
        }
    }
}

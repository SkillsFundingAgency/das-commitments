using System;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;
using AgreementStatus = SFA.DAS.Events.Api.Types.AgreementStatus;
using PaymentStatus = SFA.DAS.Events.Api.Types.PaymentStatus;

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
            var apiEvents = events.Events.Select(x => CreateEvent(x.Commitment, x.Apprenticeship, x.Event, x.EffectiveFrom, x.EffectiveTo));
            _logger.Trace($"ApprenticeshipEventsPublisher: Publishing {apiEvents.Count()} events");
            await _eventsApi.BulkCreateApprenticeshipEvent(apiEvents.ToList());
        }

        private static Events.Api.Types.ApprenticeshipEvent CreateEvent(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom, DateTime? effectiveTo)
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
                TrainingTotalCost = apprenticeship.Cost,
                TrainingType = apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard,
                PaymentOrder = apprenticeship.PaymentOrder,
                LegalEntityId = commitment.LegalEntityId,
                LegalEntityName = commitment.LegalEntityName,
                LegalEntityOrganisationType = commitment.LegalEntityOrganisationType.ToString(),
                DateOfBirth = apprenticeship.DateOfBirth,
                EffectiveFrom = effectiveFrom,
                EffectiveTo = effectiveTo
            };
        }
    }
}

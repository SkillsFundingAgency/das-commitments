using System;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    using NLog;

    using AgreementStatus = SFA.DAS.Events.Api.Types.AgreementStatus;
    using PaymentStatus = SFA.DAS.Events.Api.Types.PaymentStatus;

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
            // only publish if data is reasonably complete
            if (commitment.ProviderId != null && apprenticeship.EndDate != null && apprenticeship.StartDate != null && apprenticeship.Cost != null && apprenticeship.TrainingCode != null)
            {
                var apprenticeshipEvent = new ApprenticeshipEvent
                {
                    AgreementStatus = (AgreementStatus)apprenticeship.AgreementStatus,
                    ApprenticeshipId = apprenticeship.Id,
                    EmployerAccountId = commitment.EmployerAccountId.ToString(),
                    LearnerId = apprenticeship.ULN ?? "NULL",
                    TrainingId = apprenticeship.TrainingCode ?? string.Empty,
                    Event = @event,
                    PaymentStatus = (PaymentStatus)apprenticeship.PaymentStatus,
                    ProviderId = commitment.ProviderId.ToString(),
                    TrainingEndDate = apprenticeship.EndDate ?? DateTime.MaxValue,
                    TrainingStartDate = apprenticeship.StartDate ?? DateTime.MaxValue,
                    TrainingTotalCost = apprenticeship.Cost ?? decimal.MinValue,
                    TrainingType = apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard
                };

                _logger.Info($"Create apprenticeship event: {apprenticeshipEvent.Event}", commitmentId: commitment.Id, apprenticeshipId: apprenticeship.Id);
                await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
            }
        }
    }
}

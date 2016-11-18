using System;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipEvents : IApprenticeshipEvents
    {
        private readonly IEventsApi _eventsApi;

        public ApprenticeshipEvents(IEventsApi eventsApi)
        {
            _eventsApi = eventsApi;
        }

        public async Task PublishEvent(Commitment commitment, Apprenticeship apprenticeship, string @event)
        {
            // only publish if data is reasonably complete
            if (commitment.ProviderId != null && apprenticeship.EndDate != null && apprenticeship.StartDate != null && apprenticeship.Cost != null && apprenticeship.TrainingCode != null)
            {
                var apprenticeshipEvent = new ApprenticeshipEvent
                {
                    AgreementStatus = apprenticeship.AgreementStatus.ToString(),
                    ApprenticeshipId = apprenticeship.Id,
                    EmployerAccountId = commitment.EmployerAccountId.ToString(),
                    LearnerId = apprenticeship.ULN ?? "NULL",
                    TrainingId = apprenticeship.TrainingCode ?? string.Empty,
                    Event = @event,
                    PaymentStatus = apprenticeship.PaymentStatus.ToString(),
                    ProviderId = commitment.ProviderId.ToString(),
                    TrainingEndDate = apprenticeship.EndDate ?? DateTime.MaxValue,
                    TrainingStartDate = apprenticeship.StartDate ?? DateTime.MaxValue,
                    TrainingTotalCost = apprenticeship.Cost ?? decimal.MinValue,
                    TrainingType = apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard
                };

                await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
            }
        }
    }
}

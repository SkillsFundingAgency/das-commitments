using System;
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

        public async Task PublishEvent(Apprenticeship apprenticeship, string @event)
        {
            // only publish if data is reasonably complete
            if (apprenticeship.EndDate != null && apprenticeship.StartDate != null && apprenticeship.Cost != null && apprenticeship.TrainingCode != null)
            {
                var apprenticeshipEvent = new ApprenticeshipEvent
                {
                    AgreementStatus = (AgreementStatus)apprenticeship.AgreementStatus,
                    ApprenticeshipId = apprenticeship.Id,
                    EmployerAccountId = apprenticeship.EmployerAccountId.ToString(),
                    LearnerId = apprenticeship.ULN ?? "NULL",
                    TrainingId = apprenticeship.TrainingCode ?? string.Empty,
                    Event = @event,
                    PaymentStatus = (PaymentStatus)apprenticeship.PaymentStatus,
                    ProviderId = apprenticeship.ProviderId.ToString(),
                    TrainingEndDate = apprenticeship.EndDate ?? DateTime.MaxValue,
                    TrainingStartDate = apprenticeship.StartDate ?? DateTime.MaxValue,
                    TrainingTotalCost = apprenticeship.Cost ?? decimal.MinValue,
                    TrainingType = apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard,
                    PaymentOrder = apprenticeship.PaymentOrder
                };

                _logger.Info($"Create apprenticeship event: {apprenticeshipEvent.Event}", apprenticeshipId: apprenticeship.Id);
                await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
            }
        }

        public async Task PublishEvent(Commitment commitment, Apprenticeship apprenticeship, string @event)
        {
            //todo: consider removing this overload as all the props should be available on the apprenticeship (don't need the commitment). BUT in some cases this is called with an apprenticeship that has been sent from outside the API so won't have the "apprenticeship summary" props set
            // only publish if data is reasonably complete
            if (commitment.ProviderId != null && apprenticeship.EndDate != null && apprenticeship.StartDate != null && apprenticeship.Cost != null && apprenticeship.TrainingCode != null)
            {
                var apprenticeshipEvent = new ApprenticeshipEvent
                {
                    AgreementStatus = (AgreementStatus) apprenticeship.AgreementStatus,
                    ApprenticeshipId = apprenticeship.Id,
                    EmployerAccountId = commitment.EmployerAccountId.ToString(),
                    LearnerId = apprenticeship.ULN ?? "NULL",
                    TrainingId = apprenticeship.TrainingCode ?? string.Empty,
                    Event = @event,
                    PaymentStatus = (PaymentStatus) apprenticeship.PaymentStatus,
                    ProviderId = commitment.ProviderId.ToString(),
                    TrainingEndDate = apprenticeship.EndDate ?? DateTime.MaxValue,
                    TrainingStartDate = apprenticeship.StartDate ?? DateTime.MaxValue,
                    TrainingTotalCost = apprenticeship.Cost ?? decimal.MinValue,
                    TrainingType = apprenticeship.TrainingType == TrainingType.Framework ? TrainingTypes.Framework : TrainingTypes.Standard,
                    PaymentOrder = apprenticeship.PaymentOrder
                };

                _logger.Info($"Create apprenticeship event: {apprenticeshipEvent.Event}", commitmentId: commitment.Id, apprenticeshipId: apprenticeship.Id);
                await _eventsApi.CreateApprenticeshipEvent(apprenticeshipEvent);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Events.Api.Types;
using AgreementStatus = SFA.DAS.Events.Api.Types.AgreementStatus;
using PaymentStatus = SFA.DAS.Events.Api.Types.PaymentStatus;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipEventsList : IApprenticeshipEventsList
    {
        private readonly List<ApprenticeshipEvent> _events;

        public IReadOnlyList<ApprenticeshipEvent> Events => _events;

        public ApprenticeshipEventsList()
        {
            _events = new List<ApprenticeshipEvent>();
        }

        public void Add(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom = null)
        {
            _events.Add(CreateEvent(commitment, apprenticeship, @event, effectiveFrom));
        }

        private static ApprenticeshipEvent CreateEvent(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom = null)
        {
            return new ApprenticeshipEvent
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
                EffectiveTo = null
            };
        }
    }
}

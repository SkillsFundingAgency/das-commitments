using System;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipEvent : IApprenticeshipEvent
    {
        public Commitment Commitment { get; }
        public Apprenticeship Apprenticeship { get; }
        public string Event { get; }
        public DateTime? EffectiveFrom { get; }
        public DateTime? EffectiveTo { get; }

        public ApprenticeshipEvent(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom, DateTime? effectiveTo)
        {
            Commitment = commitment;
            Apprenticeship = apprenticeship;
            Event = @event;
            EffectiveFrom = effectiveFrom;
            EffectiveTo = effectiveTo;
        }
    }
}

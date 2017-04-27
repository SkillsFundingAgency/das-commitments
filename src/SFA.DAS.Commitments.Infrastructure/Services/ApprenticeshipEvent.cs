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

        public ApprenticeshipEvent(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom)
        {
            Commitment = commitment;
            Apprenticeship = apprenticeship;
            Event = @event;
            EffectiveFrom = effectiveFrom;
        }
    }
}

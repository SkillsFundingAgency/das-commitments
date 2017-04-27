using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class ApprenticeshipEventsList : IApprenticeshipEventsList
    {
        private readonly List<IApprenticeshipEvent> _events;

        public IReadOnlyList<IApprenticeshipEvent> Events => _events;

        public ApprenticeshipEventsList()
        {
            _events = new List<IApprenticeshipEvent>();
        }

        public void Add(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom = null)
        {
            _events.Add(CreateEvent(commitment, apprenticeship, @event, effectiveFrom));
        }

        private static IApprenticeshipEvent CreateEvent(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom = null)
        {
            return new ApprenticeshipEvent(commitment, apprenticeship, @event, effectiveFrom);
        }
    }
}

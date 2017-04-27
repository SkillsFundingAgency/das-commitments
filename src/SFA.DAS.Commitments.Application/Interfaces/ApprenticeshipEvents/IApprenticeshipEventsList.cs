using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents
{
    public interface IApprenticeshipEventsList
    {
        void Add(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom = null);
        IReadOnlyList<IApprenticeshipEvent> Events { get; }
    }
}

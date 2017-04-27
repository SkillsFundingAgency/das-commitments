using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    public interface IApprenticeshipEventsList
    {
        void Add(Commitment commitment, Apprenticeship apprenticeship, string @event, DateTime? effectiveFrom = null);
        IReadOnlyList<ApprenticeshipEvent> Events { get; }
    }
}

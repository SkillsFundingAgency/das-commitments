using System;
using System.Collections.Generic;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents
{
    public interface IApprenticeshipEvent
    {
        Commitment Commitment { get; }
        Apprenticeship Apprenticeship { get; }
        string Event { get; }
        DateTime? EffectiveFrom { get; }
        DateTime? EffectiveTo { get; }
        IEnumerable<PriceHistory> PriceHistory { get; set; }
    }
}

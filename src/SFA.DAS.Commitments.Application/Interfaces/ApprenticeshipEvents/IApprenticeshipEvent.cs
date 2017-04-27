using System;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.Interfaces.ApprenticeshipEvents
{
    public interface IApprenticeshipEvent
    {
        Commitment Commitment { get; }
        Apprenticeship Apprenticeship { get; }
        string Event { get; }
        DateTime? EffectiveFrom { get; }
    }
}

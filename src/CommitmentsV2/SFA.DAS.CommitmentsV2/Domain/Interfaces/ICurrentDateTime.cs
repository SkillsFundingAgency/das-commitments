using System;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ICurrentDateTime
    {
        DateTime UtcNow { get; }
    }
}
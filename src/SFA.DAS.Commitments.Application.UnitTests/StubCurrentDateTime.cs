using System;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Application.UnitTests
{
    internal sealed class StubCurrentDateTime : ICurrentDateTime
    {
        public DateTime Now
        {
            get
            {
                return DateTime.UtcNow;
            }
        }
    }
}

using System;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    public class CurrentDateTime : ICurrentDateTime
    {
        public DateTime Now => DateTime.UtcNow;
    }
}

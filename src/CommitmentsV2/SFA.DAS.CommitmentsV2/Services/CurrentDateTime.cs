using System;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class CurrentDateTime : ICurrentDateTime
    {
        private readonly DateTime? _time;

        public DateTime UtcNow => _time ?? DateTime.UtcNow;

        public CurrentDateTime()
        {
        }

        public CurrentDateTime(DateTime? value)
        {
            _time = value;
        }
    }
}

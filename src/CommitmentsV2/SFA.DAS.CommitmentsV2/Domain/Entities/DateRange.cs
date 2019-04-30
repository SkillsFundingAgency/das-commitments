using System;
using SFA.DAS.CommitmentsV2.Domain.Extensions;

namespace SFA.DAS.CommitmentsV2.Domain.Entities
{
    public class DateRange
    {
        public DateTime From { get; }
        public DateTime To { get; }

        public DateRange(DateTime from, DateTime to)
        {
            From = from;
            To = to;
        }

        public bool IsZeroDays => From.Equals(To);

        public bool Contains(DateTime value)
        {
            return From.IsBeforeMonth(value) && To.IsAfterMonth(value);
        }
    }
}

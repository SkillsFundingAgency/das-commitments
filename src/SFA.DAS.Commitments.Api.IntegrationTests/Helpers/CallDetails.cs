using System;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers
{
    public class CallDetails
    {
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan CallTime { get; set; }
        public int ThreadId { get; set; }

        public override string ToString()
        {
            return $"{ThreadId,3} {Name,-18} {StartTime} {CallTime} {new string('*', (int)CallTime.TotalSeconds)}";
        }

        public string ToString(DateTime globalStart, TimeSpan globalTime, int timelineWidthInChars)
        {
            var startX = (int)(((StartTime - globalStart).TotalSeconds / globalTime.TotalSeconds) * timelineWidthInChars);
            var length = (int)((CallTime.TotalSeconds / globalTime.TotalSeconds) * timelineWidthInChars);

            return $"{ThreadId,3} {Name,-18} {StartTime:HH:mm:ss:ffff} {CallTime} {new string(' ', startX)}{new string(Name == "GetApprenticeship" ? '-':'=', length)}";
        }
    }
}

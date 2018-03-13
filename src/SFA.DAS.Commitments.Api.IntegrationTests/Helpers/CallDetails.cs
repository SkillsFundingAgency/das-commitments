using System;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers
{
    public class CallDetails
    {
        public DateTime StartTime { get; set; }
        public TimeSpan CallTime { get; set; }
        public int ThreadId { get; set; }
    }
}

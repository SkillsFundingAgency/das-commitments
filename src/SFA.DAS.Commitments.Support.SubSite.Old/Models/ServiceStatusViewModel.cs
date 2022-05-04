using System;

namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class ServiceStatusViewModel
    {
        public string ServiceName { get; set; }
        public string ServiceVersion { get; set; }
        public DateTimeOffset ServiceTime { get; set; }
        public string Request { get; set; }

    }
}
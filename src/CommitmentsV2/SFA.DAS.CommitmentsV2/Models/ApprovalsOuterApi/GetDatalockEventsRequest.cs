using System;

namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi
{
    public class GetDataLockEventsRequest : IGetApiRequest
    {
        public long SinceEventId { get; set; }
        public DateTime? SinceTime { get; set; }
        public string EmployerAccountId { get; set; }
        public long Ukprn { get; set; }
        public int PageNumber { get; set; }

        public GetDataLockEventsRequest()
        {
        }

        public GetDataLockEventsRequest(long sinceEventId)
        {
            SinceEventId = sinceEventId;
        }

        public string GetUrl => BuildUrl();

        private string BuildUrl()
        {
            var url = $"datalock/events?page={PageNumber}";
            if (SinceEventId > 0)
            {
                url += $"&sinceEventId={SinceEventId}";
            }
            if (SinceTime.HasValue)
            {
                url += $"&sinceTime={SinceTime.Value:yyyy-MM-ddTHH:mm:ss}";
            }
            if (!string.IsNullOrEmpty(EmployerAccountId))
            {
                url += $"&employerAccountId={EmployerAccountId}";
            }
            if (Ukprn > 0)
            {
                url += $"&ukprn={Ukprn}";
            }

            return url;
        }
    }
}
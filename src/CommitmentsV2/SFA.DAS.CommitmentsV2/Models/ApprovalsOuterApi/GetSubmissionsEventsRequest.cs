namespace SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

public class GetSubmissionsEventsRequest : IGetApiRequest
{
    public long SinceEventId { get; set; }
    public DateTime? SinceTime { get; set; }
    public long Ukprn { get; set; }
    public int PageNumber { get; set; }

    public string GetUrl => BuildUrl();

    private string BuildUrl()
    {
        var url = $"PaymentEvents/submissionEvents?pageNumber={PageNumber}";
        if (SinceEventId > 0)
        {
            url += $"&sinceEventId={SinceEventId}";
        }
        if (SinceTime.HasValue)
        {
            url += $"&sinceTime={SinceTime.Value:yyyy-MM-ddTHH:mm:ss}";
        }
        if (Ukprn > 0)
        {
            url += $"&ukprn={Ukprn}";
        }

        return url;
    }
}
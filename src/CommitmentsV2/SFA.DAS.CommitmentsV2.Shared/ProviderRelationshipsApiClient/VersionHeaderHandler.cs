using System.Net.Http;
using System.Threading;

namespace SFA.DAS.CommitmentsV2.Shared.ProviderRelationshipsApiClient;

public class VersionHeaderHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Add("X-Version", "1.0");
        return base.SendAsync(request, cancellationToken);
    }
}
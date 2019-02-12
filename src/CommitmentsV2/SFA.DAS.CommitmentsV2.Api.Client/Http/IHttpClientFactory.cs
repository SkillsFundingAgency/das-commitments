using System.Net.Http;

namespace SFA.DAS.CommitmentsV2.Api.Client.Http
{
    public interface IHttpClientFactory
    {
        HttpClient CreateHttpClient();
    }
}
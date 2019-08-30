using System.Threading;
using Microsoft.Extensions.Logging;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Types;
using System.Threading.Tasks;

namespace SFA.DAS.ReservationsV2.Api.Client
{
    public class HttpHelper : IHttpHelper
    {
        private readonly IRestHttpClient _client;
        private readonly ILogger<ReservationsApiClient> _logger;

        public HttpHelper(IRestHttpClient client, ILogger<ReservationsApiClient> logger)
        {
            _client = client;
            _logger = logger;
        }

        public Task<T> GetAsync<T>(string url, object data, CancellationToken token)
        {
            return _client.Get<T>(url, data, token);
        }

        public Task<TResponse> PostAsJson<TRequest, TResponse>(string url, TRequest data, CancellationToken token)
        {
            return _client.PostAsJson<TRequest, TResponse>(url, data, token);
        }
    }
}

using Newtonsoft.Json;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Reservations.Api.Types;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Authentication.Extensions.Legacy;

namespace SFA.DAS.Reservations.Api.Client
{
    public class HttpHelper : ApiClientBase, IHttpHelper
    {
        private readonly ILog _log;

        public HttpHelper(
            HttpClient client, 
            ILog log) : base(client)
        {
            _log = log;
        }

        public async Task<T> GetAsync<T>(string url, object data, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            string stringResponse = null;
            try
            {
                stringResponse = await GetAsync(url, data);
                var result = JsonConvert.DeserializeObject<T>(stringResponse);
                return result;
            }
            catch (Exception ex)
            {
                var msg = $"Attempt to call URL {url} " +
                          $"{(data == null ? "(no additional query string parameters provided)" : "using query string built from type" + data.GetType().Name)} " +
                          $"failed to deserialise returned string ({(stringResponse == null ? "which was null" : "length" + stringResponse.Length)}) " +
                          $"into type {typeof(T).Name}.";
                _log.Error(ex, msg);
                throw;
            }
        }

        public async Task<TResponse> PostAsJson<TRequest, TResponse>(string url, TRequest data, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            string stringResponse = null;
            try
            {
                stringResponse = await PostAsync(url, JsonConvert.SerializeObject(data));
                var result = JsonConvert.DeserializeObject<TResponse>(stringResponse);
                return result;
            }
            catch (Exception ex)
            {
                var msg = $"Attempt to post to URL {url} " +
                          $"failed to deserialise returned string ({(stringResponse == null ? "which was null" : "length" + stringResponse.Length)}) " +
                          $"into type {typeof(TResponse).Name}.";
                _log.Error(ex, msg);
                throw;
            }
        }
    }
}

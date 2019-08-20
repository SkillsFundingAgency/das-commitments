using Newtonsoft.Json;
using SFA.DAS.Http;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Reservations.Api.Types;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

        public async Task<T> GetAsync<T>(string url, object data)
        {
        public async Task<BulkCreateReservationsResult> BulkCreateReservations(long accountLegalEntityId, uint count, CancellationToken cancellationToken)
        {
            var bulkReservationsResult = await _reservationHelper.BulkCreateReservations(accountLegalEntityId, count, PostAsync<BulkCreateReservationsResult>);
            _log.Info($"BulkCreateReservations - accountLegalEntity Id:{accountLegalEntityId} count:{count} reservations-created:{bulkReservationsResult?.Reservations?.Length}");
            return bulkReservationsResult;
        }

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

        private async Task<T> PostAsync<T>(string url)
        {
            string stringResponse = null;
            try
            {
                stringResponse = await PostAsync(url, null);
                var result = JsonConvert.DeserializeObject<T>(stringResponse);
                return result;
            }
            catch (Exception ex)
            {
                var msg = $"Attempt to post to URL {url} " +
                          $"failed to deserialise returned string ({(stringResponse == null ? "which was null" : "length" + stringResponse.Length)}) " +
                          $"into type {typeof(T).Name}.";
                _log.Error(ex, msg);
                throw;
            }
        }
    }
}

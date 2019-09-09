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
    public class ReservationsApiClient : ApiClientBase, IReservationsApiClient
    {
        private readonly IReservationHelper _reservationHelper;
        private readonly ILog _log;

        public ReservationsApiClient(
            HttpClient client, 
            IReservationHelper reservationHelper,
            ILog log) : base(client)
        {
            _reservationHelper = reservationHelper;
            _log = log;
        }

        public Task Ping(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public async Task<ReservationValidationResult> ValidateReservation(ValidationReservationMessage request, CancellationToken cancellationToken)
        {
            var validationResult = await _reservationHelper.ValidateReservation(request, GetAsync<ReservationValidationResult>);
            _log.Info($"reservation id:{request.ReservationId} course:{request.CourseCode} start-date:{request.StartDate} validation-result:{validationResult.IsOkay}");
            return validationResult;
        }

        private async Task<T> GetAsync<T>(string url, object data)
        {
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
    }
}

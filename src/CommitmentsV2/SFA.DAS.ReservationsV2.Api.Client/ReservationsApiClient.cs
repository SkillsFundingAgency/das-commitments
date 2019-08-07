using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SFA.DAS.Http;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.ReservationsV2.Api.Client
{
    public class ReservationsApiClient : IReservationsApiClient
    {
        private readonly IRestHttpClient _client;
        private readonly IReservationHelper _reservationHelper;
        private readonly ILogger<ReservationsApiClient> _logger;

        public ReservationsApiClient(IRestHttpClient client, IReservationHelper reservationHelper, ILogger<ReservationsApiClient> logger)
        {
            _client = client;
            _reservationHelper = reservationHelper;
            _logger = logger;
        }

        public async Task<ReservationValidationResult> ValidateReservation(ValidationReservationMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _reservationHelper.ValidateReservation(request, (url, data) =>
                    _client.Get<ReservationValidationResult>(url, data, cancellationToken));

                _logger.LogInformation($"reservation id:{request.ReservationId} course:{request.CourseCode} start-date:{request.StartDate} validation-result:{validationResult.IsOkay}");
                return validationResult;

            }
            catch (Exception ex)
            {
                _logger.LogInformation($"reservation id:{request.ReservationId} course:{request.CourseCode} start-date:{request.StartDate} failed with error {ex.GetType().Name} {ex.Message}");
                throw;
            }
        }
    }
}

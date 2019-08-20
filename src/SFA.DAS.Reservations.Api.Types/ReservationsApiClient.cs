using System;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.Reservations.Api.Types
{
    public class ReservationsApiClient : IReservationsApiClient
    {
        private readonly ReservationsClientApiConfiguration _config;
        private readonly IHttpHelper _httpHelper;

        public ReservationsApiClient(ReservationsClientApiConfiguration config, IHttpHelper httpHelper)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _httpHelper = httpHelper ?? throw new ArgumentNullException(nameof(httpHelper));
        }

        public Task<ReservationValidationResult> ValidateReservation(ReservationValidationMessage request, CancellationToken cancellationToken)
        {
            var url = BuildUrl($"api/reservations/validate/{request.ReservationId}");

            var data = new
            {
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                request.CourseCode
            };

            return _httpHelper.GetAsync<ReservationValidationResult>(url, data);
        }

        public Task<ReservationAllocationStatusResult> GetReservationAllocationStatus(ReservationAllocationStatusMessage request, CancellationToken cancellationToken)
        {
            var url = BuildUrl($"api/accounts/{request.AccountId}/status");

            return _httpHelper.GetAsync<ReservationAllocationStatusResult>(url, null);
        }

        private string BuildUrl(string path)
        {
            var effectiveApiBaseUrl = _config.EffectiveApiBaseUrl.TrimEnd(new[] { '/' });
            path = path.TrimStart(new[] {'/'});

            return $"{effectiveApiBaseUrl}/{path}";
        }
    }
}
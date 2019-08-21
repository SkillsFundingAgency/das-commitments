using System;
using System.Threading.Tasks;
using SFA.DAS.Reservations.Api.Types.Configuration;

namespace SFA.DAS.Reservations.Api.Types
{
    public class ReservationHelper : IReservationHelper
    {
        private readonly ReservationsClientApiConfiguration _config;

        public ReservationHelper(ReservationsClientApiConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public string EffectiveApiBaseUrl => _config.EffectiveApiBaseUrl.TrimEnd(new [] { '/' });


        public Task<ReservationValidationResult> ValidateReservation(ValidationReservationMessage request, Func<string, object, Task<ReservationValidationResult>> call)
        {
            var url = $"{EffectiveApiBaseUrl}/api/reservations/validate/{request.ReservationId}";

            var data = new
            {
                StartDate = request.StartDate.ToString("yyyy-MM-dd"),
                request.CourseCode
            };

            return call(url, data);
        }

        public Task<BulkCreateReservationsResult> BulkCreateReservations(long accountLegalEntityId, BulkCreateReservationsRequest request, Func<string, BulkCreateReservationsRequest, Task<BulkCreateReservationsResult>> call)
        {
            var url = $"{EffectiveApiBaseUrl}/api/accounts/{accountLegalEntityId}/bulk-create";
            return call(url, request);
        }



    }
}
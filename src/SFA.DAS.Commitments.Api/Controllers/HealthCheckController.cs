using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.Commitments.Api.Controllers
{
    public class HealthCheckController : ApiController
    {
        private readonly IReservationsApiClient _reservationClient;

        public HealthCheckController(IReservationsApiClient reservationClient)
        {
            _reservationClient = reservationClient;
        }


        [Route("api/HealthCheck")]
        public IHttpActionResult GetStatus()
        {
            return Ok();
        }

        [Route("api/reservation")]
        public async Task<IHttpActionResult> TestReservationValidation()
        {
            var reservationValidationResult = await _reservationClient.ValidateReservation(new ValidationReservationMessage
            {
                CourseCode = "ABC",
                ReservationId = Guid.NewGuid()
            }, CancellationToken.None);

            return Json(reservationValidationResult);
        }
    }
}
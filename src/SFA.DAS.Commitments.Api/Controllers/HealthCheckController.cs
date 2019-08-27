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
    }
}
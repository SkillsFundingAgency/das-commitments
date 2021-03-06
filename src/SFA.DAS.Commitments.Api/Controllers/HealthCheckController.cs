﻿using System.Web.Http;
using SFA.DAS.Reservations.Api.Types;

namespace SFA.DAS.Commitments.Api.Controllers
{
    public class HealthCheckController : ApiController
    {
        public HealthCheckController()
        {
        }

        [Route("api/HealthCheck")]
        public IHttpActionResult GetStatus()
        {
            return Ok();
        }
    }
}
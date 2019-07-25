using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.Mvc;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [DasAuthorize]
    [Route("api/ping")]
    public class PingController : ControllerBase
    {
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok();
        }
    }
}
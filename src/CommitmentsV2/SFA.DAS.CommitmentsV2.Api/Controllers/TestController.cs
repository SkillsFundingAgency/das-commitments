using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Authorization;
using SFA.DAS.CommitmentsV2.Authentication;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;

        public TestController(ILogger<TestController> logger)
        {
            _logger = logger;
        }
        // GET api/
        //
        //
        // values

        [Authorize(Policies.Provider)]
        [HttpGet("provider")]
        public ActionResult<string> GetProvider()
        {
            _logger.LogInformation("Reached provider endpoint");
            return "Provider Secure Endpoint reached";
        }

        [Authorize(Policies.Employer)]
        [HttpGet("employer")]
        public ActionResult<string> GetEmployer()
        {
            _logger.LogInformation("Reached employer endpoint");
            return "Employer Secure Endpoint reached";
        }

        // GET api/values/5
        [Authorize]
        [HttpGet]
        public ActionResult<string> Get(int id)
        {
            _logger.LogInformation("Reached secure endpoint");
            return "Secure Endpoint reached";
        }

        [HttpGet("role")]
        public ActionResult<string> GetRole([FromServices] IAuthenticationService authenticationService)
        {
            var party = authenticationService.GetUserParty();
            return Ok($"user party : { party.ToString()}");
        }

    }
}

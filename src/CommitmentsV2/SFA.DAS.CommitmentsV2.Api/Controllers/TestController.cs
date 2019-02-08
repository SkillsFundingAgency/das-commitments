using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        [Authorize(Roles = "Provider")]
        [HttpGet("provider")]
        public ActionResult<string> GetProvider()
        {
            return "Provider Secure Endpoint reached";
        }

        [Authorize(Roles = "Employer")]
        [HttpGet("employer")]
        public ActionResult<string> GetEmployer()
        {
            return "Employer Secure Endpoint reached";
        }

        // GET api/values/5
        [Authorize]
        [HttpGet]
        public ActionResult<string> Get(int id)
        {
            return "Secure Endpoint reached";
        }

    }
}

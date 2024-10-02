using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Authorization;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController(ILogger<TestController> logger) : ControllerBase
{

    [Authorize(Policies.Provider)]
    [HttpGet("provider")]
    public ActionResult<string> GetProvider()
    {
        logger.LogInformation("Reached provider endpoint");
        return "Provider Secure Endpoint reached";
    }

    [Authorize(Policies.Employer)]
    [HttpGet("employer")]
    public ActionResult<string> GetEmployer()
    {
        logger.LogInformation("Reached employer endpoint");
        return "Employer Secure Endpoint reached";
    }

    [Authorize]
    [HttpGet]
    public ActionResult<string> Get(int id)
    {
        logger.LogInformation("Reached secure endpoint");
        return "Secure Endpoint reached";
    }

}
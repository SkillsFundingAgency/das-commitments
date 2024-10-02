using SFA.DAS.Authorization.Mvc.Attributes;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Authentication;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[DasAuthorize]
[Route("api/whoami")]
public class WhoAmIController(IAuthenticationService authenticationService) : ControllerBase
{
    [HttpGet]
    public IActionResult WhoAmI()
    {
        var roles = authenticationService.GetAllUserRoles().ToList();
            
        return Ok(new WhoAmIResponse
        {
            Roles = roles
        });
    }
}
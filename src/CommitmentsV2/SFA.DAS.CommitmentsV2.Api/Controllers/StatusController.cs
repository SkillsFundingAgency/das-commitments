using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Types;
using IAuthenticationService = SFA.DAS.CommitmentsV2.Authentication.IAuthenticationService;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class StatusController : ControllerBase
    {
        private readonly ILogger<StatusController> _logger;
        private readonly IAuthenticationService _authenticationService;

        public StatusController(ILogger<StatusController> logger, IAuthenticationService authenticationService)
        {
            _logger = logger;
            _authenticationService = authenticationService;
        }

        [HttpGet("clientrole")]
        public ActionResult<Party> GetClientRole()
        {
            var currentUserRole = _authenticationService.GetUserParty();

            _logger.LogInformation($"The current request is from an app configured as party type {currentUserRole}");

            return currentUserRole;
        }
    }
}

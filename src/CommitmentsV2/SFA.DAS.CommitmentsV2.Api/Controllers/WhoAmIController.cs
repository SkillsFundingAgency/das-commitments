using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Authorization.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Authentication;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [DasAuthorize]
    [Route("api/whoami")]
    public class WhoAmIController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;

        public WhoAmIController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpGet]
        public IActionResult WhoAmI()
        {
            var roles = new List<string>();
            
            if (_authenticationService.IsUserInRole(Role.Employer))
            {
                roles.Add(Role.Employer);
            }
            
            if (_authenticationService.IsUserInRole(Role.Provider))
            {
                roles.Add(Role.Provider);
            }

            if (roles.Count == 0)
            {
                throw new InvalidOperationException("Client is authenticated with an unknown role");
            }

            if (roles.Count > 1)
            {
                throw new InvalidOperationException("Client is authenticated with multiple roles");
            }
            
            return Ok(new WhoAmIResponse
            {
                Role = roles.Single()
            });
        }
    }
}
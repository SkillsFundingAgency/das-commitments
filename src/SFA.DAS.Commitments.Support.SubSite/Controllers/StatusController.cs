using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Commitments.Support.SubSite.GlobalConstants;
using SFA.DAS.Commitments.Support.SubSite.Models;

namespace SFA.DAS.Commitments.Support.SubSite.Controllers
{
    [ApiController]
    [Route("api/status")]
    public class StatusController : Controller
    {
        [AllowAnonymous]
        public IActionResult Get()
        {
            return Ok(new ServiceStatusViewModel
            {
                ServiceName = ApplicationConstants.ServiceName,
                ServiceVersion = "Unknown",
                ServiceTime = DateTimeOffset.UtcNow,
                Request = "Unknown"
            });
        }
    }
}
using System;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.Commitments.Support.SubSite.GlobalConstants;
using SFA.DAS.Commitments.Support.SubSite.Models;
using Microsoft.AspNetCore.Http.Extensions;

namespace SFA.DAS.Commitments.Support.SubSite.Controllers
{
    [ApiController]
    [Route("api/status")]
    public class StatusController : Controller
    {
        [AllowAnonymous]
        public ActionResult Get()
        {
            return Ok(new ServiceStatusViewModel
            {
                ServiceName = ApplicationConstants.ServiceName,
                ServiceVersion = "Unknown",
                ServiceTime = DateTimeOffset.UtcNow,
                Request = AddRequestContext()
            });
        }

        private string AddRequestContext()
        {
            try
            {
                return $" {HttpContext.Request.Method}: {UriHelper.GetDisplayUrl(Request)}";
            }
            catch
            {
                return "Unknown";
            }
        }
    }
}
using System;
using System.Reflection;
using System.Web;
using System.Web.Http;
using SFA.DAS.Commitments.Support.SubSite.GlobalConstants;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Support.Shared;

namespace SFA.DAS.Commitments.Support.SubSite.Controllers
{
    [RoutePrefix("api/status")]
    public class StatusController : ApiController
    {
        [AllowAnonymous]
        public IHttpActionResult Get()
        {
            return Ok(new ServiceStatusViewModel
            {
                ServiceName = ApplicationConstants.ServiceName,
                ServiceVersion = AddServiceVersion(),
                ServiceTime = DateTimeOffset.UtcNow,
                Request = AddRequestContext()
            });
        }

        private string AddServiceVersion()
        {
            try
            {
                return Assembly.GetExecutingAssembly().Version();
            }
            catch
            {
                return "Unknown";
            }
        }

        private string AddRequestContext()
        {
            try
            {
                return $" {HttpContext.Current.Request.HttpMethod}: {HttpContext.Current.Request.RawUrl}";
            }
            catch
            {
                return "Unknown";
            }
        }

    }
}
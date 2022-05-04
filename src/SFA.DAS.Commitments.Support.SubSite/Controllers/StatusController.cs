using System;
using System.Reflection;
using System.Web;
using System.Web.Http;
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
        //[AllowAnonymous]
        //public ActionResult Get()
        //{
        //    return Ok(new ServiceStatusViewModel
        //    {
        //        ServiceName = ApplicationConstants.ServiceName,
        //        ServiceVersion = AddServiceVersion(),
        //        ServiceTime = DateTimeOffset.UtcNow,
        //        Request = AddRequestContext()
        //    });
        //}

        //private string AddServiceVersion()
        //{
        //    try
        //    {
        //        return Assembly.GetExecutingAssembly().Version();
        //    }
        //    catch
        //    {
        //        return "Unknown";
        //    }
        //}

        //private string AddRequestContext()
        //{
        //    try
        //    {
        //        return $" {HttpContext.Current.Request.HttpMethod}: {HttpContext.Current.Request.RawUrl}";
        //    }
        //    catch
        //    {
        //        return "Unknown";
        //    }
        //}
    }
}
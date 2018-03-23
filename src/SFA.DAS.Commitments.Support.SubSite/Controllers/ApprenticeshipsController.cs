using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Support.Shared;

namespace SFA.DAS.Commitments.Support.SubSite.Controllers
{
    public class ApprenticeshipsController : Controller
    {

        public ApprenticeshipsController()
        {
            
        }


        public ActionResult FindApprenticeship()
        {
            return View();

        }


    }
}
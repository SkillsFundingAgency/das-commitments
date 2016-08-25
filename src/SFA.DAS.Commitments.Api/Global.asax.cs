using System.Web.Http;
using System.Web.Security;

namespace SFA.DAS.Commitments.Api
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}

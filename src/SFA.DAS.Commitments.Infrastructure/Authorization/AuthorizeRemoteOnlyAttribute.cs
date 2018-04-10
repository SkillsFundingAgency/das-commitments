using System.Web.Http;
using System.Web.Http.Controllers;

namespace SFA.DAS.Commitments.Infrastructure.Authorization
{
    public class AuthorizeRemoteOnlyAttribute : AuthorizeAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            if (actionContext.Request.RequestUri.IsLoopback)
                return;

            base.OnAuthorization(actionContext);
        }
    }
}

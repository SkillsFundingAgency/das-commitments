using System.Web.Http.Filters;

namespace SFA.DAS.Commitments.Api
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(HttpFilterCollection filters)
        {
            filters.Add(new NLog.Logger.Web.RequestIdHttpActionFilter());
            filters.Add(new NLog.Logger.Web.SessionIdHttpActionFilter());
        }
    }
}
using System.Web.Http.Filters;

namespace SFA.DAS.Commitments.Api
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(HttpFilterCollection filters)
        {
            // ToDo: Add action filter from NLog
            // Request and session
            //filters.Add(new HeaderTestActionFilter());
        }
    }
}
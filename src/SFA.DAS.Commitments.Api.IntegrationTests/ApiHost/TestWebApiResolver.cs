using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;
using SFA.DAS.Commitments.Api.Controllers;

namespace SFA.DAS.Commitments.Api.IntegrationTests.ApiHost
{
    public class TestWebApiResolver : DefaultAssembliesResolver
    {
        public override ICollection<Assembly> GetAssemblies()
        {
            return new List<Assembly> { typeof(EmployerController).Assembly };
        }
    }
}

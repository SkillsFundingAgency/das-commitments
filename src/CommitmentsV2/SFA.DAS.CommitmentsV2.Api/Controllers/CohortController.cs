using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/[controller]")]
    public class CohortController : Controller
    {
        public CreateCohortResponse Post([FromBody]CreateCohortRequest request)
        {
            return new CreateCohortResponse();
        }
    }
}
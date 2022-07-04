using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/overlapping-training-dates")]
    public class OverlappingTrainingDateController : ControllerBase
    {
        [HttpPost]
        [Route("create")]
        public IActionResult CreateOverlappingTrainingDate([FromBody]CreateOverlappingTrainingDateRequest request)
        {
            
        }
    }
}

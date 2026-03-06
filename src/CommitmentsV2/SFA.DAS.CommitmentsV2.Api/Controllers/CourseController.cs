using Microsoft.AspNetCore.Authorization;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllCourses;

namespace SFA.DAS.CommitmentsV2.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/courses")]
public class CourseController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await mediator.Send(new GetAllCoursesQuery());

        return Ok(new GetAllCoursesResponse
        {
            Courses = result.Courses
        });
    }
}
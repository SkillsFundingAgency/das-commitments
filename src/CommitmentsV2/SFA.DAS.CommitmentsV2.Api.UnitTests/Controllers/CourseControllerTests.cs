using Microsoft.AspNetCore.Mvc;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAllCourses;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers;

[TestFixture]
[Parallelizable]
public class CourseControllerTests
{
    private Mock<IMediator> _mediator;
    private CourseController _controller;

    public CourseControllerTests()
    {
        _mediator = new Mock<IMediator>();
        _controller = new CourseController(_mediator.Object);
    }

    [Test]
    public async Task GetAllCourses_Then_ReturnValidResponse()
    {
        // Arrange

        _mediator.Setup(m => m.Send(It.IsAny<GetAllCoursesQuery>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetAllCoursesQueryResult()
            {
                Courses = new List<Course>()
                {
                    new Course()
                    {
                         LarsCode = "101",
                         Title = "Course 101",
                         Level ="7",
                         LearningType = "Apprenticeship",
                         MaxFunding = 15000,
                         EffectiveFrom = DateTime.UtcNow.AddMonths(-1),
                         EffectiveTo = null
                    }
                }
            });

        // Act

        var result = await _controller.GetAll();

        // Assert

        result.Should().NotBeNull();
        var jsonResult = result as OkObjectResult;
        var getAllCoursesResponse = jsonResult?.Value as GetAllCoursesResponse;
        getAllCoursesResponse.Courses.Should().HaveCount(1);
    }
}

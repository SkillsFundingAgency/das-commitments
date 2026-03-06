using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Extensions;
using Course = SFA.DAS.CommitmentsV2.Api.Types.Responses.Course;

namespace SFA.DAS.CommitmentsV2.Application.Queries.GetAllCourses;

public class GetAllCoursesQueryHandler(ProviderCommitmentsDbContext dbContext) : IRequestHandler<GetAllCoursesQuery, GetAllCoursesQueryResult>
{
    public async Task<GetAllCoursesQueryResult> Handle(GetAllCoursesQuery request, CancellationToken cancellationToken)
    {
        var courses = dbContext.Courses.ToList();

        return new GetAllCoursesQueryResult()
        {
            Courses = courses.Select(courseItem => new Course
            {
                LarsCode = courseItem.LarsCode,
                Title = courseItem.Title,
                Level = courseItem.Level,
                LearningType = courseItem.LearningType.GetEnumDescription(),
                MaxFunding = courseItem.MaxFunding,
                EffectiveFrom = courseItem.EffectiveFrom,
                EffectiveTo = courseItem.EffectiveTo
            }).ToList()
        };
    }
}
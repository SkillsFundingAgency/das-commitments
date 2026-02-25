using MoreLinq.Extensions;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;

namespace SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;

public class ImportCoursesJob(ILogger<ImportCoursesJob> logger,
    IApprovalsOuterApiClient apiClient,
    IProviderCommitmentsDbContext providerContext,
    CommitmentsV2Configuration configuration)
{
    public async Task Import([TimerTrigger("45 10 1 * * *", RunOnStartup = false)] TimerInfo timer)
    {
        logger.LogInformation("ImportCoursesJob - Started");

        var response = await apiClient.Get<CourseResponse>(new GetCoursesRequest());

        var courses = response.Courses.ToList();
        await ProcessCourses(courses);

        logger.LogInformation("ImportCoursesJob - Finished");
    }

    private async Task ProcessCourses(IEnumerable<CourseSummary> courses)
    {
        logger.LogTrace("ImportCoursesJob: {courses} records retrived from coruses api", courses.Count());

        var batches = courses.Batch(1000).Select(b => b.ToDataTable(
            p => p.LarsCode,
            p => p.Title,
            p => p.Level,
            p => p.LearningTypeByte,
            p => p.CurrentFundingCap,
            p => p.EffectiveFrom,
            p => p.EffectiveTo
        ));

        foreach (var batch in batches)
        {
            if (!configuration.IgnoreShortCourses)
            {
                await ImportCourses(providerContext, batch);
            }
            else
            {
                logger.LogInformation("IgnoreShortCourses is enabled - courses not saved");
            }
        }
    }

    private static Task ImportCourses(IProviderCommitmentsDbContext db, DataTable coursesDataTable)
    {
        var courses = new SqlParameter("Courses", SqlDbType.Structured)
        {
            TypeName = "Courses",
            Value = coursesDataTable
        };

        return db.ExecuteSqlCommandAsync("EXEC ImportCourses @courses", courses);
    }
}
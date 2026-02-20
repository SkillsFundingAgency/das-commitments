using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Configuration;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.ScheduledJobs;

public class ImportCoursesJobTests
{
    [Test, MoqAutoData]
    public async Task Then_The_Courses_Are_Imported_From_The_Client(
        CourseResponse apiResponse,
        CourseSummary course1,
        CourseSummary course2,
        [Frozen] Mock<IApprovalsOuterApiClient> apiClient,
        [Frozen] Mock<IProviderCommitmentsDbContext> context,
        [Frozen] CommitmentsV2Configuration configuration,
        ImportCoursesJob importCoursesJob
        )
    {
        //Arrange
        configuration.IgnoreShortCourses = false;
        apiResponse.Courses = new List<CourseSummary> { course1, course2 };
        apiClient.Setup(x => x.Get<CourseResponse>(It.IsAny<GetCoursesRequest>())).ReturnsAsync(apiResponse);
        var importedCourses = new List<CourseSummary>();
        context.Setup(d => d.ExecuteSqlCommandAsync("EXEC ImportCourses @courses", It.IsAny<SqlParameter>()))
            .Returns(Task.CompletedTask)
            .Callback<string, object[]>((s, p) =>
            {
                var sqlParameter = (SqlParameter)p[0];
                var dataTable = (DataTable)sqlParameter.Value;

                importedCourses.AddRange(dataTable.AsEnumerable().Select(r => new CourseSummary
                {
                    LarsCode = (string)r[0],
                    Title = (string)r[1],
                    Level = (int)r[2],
                    LearningType = (string)r[3],
                    CurrentFundingCap = (int)r[4],
                    EffectiveFrom = (DateTime?)r[5],
                    EffectiveTo = (DateTime?)r[6]
                }));
            });

        //Act
        await importCoursesJob.Import(null);

        //Assert
        importedCourses.Should().BeEquivalentTo(new object[] {
        new {
            course1.LarsCode,
            course1.Title,
            course1.Level,
            course1.LearningType,
            course1.CurrentFundingCap,
            course1.EffectiveFrom,
            course1.EffectiveTo
        },
        new {
            course2.LarsCode,
            course2.Title,
            course2.Level,
            course2.LearningType,
            course2.CurrentFundingCap,
            course2.EffectiveFrom,
            course2.EffectiveTo
        }});
    }

    [Test, MoqAutoData]
    public async Task Then_The_Courses_Are_Not_Saved_When_Config_Ignore_Short_Courses_Is_Enabled(
        CourseResponse apiResponse,
        CourseSummary course1,
        CourseSummary course2,
        [Frozen] Mock<IApprovalsOuterApiClient> apiClient,
        [Frozen] Mock<IProviderCommitmentsDbContext> context,
        [Frozen] CommitmentsV2Configuration configuration,
        ImportCoursesJob importCoursesJob
        )
    {
        //Arrange
        configuration.IgnoreShortCourses = true;

        apiResponse.Courses = new List<CourseSummary> { course1, course2 };
        apiClient.Setup(x => x.Get<CourseResponse>(It.IsAny<GetCoursesRequest>())).ReturnsAsync(apiResponse);
        var importedCourses = new List<CourseSummary>();
        context.Setup(d => d.ExecuteSqlCommandAsync("EXEC ImportCourses @courses", It.IsAny<SqlParameter>()))
            .Returns(Task.CompletedTask)
            .Callback<string, object[]>((s, p) =>
            {
                var sqlParameter = (SqlParameter)p[0];
                var dataTable = (DataTable)sqlParameter.Value;

                importedCourses.AddRange(dataTable.AsEnumerable().Select(r => new CourseSummary
                {
                    LarsCode = (string)r[0],
                    Title = (string)r[1],
                    Level = (int)r[2],
                    LearningType = (string)r[3],
                    CurrentFundingCap = (int)r[4],
                    EffectiveFrom = (DateTime?)r[5],
                    EffectiveTo = (DateTime?)r[6]
                }));
            });

        //Act
        await importCoursesJob.Import(null);

        //Assert
        importedCourses.Should().BeEmpty();
    }
}
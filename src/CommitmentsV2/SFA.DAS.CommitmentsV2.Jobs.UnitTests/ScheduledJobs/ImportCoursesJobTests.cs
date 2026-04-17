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
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.Testing.AutoFixture;
using LearningType = SFA.DAS.Common.Domain.Types.LearningType;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.ScheduledJobs;

public class ImportCoursesJobTests
{
    [Test, MoqAutoData]
    public async Task Then_Importing_Courses_Maps_All_Client_Fields_To_The_Table_Valued_Parameter(
        CourseResponse apiResponse,
        CourseSummary course1,
        CourseSummary course2,
        [Frozen] Mock<IApprovalsOuterApiClient> apiClient,
        [Frozen] Mock<IProviderCommitmentsDbContext> context,
        ImportCoursesJob importCoursesJob
        )
    {
        //Arrange
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
                    LearningType = (LearningType)r[3],
                    MaxFunding = (int)r[4],
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
            course1.LearningTypeByte,
            course1.MaxFunding,
            course1.EffectiveFrom,
            course1.EffectiveTo
        },
        new {
            course2.LarsCode,
            course2.Title,
            course2.Level,
            course2.LearningTypeByte,
            course2.MaxFunding,
            course2.EffectiveFrom,
            course2.EffectiveTo
        }});
    }

    [Test, MoqAutoData]
    public async Task Then_Importing_Courses_Persists_All_Courses_Returned_By_The_Client(
        CourseResponse apiResponse,
        CourseSummary course1,
        CourseSummary course2,
        [Frozen] Mock<IApprovalsOuterApiClient> apiClient,
        [Frozen] Mock<IProviderCommitmentsDbContext> context,
        ImportCoursesJob importCoursesJob
        )
    {
        //Arrange
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
                    LearningType = (LearningType)r[3],
                    MaxFunding = (int)r[4],
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
            course1.LearningTypeByte,
            course1.MaxFunding,
            course1.EffectiveFrom,
            course1.EffectiveTo
        },
        new {
            course2.LarsCode,
            course2.Title,
            course2.Level,
            course2.LearningTypeByte,
            course2.MaxFunding,
            course2.EffectiveFrom,
            course2.EffectiveTo
        }});
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture.NUnit3;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.CommitmentsV2.Models.Api;
using SFA.DAS.CommitmentsV2.Models.Api.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.ScheduledJobs
{
    public class ImportFrameworksJobTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Frameworks_Are_Imported_From_The_Client(
            FrameworkResponse apiResponse,
            [Frozen] Mock<IApiClient> apiClient,
            [Frozen] Mock<ICommitmentsDbContext> context,
            ImportFrameworksJob importFrameworksJob
            )
        {
            //Arrange
            apiClient.Setup(x => x.Get<FrameworkResponse>(It.IsAny<GetFrameworksRequest>())).ReturnsAsync(apiResponse);
            var importedStandards = new List<FrameworkSummary>(); 
            context.Setup(d => d.ExecuteSqlCommandAsync("EXEC ImportFrameworks @frameworks", It.IsAny<SqlParameter>()))
                .Returns(Task.CompletedTask)
                .Callback<string, object[]>((s, p) =>
                {
                    var sqlParameter = (SqlParameter)p[0];
                    var dataTable = (DataTable)sqlParameter.Value;

                    importedStandards.AddRange(dataTable.AsEnumerable().Select(r => new FrameworkSummary()
                    {
                        Id = (string)r[0],
                        FrameworkCode = (int)r[1],
                        FrameworkName = (string)r[2],
                        Level = (int)r[3],
                        PathwayCode = (int)r[4],
                        PathwayName = (string)r[5],
                        ProgrammeType = (int)r[6],
                        Title = (string)r[7],
                        Duration = (int)r[8],
                        MaxFunding = (int)r[9],
                        EffectiveFrom = (DateTime?)r[10],
                        EffectiveTo = (DateTime?)r[11],
                    }));
                });
            
            //Act
            await importFrameworksJob.Import(null);
            
            //Assert
            importedStandards.Should().BeEquivalentTo(apiResponse.Frameworks, options => options.Excluding(c=>c.FundingPeriods));
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_StandardsFunding_Items_Are_Imported_From_The_Client(
            FrameworkResponse apiResponse,
            [Frozen] Mock<IApiClient> apiClient,
            [Frozen] Mock<ICommitmentsDbContext> context,
            ImportFrameworksJob importFrameworksJob
        )
        {
            //Arrange
            apiClient.Setup(x => x.Get<FrameworkResponse>(It.IsAny<GetFrameworksRequest>())).ReturnsAsync(apiResponse);
            var importedStandardFunding = new List<FundingPeriodItem>(); 
            context.Setup(d => d.ExecuteSqlCommandAsync("EXEC ImportFrameworksFunding @frameworksFunding", It.IsAny<SqlParameter>()))
                .Returns(Task.CompletedTask)
                .Callback<string, object[]>((s, p) =>
                {
                    var sqlParameter = (SqlParameter)p[0];
                    var dataTable = (DataTable)sqlParameter.Value;

                    importedStandardFunding.AddRange(dataTable.AsEnumerable().Select(r => new FundingPeriodItem
                    {
                        FrameworkId = (string)r[0],
                        FundingCap = (int)r[1],
                        EffectiveFrom = (DateTime?)r[2],
                        EffectiveTo = (DateTime?)r[3],
                    }));
                });
            
            //Act
            await importFrameworksJob.Import(null);
            
            //Assert
            var expectedItems = new List<FundingPeriodItem>();
            foreach (var responseStandard in apiResponse.Frameworks)
            {
                var frameworkId = responseStandard.Id;
                expectedItems.AddRange(responseStandard.FundingPeriods.Select(fundingPeriod => new FundingPeriodItem
                {
                    FrameworkId = frameworkId, 
                    EffectiveFrom = fundingPeriod.EffectiveFrom, 
                    EffectiveTo = fundingPeriod.EffectiveTo, 
                    FundingCap = fundingPeriod.FundingCap
                }));
            }
            importedStandardFunding.Should().BeEquivalentTo(expectedItems);
        }
    }
}
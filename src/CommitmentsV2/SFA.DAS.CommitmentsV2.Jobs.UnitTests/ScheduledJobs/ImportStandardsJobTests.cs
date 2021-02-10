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
    public class ImportStandardsJobTests
    {
        [Test, MoqAutoData]
        public async Task Then_The_Standards_Are_Imported_From_The_Client(
            StandardResponse apiResponse,
            [Frozen] Mock<IApiClient> apiClient,
            [Frozen] Mock<IProviderCommitmentsDbContext> context,
            ImportStandardsJob importStandardsJob
            )
        {
            //Arrange
            apiClient.Setup(x => x.Get<StandardResponse>(It.IsAny<GetStandardsRequest>())).ReturnsAsync(apiResponse);
            var importedStandards = new List<StandardSummary>(); 
            context.Setup(d => d.ExecuteSqlCommandAsync("EXEC ImportStandards @standards", It.IsAny<SqlParameter>()))
                .Returns(Task.CompletedTask)
                .Callback<string, object[]>((s, p) =>
                {
                    var sqlParameter = (SqlParameter)p[0];
                    var dataTable = (DataTable)sqlParameter.Value;

                    importedStandards.AddRange(dataTable.AsEnumerable().Select(r => new StandardSummary
                    {
                        Id = (int)r[0],
                        Title = (string)r[1],
                        Level = (int)r[2],
                        Duration = (int)r[3],
                        CurrentFundingCap = (int)r[4],
                        EffectiveFrom = (DateTime?)r[5],
                        LastDateForNewStarts = (DateTime?)r[6],
                    }));
                });
            
            //Act
            await importStandardsJob.Import(null);
            
            //Assert
            importedStandards.Should().BeEquivalentTo(apiResponse.Standards, options => options.Excluding(c=>c.FundingPeriods));
        }
        
        [Test, MoqAutoData]
        public async Task Then_The_StandardsFunding_Items_Are_Imported_From_The_Client(
            StandardResponse apiResponse,
            [Frozen] Mock<IApiClient> apiClient,
            [Frozen] Mock<IProviderCommitmentsDbContext> context,
            ImportStandardsJob importStandardsJob
        )
        {
            //Arrange
            apiClient.Setup(x => x.Get<StandardResponse>(It.IsAny<GetStandardsRequest>())).ReturnsAsync(apiResponse);
            var importedStandardFunding = new List<FundingPeriodItem>(); 
            context.Setup(d => d.ExecuteSqlCommandAsync("EXEC ImportStandardsFunding @standardsFunding", It.IsAny<SqlParameter>()))
                .Returns(Task.CompletedTask)
                .Callback<string, object[]>((s, p) =>
                {
                    var sqlParameter = (SqlParameter)p[0];
                    var dataTable = (DataTable)sqlParameter.Value;

                    importedStandardFunding.AddRange(dataTable.AsEnumerable().Select(r => new FundingPeriodItem
                    {
                        StandardId = (int)r[0],
                        FundingCap = (int)r[1],
                        EffectiveFrom = (DateTime?)r[2],
                        EffectiveTo = (DateTime?)r[3],
                    }));
                });
            
            //Act
            await importStandardsJob.Import(null);
            
            //Assert
            var expectedItems = new List<FundingPeriodItem>();
            foreach (var responseStandard in apiResponse.Standards)
            {
                var standardId = responseStandard.Id;
                expectedItems.AddRange(responseStandard.FundingPeriods.Select(fundingPeriod => new FundingPeriodItem
                {
                    StandardId = standardId, 
                    EffectiveFrom = fundingPeriod.EffectiveFrom, 
                    EffectiveTo = fundingPeriod.EffectiveTo, 
                    FundingCap = fundingPeriod.FundingCap
                }));
            }
            importedStandardFunding.Should().BeEquivalentTo(expectedItems);
        }
    }
}
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
            apiResponse.Standards.ToList().ForEach(s => s.Status = "Approved for delivery");
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
                        StandardUId = (string)r[0],
                        LarsCode = (int)r[1],
                        IFateReferenceNumber = (string)r[2],
                        Version = (string)r[3],
                        Title = (string)r[4],
                        Level = (int)r[5],
                        Duration = (int)r[6],
                        CurrentFundingCap = (int)r[7],
                        EffectiveFrom = (DateTime?)r[8],
                        LastDateForNewStarts = (DateTime?)r[9],
                        VersionMajor = (int)r[10],
                        VersionMinor = (int)r[11],
                        StandardPageUrl = (string)r[12],
                        Status = (string)r[13],
                        IsLatestVersion = (bool)r[14]
                    }));
                });

            //Act
            await importStandardsJob.Import(null);

            //Assert
            var firstStandard = apiResponse.Standards.First();
            var secondStandard = apiResponse.Standards.ElementAt(1);
            var thirdStandard = apiResponse.Standards.Last();

            importedStandards.Should().BeEquivalentTo(new object[] {
            new {
                firstStandard.StandardUId,
                firstStandard.LarsCode,
                firstStandard.IFateReferenceNumber,
                firstStandard.Version,
                firstStandard.Title,
                firstStandard.Level,
                firstStandard.Duration,
                firstStandard.CurrentFundingCap,
                firstStandard.VersionMajor,
                firstStandard.VersionMinor,
                firstStandard.StandardPageUrl,
                firstStandard.Status,
                firstStandard.IsLatestVersion,
                EffectiveFrom = firstStandard.VersionDetail.EarliestStartDate,
                LastDateForNewStarts = firstStandard.VersionDetail.LatestStartDate
            },
            new {
                secondStandard.StandardUId,
                secondStandard.LarsCode,
                secondStandard.IFateReferenceNumber,
                secondStandard.Version,
                secondStandard.Title,
                secondStandard.Level,
                secondStandard.Duration,
                secondStandard.CurrentFundingCap,
                secondStandard.VersionMajor,
                secondStandard.VersionMinor,
                secondStandard.StandardPageUrl,
                secondStandard.Status,
                secondStandard.IsLatestVersion,
                EffectiveFrom = secondStandard.VersionDetail.EarliestStartDate,
                LastDateForNewStarts = secondStandard.VersionDetail.LatestStartDate
            },
            new {
                thirdStandard.StandardUId,
                thirdStandard.LarsCode,
                thirdStandard.IFateReferenceNumber,
                thirdStandard.Version,
                thirdStandard.Title,
                thirdStandard.Level,
                thirdStandard.Duration,
                thirdStandard.CurrentFundingCap,
                thirdStandard.VersionMajor,
                thirdStandard.VersionMinor,
                thirdStandard.StandardPageUrl,
                thirdStandard.Status,
                thirdStandard.IsLatestVersion,
                EffectiveFrom = thirdStandard.VersionDetail.EarliestStartDate,
                LastDateForNewStarts = thirdStandard.VersionDetail.LatestStartDate
            }});
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
            apiResponse.Standards.ToList().ForEach(s => s.Status = "Approved for delivery");
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
                var standardId = responseStandard.LarsCode;
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

        [Test, MoqAutoData]
        public async Task Then_The_StandardOptions_Items_Are_Imported_From_The_Client(
            StandardResponse apiResponse,
            [Frozen] Mock<IApiClient> apiClient,
            [Frozen] Mock<IProviderCommitmentsDbContext> context,
            ImportStandardsJob importStandardsJob
)
        {
            //Arrange
            apiResponse.Standards.ToList().ForEach(s => s.Status = "Approved for delivery");
            apiClient.Setup(x => x.Get<StandardResponse>(It.IsAny<GetStandardsRequest>())).ReturnsAsync(apiResponse);
            var importedStandardOptions = new List<object>();
            context.Setup(d => d.ExecuteSqlCommandAsync("EXEC ImportStandardOptions @standardOptions", It.IsAny<SqlParameter>()))
                .Returns(Task.CompletedTask)
                .Callback<string, object[]>((s, p) =>
                {
                    var sqlParameter = (SqlParameter)p[0];
                    var dataTable = (DataTable)sqlParameter.Value;

                    importedStandardOptions.AddRange(dataTable.AsEnumerable().Select(r => new
                    {
                        StandardUId = (string)r[0],
                        Option = (string)r[1]
                    }));
                });

            //Act
            await importStandardsJob.Import(null);

            //Assert
            var expectedItems = new List<object>();
            foreach (var responseStandard in apiResponse.Standards)
            {
                var standardId = responseStandard.LarsCode;
                expectedItems.AddRange(responseStandard.Options.Select(o => new { responseStandard.StandardUId, Option = o }));
            }
            importedStandardOptions.Should().BeEquivalentTo(expectedItems);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Jobs.ScheduledJobs;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.Testing;

namespace SFA.DAS.CommitmentsV2.Jobs.UnitTests.ScheduledJobs
{
    [TestFixture]
    [Parallelizable]
    public class ImportProvidersJobTests : FluentTest<ImportProvidersJobTestsFixture>
    {
        [Test]
        public Task ImportProvidersJob_WhenRunningImportProvidersJob_ThenShouldImportProvidersInBatchesOf1000()
        {
            return TestAsync(f => f.SetProviders(1500), f => f.Run(), f => f.Db.Verify(d => d.ExecuteSqlCommandAsync(
                "EXEC ImportProviders @providers, @now",
                It.Is<SqlParameter>(p => p.ParameterName == "providers"),
            It.Is<SqlParameter>(p => p.ParameterName == "now" && (DateTime)p.Value >= f.Now)), Times.Exactly(2)));
        }

        [Test]
        public Task ImportProvidersJob_WhenRunningImportProvidersJob_ThenShouldImportProviders()
        {
            return TestAsync(f => f.SetProviders(1500), f => f.Run(), f => f.ImportedProviders.Should().BeEquivalentTo(f.Providers));
        }
    }

    public class ImportProvidersJobTestsFixture
    {
        public DateTime Now { get; set; }
        public Mock<ProviderCommitmentsDbContext> Db { get; set; }
        public ImportProvidersJobs ImportProvidersJob { get; set; }
        public Mock<IApprovalsOuterApiClient> ProviderApiClient { get; set; }
        public List<ProviderSummary> Providers { get; set; }
        public List<ProviderSummary> ImportedProviders { get; set; }

        public ImportProvidersJobTestsFixture()
        {
            Now = DateTime.UtcNow;
            Db = new Mock<ProviderCommitmentsDbContext>();
            ProviderApiClient = new Mock<IApprovalsOuterApiClient>();
            ImportedProviders = new List<ProviderSummary>();

            Db.Setup(d => d.ExecuteSqlCommandAsync(It.IsAny<string>(), It.IsAny<SqlParameter>(), It.IsAny<SqlParameter>()))
                .Returns(Task.CompletedTask)
                .Callback<string, object[]>((s, p) =>
                {
                    var sqlParameter = (SqlParameter)p[0];
                    var dataTable = (DataTable)sqlParameter.Value;

                    ImportedProviders.AddRange(dataTable.AsEnumerable().Select(r => new ProviderSummary
                    {
                        Ukprn = (int)r[0],
                        Name = (string)r[1]
                    }));
                });

            ImportProvidersJob = new ImportProvidersJobs((new Mock<ILogger<ImportProvidersJobs>>()).Object, ProviderApiClient.Object, new Lazy<ProviderCommitmentsDbContext>(() => Db.Object));
        }

        public Task Run()
        {
            return ImportProvidersJob.ImportProvidersJob(null);
        }

        public ImportProvidersJobTestsFixture SetProviders(int count)
        {
            Providers = Enumerable.Range(1, count)
                .Select(i => new ProviderSummary
                {
                    Ukprn = i,
                    Name = i.ToString()
                })
                .ToList();

            ProviderApiClient.Setup(c => c.Get<ProviderResponse>(It.IsAny<GetProvidersRequest>())).ReturnsAsync(new ProviderResponse{Providers = Providers});

            return this;
        }
    }
}
using Dapper;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Data;

namespace SFA.DAS.Commitments.Database.IntegrationTests.StoredProcedures.GetLearnersBatch
{
    [TestFixture]
    public class WhenFilterMatchesNoData : GetLearnersBatchIntegrationTestBase
    {
        [Test]
        public void Then_No_Learners_Returned()
        {
            // Arrange.

            var parameters = new DynamicParameters();
            parameters.Add("sinceTime", new DateTime(2025, 05, 01), DbType.DateTime);
            parameters.Add("batchSize", DBNull.Value, DbType.Int32);
            parameters.Add("batchNumber", DBNull.Value, DbType.Int32);
            parameters.Add("totalNumberOfBatches", dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Act.

            var results = TestDatabase.Query<object>("GetLearnersBatch", parameters, CommandType.StoredProcedure);

            // Assert.

            results.Should().NotBeNull();
            results.Should().HaveCount(0);
            parameters.Get<int>("totalNumberOfBatches").Should().Be(0);
        }
    }
}

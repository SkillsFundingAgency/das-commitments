using Dapper;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Data;

namespace SFA.DAS.Commitments.Database.IntegrationTests.StoredProcedures.GetLearnersBatch
{
    [TestFixture]
    public class WhenBatchRequested : GetLearnersBatchIntegrationTestBase
    {
        [TestCase(10, 1, 2, 10)]
        [TestCase(10, 2, 2, 7)]
        public void Then_Valid_Learners_Batch_Returned(int batchSize, int batchNumber, int totalNumberOfBatches, int expectedLearnersCount)
        {
            // Arrange.

            var parameters = new DynamicParameters();
            parameters.Add("sinceTime", DBNull.Value, DbType.DateTime);
            parameters.Add("batchSize", batchSize, DbType.Int32);
            parameters.Add("batchNumber", batchNumber, DbType.Int32);
            parameters.Add("totalNumberOfBatches", dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Act.

            var results = TestDatabase.Query<object>("GetLearnersBatch", parameters, CommandType.StoredProcedure);

            // Assert.

            results.Should().NotBeNull();
            results.Should().HaveCount(expectedLearnersCount);
            parameters.Get<int>("totalNumberOfBatches").Should().Be(totalNumberOfBatches);
        }
    }
}

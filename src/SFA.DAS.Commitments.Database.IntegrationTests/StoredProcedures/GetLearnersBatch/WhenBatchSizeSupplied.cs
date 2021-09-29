using Dapper;
using FluentAssertions;
using NUnit.Framework;
using System;
using System.Data;

namespace SFA.DAS.Commitments.Database.IntegrationTests.StoredProcedures.GetLearnersBatch
{
    [TestFixture]
    public class WhenBatchSizeSupplied : GetLearnersBatchIntegrationTestBase
    {
        [TestCase(1, 17)]
        [TestCase(2, 9)]
        [TestCase(3, 6)]
        [TestCase(4, 5)]
        [TestCase(5, 4)]
        [TestCase(6, 3)]
        [TestCase(7, 3)]
        [TestCase(8, 3)]
        [TestCase(9, 2)]
        [TestCase(16, 2)]
        [TestCase(17, 1)]
        public void Then_TotalNumberOfBatchesCalculated(int batchSize, int expectedTotalNumberOfBatches)
        {
            // Arrange.

            var parameters = new DynamicParameters();
            parameters.Add("sinceTime", DBNull.Value, DbType.DateTime);
            parameters.Add("batchSize", batchSize, DbType.Int32);
            parameters.Add("batchNumber", 1, DbType.Int32);
            parameters.Add("totalNumberOfBatches", dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Act.

            var results = TestDatabase.Query<object>("GetLearnersBatch", parameters, CommandType.StoredProcedure);

            // Assert.

            results.Should().NotBeNull();
            parameters.Get<int>("totalNumberOfBatches").Should().Be(expectedTotalNumberOfBatches);
        }
    }
}

using NUnit.Framework;
using FluentAssertions;
using Dapper;
using System.Data;
using System;

namespace SFA.DAS.Commitments.Database.IntegrationTests.StoredProcedures.GetLearnersBatch
{
    [TestFixture]
    public class WhenNoBatchSupplied : GetLearnersBatchIntegrationTestBase
    {
        [Test]
        public void Then_Use_Default_Values()
        {
            // Arrange.

            var parameters = new DynamicParameters();
            parameters.Add("sinceTime", DBNull.Value, DbType.DateTime);
            parameters.Add("batchSize", DBNull.Value, DbType.Int32, direction: ParameterDirection.InputOutput);
            parameters.Add("batchNumber", DBNull.Value, DbType.Int32, direction: ParameterDirection.InputOutput);
            parameters.Add("totalNumberOfBatches", dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Act.

            var results = TestDatabase.Query<object>("GetLearnersBatch", parameters, CommandType.StoredProcedure);

            // Assert.

            results.Should().NotBeNull();
            parameters.Get<int>("batchSize").Should().Be(1000);
            parameters.Get<int>("batchNumber").Should().Be(1);
        }
    }
}

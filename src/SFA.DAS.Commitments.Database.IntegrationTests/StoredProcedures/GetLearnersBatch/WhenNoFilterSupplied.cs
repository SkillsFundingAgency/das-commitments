using NUnit.Framework;
using FluentAssertions;
using Dapper;
using System.Data;
using System;

namespace SFA.DAS.Commitments.Database.IntegrationTests.StoredProcedures.GetLearnersBatch
{
    [TestFixture]
    public class WhenNoFilterSupplied : GetLearnersBatchIntegrationTestBase
    {
        [Test]
        public void Then_All_Valid_Learners_Returned()
        {
            // Arrange.

            var parameters = new DynamicParameters();
            parameters.Add("sinceTime", DBNull.Value, DbType.DateTime);
            parameters.Add("batchSize", DBNull.Value, DbType.Int32);
            parameters.Add("batchNumber", DBNull.Value, DbType.Int32);
            parameters.Add("totalNumberOfBatches", dbType: DbType.Int32, direction: ParameterDirection.Output);

            // Act.

            var results = TestDatabase.Query<object>("GetLearnersBatch", parameters, CommandType.StoredProcedure);

            // Assert.

            results.Should().NotBeNull();
            results.Should().HaveCount(17);
            parameters.Get<int>("totalNumberOfBatches").Should().Be(1);
        }
    }
}

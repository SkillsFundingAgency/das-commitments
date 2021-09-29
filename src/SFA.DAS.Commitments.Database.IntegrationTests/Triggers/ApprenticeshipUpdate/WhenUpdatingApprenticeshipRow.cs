using NUnit.Framework;
using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;

namespace SFA.DAS.Commitments.Database.IntegrationTests.Triggers.ApprenticeshipUpdate
{
    [TestFixture]
    public class WhenUpdatingApprenticeshipRow : IntegrationTestBase
    {
        [OneTimeSetUp]
        public void SeedRelatedData()
        {
            string sql = "SET IDENTITY_INSERT Commitment ON; ";
            sql += "INSERT INTO Commitment (Id, Reference, EmployerAccountId) VALUES (1, 'JRML7V', 30060); ";
            sql += "SET IDENTITY_INSERT Commitment OFF";
            TestDatabase.Execute(sql);
        }

        [Test]
        public void Then_UpdatedOn_Is_Set()
        {
            // Arrange.

            TestDatabase.Execute("SET IDENTITY_INSERT Apprenticeship ON; INSERT INTO Apprenticeship(Id, CommitmentId, FirstName, LastName, ULN, Cost) VALUES (1, 1, 'Alan', 'Apprenctice', '2140896210', 1500);");

            // Act.

            var updateTimeUtc = DateTime.UtcNow;
            TestDatabase.Execute("UPDATE Apprenticeship SET Cost = 1600 WHERE id = 1;");

            // Assert.

            var results = TestDatabase.Query<DateTime?>("SELECT UpdatedOn FROM Apprenticeship WHERE id = 1;");
            results.Should().NotBeNull();
            results.Should().HaveCount(1);
            results.ElementAt(0).Should().NotBeNull();
            results.ElementAt(0).Value.Should().BeCloseTo(updateTimeUtc, 2.Seconds());
        }
    }
}

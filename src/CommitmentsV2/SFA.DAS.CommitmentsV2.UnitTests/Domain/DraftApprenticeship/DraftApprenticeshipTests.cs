using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Types;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.DraftApprenticeship
{
    [TestFixture]
    public class DraftApprenticeshipConstructorTests
    {
        private DraftApprenticeshipDetails _source;

        [SetUp]
        public void Arrange()
        {
            var fixture = new Fixture();
            _source = fixture.Create<DraftApprenticeshipDetails>();
        }

        [Test]
        public void ThenFirstNameIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Originator.Employer);
            Assert.AreEqual(_source.FirstName, result.FirstName);
        }

        [Test]
        public void ThenLastNameIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Originator.Employer);
            Assert.AreEqual(_source.LastName, result.LastName);
        }

        [Test]
        public void ThenUlnIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Originator.Employer);
            Assert.AreEqual(_source.Uln, result.Uln);
        }

        [Test]
        public void ThenCostIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Originator.Employer);
            Assert.AreEqual(_source.Cost, result.Cost);
        }

        [Test]
        public void ThenStartDateIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Originator.Employer);
            Assert.AreEqual(_source.StartDate, result.StartDate);
        }

        [Test]
        public void ThenEndDateIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Originator.Employer);
            Assert.AreEqual(_source.EndDate, result.EndDate);
        }

        [Test]
        public void ThenDateOfBirthIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Originator.Employer);
            Assert.AreEqual(_source.DateOfBirth, result.DateOfBirth);
        }

        [Test]
        public void ThenEmployerRefIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Originator.Employer);
            Assert.AreEqual(_source.Reference, result.EmployerRef);
        }

        [Test]
        public void ThenProviderRefIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Originator.Provider);
            Assert.AreEqual(_source.Reference, result.ProviderRef);
        }

        [Test]
        public void ThenReservationIdIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Originator.Employer);
            Assert.AreEqual(_source.ReservationId, result.ReservationId);
        }
    }
}

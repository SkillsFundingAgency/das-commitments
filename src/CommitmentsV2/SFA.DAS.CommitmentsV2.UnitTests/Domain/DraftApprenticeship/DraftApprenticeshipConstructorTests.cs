using AutoFixture;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Types;

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
            _source = fixture.Build<DraftApprenticeshipDetails>().Without(x=>x.Uln).With(x => x.IsOnFlexiPaymentPilot, true).Create();
        }

        [Test]
        public void ThenFirstNameIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.AreEqual(_source.FirstName, result.FirstName);
        }

        [Test]
        public void ThenLastNameIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.AreEqual(_source.LastName, result.LastName);
        }

        [Test]
        public void ThenUlnIsMappedCorrectlyWhenOriginatorIsProvider()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Provider);
            Assert.AreEqual(_source.Uln, result.Uln);
        }

        [Test]
        public void ThenUlnIsNotMappedWhenOriginatorIsEmployer()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.AreEqual(_source.Uln, result.Uln);
            Assert.IsNull(result.Uln);
        }

        [Test]
        public void ThenThrowsDomainExceptionWhenUlnIsHackedWhenOriginatorIsEmployer()
        {
            var hackedSource = TestHelper.Clone(_source);
            hackedSource.Uln = "123456";
            Assert.Throws<DomainException>(() => new CommitmentsV2.Models.DraftApprenticeship(hackedSource, Party.Employer));
        }

        [Test]
        public void ThenCostIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.AreEqual(_source.Cost, result.Cost);
        }

        [Test]
        public void ThenStartDateIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.AreEqual(_source.StartDate, result.StartDate);
        }

        [Test]
        public void ThenEndDateIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.AreEqual(_source.EndDate, result.EndDate);
        }

        [Test]
        public void ThenDateOfBirthIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.AreEqual(_source.DateOfBirth, result.DateOfBirth);
        }

        [Test]
        public void ThenEmployerRefIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.AreEqual(_source.Reference, result.EmployerRef);
        }

        [Test]
        public void ThenProviderRefIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Provider);
            Assert.AreEqual(_source.Reference, result.ProviderRef);
        }

        [Test]
        public void ThenReservationIdIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.AreEqual(_source.ReservationId, result.ReservationId);
        }

        [Test]
        public void ThenIsOnFlexiPaymentPilotIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.AreEqual(_source.IsOnFlexiPaymentPilot, result.IsOnFlexiPaymentPilot);
        }
    }
}
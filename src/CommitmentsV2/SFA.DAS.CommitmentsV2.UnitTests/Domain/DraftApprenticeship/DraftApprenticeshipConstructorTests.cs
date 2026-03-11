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
            _source = fixture.Build<DraftApprenticeshipDetails>().Without(x=>x.Uln).Create();
        }

        [Test]
        public void ThenFirstNameIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.That(result.FirstName, Is.EqualTo(_source.FirstName));
        }

        [Test]
        public void ThenLastNameIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.That(result.LastName, Is.EqualTo(_source.LastName));
        }

        [Test]
        public void ThenUlnIsMappedCorrectlyWhenOriginatorIsProvider()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Provider);
            Assert.That(result.Uln, Is.EqualTo(_source.Uln));
        }

        [Test]
        public void ThenUlnIsNotMappedWhenOriginatorIsEmployer()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.That(result.Uln, Is.EqualTo(_source.Uln));
            Assert.That(result.Uln, Is.Null);
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
            Assert.That(result.Cost, Is.EqualTo(_source.Cost));
        }

        [Test]
        public void ThenStartDateIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.That(result.StartDate, Is.EqualTo(_source.StartDate));
        }

        [Test]
        public void ThenEndDateIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.That(result.EndDate, Is.EqualTo(_source.EndDate));
        }

        [Test]
        public void ThenDateOfBirthIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.That(result.DateOfBirth, Is.EqualTo(_source.DateOfBirth));
        }

        [Test]
        public void ThenEmployerRefIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.That(result.EmployerRef, Is.EqualTo(_source.Reference));
        }

        [Test]
        public void ThenProviderRefIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Provider);
            Assert.That(result.ProviderRef, Is.EqualTo(_source.Reference));
        }

        [Test]
        public void ThenReservationIdIsMappedCorrectly()
        {
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Employer);
            Assert.That(result.ReservationId, Is.EqualTo(_source.ReservationId));
        }

        [TestCase(null, null, null)]
        [TestCase(null, true, true)]
        [TestCase(null, false, false)]
        [TestCase(33434, null, false)]
        [TestCase(33434, false, false)]
        [TestCase(33434, true, true)]
        public void ThenLearnerDataIdIsMappedCorrectly(long? id, bool? initialRplValue, bool? rplNeeded)
        {
            _source.LearnerDataId = id;
            _source.RecognisePriorLearning = initialRplValue;
            var result = new CommitmentsV2.Models.DraftApprenticeship(TestHelper.Clone(_source), Party.Provider);
            result.LearnerDataId.Should().Be(_source.LearnerDataId);
            result.RecognisePriorLearning.Should().Be(rplNeeded);
        }
    }
}
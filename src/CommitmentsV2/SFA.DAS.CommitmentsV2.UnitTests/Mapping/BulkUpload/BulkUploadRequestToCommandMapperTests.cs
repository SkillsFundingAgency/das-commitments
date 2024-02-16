using AutoFixture;
using MoreLinq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Mapping.BulkUpload;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.BulkUpload
{
    [TestFixture]
    public class BulkUploadRequestToCommandMapperTests
    {
        private BulkUploadAddDraftApprenticeshipsRequestToCommandMapper _mapper;
        private BulkUploadAddDraftApprenticeshipsRequest _source;
        private BulkUploadAddDraftApprenticeshipsCommand _result;

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Create<BulkUploadAddDraftApprenticeshipsRequest>();

            _mapper = new BulkUploadAddDraftApprenticeshipsRequestToCommandMapper();

            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void CohortIdIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.CohortId, Is.EqualTo(source.CohortId));
            });
        }

        [Test]
        public void CourseCodeIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.CourseCode, Is.EqualTo(source.CourseCode));
            });
        }

        [Test]
        public void CostIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.Cost, Is.EqualTo(source.Cost));
            });
        }

        [Test]
        public void FirstNameIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.FirstName, Is.EqualTo(source.FirstName));
            });
        }

        [Test]
        public void LastNameIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.LastName, Is.EqualTo(source.LastName));
            });
        }

        [Test]
        public void EmailIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.Email, Is.EqualTo(source.Email));
            });
        }

        [Test]
        public void UlnIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.Uln, Is.EqualTo(source.Uln));
            });
        }

        [Test]
        public void StartDateIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.StartDate, Is.EqualTo(source.StartDate));
            });
        }

        [Test]
        public void EndDateIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.EndDate, Is.EqualTo(source.EndDate));
            });
        }

        [Test]
        public void DateOfBirthIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.DateOfBirth, Is.EqualTo(source.DateOfBirth));
            });
        }

        [Test]
        public void ReferenceIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.ProviderRef, Is.EqualTo(source.ProviderRef));
            });
        }

        [Test]
        public void ReservationIdIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.That(result.ReservationId, Is.EqualTo(source.ReservationId));
            });
        }

        [Test]
        public void ProviderIdIsMappedCorrectly()
        {
            Assert.That(_source.ProviderId, Is.EqualTo(_result.ProviderId));
        }

        [Test]
        public void UserInfoIsMappedCorrectly()
        {
            Assert.That(_source.UserInfo.UserDisplayName == _result.UserInfo.UserDisplayName &&
                _source.UserInfo.UserEmail == _result.UserInfo.UserEmail &&
                _source.UserInfo.UserId == _result.UserInfo.UserId, Is.True);
        }

        [Test]
        public void LogIdIsMappedCorrectly()
        {
            Assert.That(_source.LogId, Is.EqualTo(_result.LogId));
        }

        [Test]
        public void ProviderActionIsCreatedCorrectly()
        {
            Assert.That("SaveAsDraft", Is.EqualTo(_result.ProviderAction));
        }
    }
}

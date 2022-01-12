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
                Assert.AreEqual(source.CohortId, result.CohortId);
            });
        }

        [Test]
        public void CourseCodeIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.CourseCode, result.CourseCode);
            });
        }

        [Test]
        public void CostIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.Cost, result.Cost);
            });
        }

        [Test]
        public void FirstNameIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.FirstName, result.FirstName);
            });
        }

        [Test]
        public void LastNameIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.LastName, result.LastName);
            });
        }

        [Test]
        public void EmailIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.Email, result.Email);
            });
        }

        [Test]
        public void UlnIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.Uln, result.Uln);
            });
        }

        [Test]
        public void StartDateIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.StartDate, result.StartDate);
            });
        }

        [Test]
        public void EndDateIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.EndDate, result.EndDate);
            });
        }

        [Test]
        public void DateOfBirthIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.DateOfBirth, result.DateOfBirth);
            });
        }

        [Test]
        public void ReferenceIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.ProviderRef, result.ProviderRef);
            });
        }

        [Test]
        public void ReservationIdIsMappedCorrectly()
        {
            _source.BulkUploadDraftApprenticeships.ForEach(source =>
            {
                var result = _result.BulkUploadDraftApprenticeships.First(y => y.Uln == source.Uln);
                Assert.AreEqual(source.ReservationId, result.ReservationId);
            });
        }

        [Test]
        public void ProviderIdIsMappedCorrectly()
        {
            Assert.AreEqual(_result.ProviderId, _source.ProviderId);
        }

        [Test]
        public void UserInfoIsMappedCorrectly()
        {
            Assert.IsTrue(_source.UserInfo.UserDisplayName == _result.UserInfo.UserDisplayName &&
                _source.UserInfo.UserEmail == _result.UserInfo.UserEmail &&
                _source.UserInfo.UserId == _result.UserInfo.UserId);
        }
    }
}

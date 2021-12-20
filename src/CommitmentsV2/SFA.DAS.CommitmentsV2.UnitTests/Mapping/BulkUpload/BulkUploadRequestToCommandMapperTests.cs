using AutoFixture;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Mapping.BulkUpload;
using System.Linq;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.BulkUpload
{
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

        //[Test]
        //public void DraftApprenticeshipsAreMappedCorrectly()
        //{
        //    var compare = new CompareLogic();
        //    var compareResult = compare.Compare(_source.BulkUploadDraftApprenticeships.ToList(), _result.BulkUploadDraftApprenticeships);
        //    Assert.IsTrue(compareResult.AreEqual);
        //}

        [Test]
        public void ProviderIdIsMappedCorrectly()
        {
            Assert.AreEqual(_result.ProviderId, _source.ProviderId);
        }

        [Test]
        public void UserInfoIsMappedCorrectly()
        {
            var compare = new CompareLogic();
            var compareResult = compare.Compare(_source.UserInfo, _result.UserInfo);
            Assert.IsTrue(compareResult.AreEqual);
        }
    }
}

using KellermanSoftware.CompareNetObjects;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.AddFileUploadLog;
using SFA.DAS.CommitmentsV2.Mapping.BulkUpload;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.CommandToResponseMappers
{
    public class BulkUploadAddLogRequestToCommandMapperTests
    {
        private BulkUploadAddLogRequestToCommandMapper _mapper;
        private AddFileUploadLogRequest _source;
        private AddFileUploadLogCommand _result;

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Create<AddFileUploadLogRequest>();

            _mapper = new BulkUploadAddLogRequestToCommandMapper();

            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void BulkUploadAddLogProviderIdIsMappedCorrectly()
        {
            var compare = new CompareLogic();
            var compareResult = compare.Compare(_source.ProviderId, _result.ProviderId);
            Assert.That(compareResult.AreEqual, Is.True);
        }

        [Test]
        public void BulkUploadAddLogRplCountIsMappedCorrectly()
        {
            var compare = new CompareLogic();
            var compareResult = compare.Compare(_source.RplCount, _result.RplCount);
            Assert.That(compareResult.AreEqual, Is.True);
        }

        [Test]
        public void BulkUploadAddLogRowCountIsMappedCorrectly()
        {
            var compare = new CompareLogic();
            var compareResult = compare.Compare(_source.RowCount, _result.RowCount);
            Assert.That(compareResult.AreEqual, Is.True);
        }

        [Test]
        public void BulkUploadAddLogFileNameIsMappedCorrectly()
        {
            var compare = new CompareLogic();
            var compareResult = compare.Compare(_source.FileName, _result.FileName);
            Assert.That(compareResult.AreEqual, Is.True);
        }

        [Test]
        public void BulkUploadAddLogFileContentIsMappedCorrectly()
        {
            var compare = new CompareLogic();
            var compareResult = compare.Compare(_source.FileContent, _result.FileContent);
            Assert.That(compareResult.AreEqual, Is.True);
        }
    }
}

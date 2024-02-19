using KellermanSoftware.CompareNetObjects;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetOverlappingTrainingDateRequest;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class GetOverlappingTrainingDateRequestResponceMapperTests
    {
        private readonly GetOverlappingTrainingDateRequestResponceMapper _mapper;
        private GetOverlappingTrainingDateRequestQueryResult _source;
        private GetOverlappingTrainingDateRequestResponce _result;

        public GetOverlappingTrainingDateRequestResponceMapperTests()
        {
            _mapper = new GetOverlappingTrainingDateRequestResponceMapper();
        }

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Build<GetOverlappingTrainingDateRequestQueryResult>().Create();
            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void ResponseIsMappedCorrectly()
        {
            var compare = new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true });
            var compareResult = compare.Compare(_source, _result);
            Assert.That(compareResult.AreEqual, Is.True);
        }
    }
}
using KellermanSoftware.CompareNetObjects;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetChangeOfPartyRequests;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class GetChangeOfPartyRequestsResponseMapperTests
    {
        private readonly GetChangeOfPartyRequestsResponseMapper _mapper;
        private GetChangeOfPartyRequestsQueryResult _source;
        private GetChangeOfPartyRequestsResponse _result;

        public GetChangeOfPartyRequestsResponseMapperTests()
        {
            _mapper = new GetChangeOfPartyRequestsResponseMapper();
        }

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Create<GetChangeOfPartyRequestsQueryResult>();
            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void RequestsAreMappedCorrectly()
        {
            var compare = new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true });
            var compareResult = compare.Compare(_source.ChangeOfPartyRequests, _result.ChangeOfPartyRequests);
            Assert.That(compareResult.AreEqual, Is.True);
        }
    }
}

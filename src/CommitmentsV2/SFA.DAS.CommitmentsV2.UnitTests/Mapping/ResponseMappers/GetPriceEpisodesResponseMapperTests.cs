using KellermanSoftware.CompareNetObjects;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetPriceEpisodes;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class GetPriceEpisodesResponseMapperTests
    {
        private readonly GetPriceEpisodesResponseMapper _mapper;
        private GetPriceEpisodesQueryResult _source;
        private GetPriceEpisodesResponse _result;

        public GetPriceEpisodesResponseMapperTests()
        {
            _mapper = new GetPriceEpisodesResponseMapper();
        }

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Create<GetPriceEpisodesQueryResult>();
            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void PriceEpisodesAreMappedCorrectly()
        {
            var compare = new CompareLogic(new ComparisonConfig{ IgnoreObjectTypes = true});
            var compareResult = compare.Compare(_source.PriceEpisodes, _result.PriceEpisodes);
            Assert.That(compareResult.AreEqual, Is.True);
        }
    }
}

using AutoFixture;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetDataLocks;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    public class GetDataLocksResponseMapperTests
    {
        private readonly GetDataLocksResponseMapper _mapper;
        private GetDataLocksQueryResult _source;
        private GetDataLocksResponse _result;

        public GetDataLocksResponseMapperTests()
        {
            _mapper = new GetDataLocksResponseMapper();
        }

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Create<GetDataLocksQueryResult>();
            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void ApprenticeshipUpdatesAreMappedCorrectly()
        {
            var compare = new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true, MaxDifferences = 100 });
            var compareResult = compare.Compare(_source.DataLocks, _result.DataLocks);
            Assert.That(compareResult.AreEqual, Is.True);
        }
    }
}

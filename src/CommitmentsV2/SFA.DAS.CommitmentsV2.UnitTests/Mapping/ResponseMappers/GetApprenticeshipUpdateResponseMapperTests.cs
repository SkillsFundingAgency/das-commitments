using System.Threading.Tasks;
using AutoFixture;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class GetApprenticeshipUpdateResponseMapperTests
    {
        private readonly GetApprenticeshipUpdateMapper _mapper;
        private GetApprenticeshipUpdateQueryResult _source;

        public GetApprenticeshipUpdateResponseMapperTests()
        {
            _mapper = new GetApprenticeshipUpdateMapper();
        }

        [SetUp]
        public void Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Create<GetApprenticeshipUpdateQueryResult>();
        }

        [Test]
        public async Task ApprenticeshipUpdateIsMappedCorrectly()
        {
            var result = await _mapper.Map(TestHelper.Clone(_source));
            var compare = new CompareLogic(new ComparisonConfig{ IgnoreObjectTypes = true});
            var compareResult = compare.Compare(_source.PendingApprenticeshipUpdate, result.PendingApprenticeshipUpdate);
            Assert.IsTrue(compareResult.AreEqual);
        }

        [Test]
        public async Task ApprenticeshipUpdateIsMapsToNullCorrectly()
        {
            var result = await _mapper.Map(new GetApprenticeshipUpdateQueryResult());
            Assert.IsNotNull(result);
            Assert.IsNull(result.PendingApprenticeshipUpdate);
        }
    }
}

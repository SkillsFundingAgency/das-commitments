using System.Threading.Tasks;
using AutoFixture;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeship;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class GetApprenticeshipResponseMapperTests
    {
        private readonly GetApprenticeshipResponseMapper _mapper;
        private GetApprenticeshipQueryResult _source;
        private GetApprenticeshipResponse _result;

        public GetApprenticeshipResponseMapperTests()
        {
            _mapper = new GetApprenticeshipResponseMapper();
        }

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Build<GetApprenticeshipQueryResult>()
                .With(e => e.DeliveryModel, DeliveryModel.PortableFlexiJob)
                .Without(e => e.FlexibleEmployment)
                .Create();
            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void ResponseIsMappedCorrectly()
        {
            var compare = new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true });
            var compareResult = compare.Compare(_source, _result);
            Assert.IsTrue(compareResult.AreEqual);
        }

        [Test]
        public void DeliveryModelIsMappedCorrectly()
        {
            Assert.AreEqual(_source.DeliveryModel, _result.DeliveryModel);
        }
    }
}
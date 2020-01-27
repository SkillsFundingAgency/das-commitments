using System.Threading.Tasks;
using AutoFixture;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeshipUpdate;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class UpdateDraftApprenticeshipRequestToUpdateDraftApprenticeshipCommandMapperTests
    {
        private readonly GetApprenticeshipUpdateResponseMapper _mapper;
        private GetApprenticeshipUpdateQueryResult _source;
        private GetApprenticeshipUpdatesResponse _result;

        public UpdateDraftApprenticeshipRequestToUpdateDraftApprenticeshipCommandMapperTests()
        {
            _mapper = new GetApprenticeshipUpdateResponseMapper();
        }

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Create<GetApprenticeshipUpdateQueryResult>();
            foreach (var app in  _source.ApprenticeshipUpdates)
            {
                app.Originator = Originator.Employer;
            }
            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void ApprenticeshipUpdatesAreMappedCorrectly()
        {
            var compare = new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true });
            var compareResult = compare.Compare(_source.ApprenticeshipUpdates, _result.ApprenticeshipUpdates);
            Assert.IsTrue(compareResult.AreEqual);
        }
    }
}

using System.Threading.Tasks;
using AutoFixture;
using KellermanSoftware.CompareNetObjects;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetEmployerCohortsReadyForApproval;
using SFA.DAS.CommitmentsV2.Mapping.ResponseMappers;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.ResponseMappers
{
    [TestFixture]
    public class GetEmployerCohortsReadyForApprovalResponseMapperTests
    {
        private readonly GetEmployerCohortsReadyForApprovalResponseMapper _mapper;
        private GetEmployerCohortsReadyForApprovalQueryResults _source;
        private GetEmployerCohortsReadyForApprovalResponse _result;

        public GetEmployerCohortsReadyForApprovalResponseMapperTests()
        {
            _mapper = new GetEmployerCohortsReadyForApprovalResponseMapper();
        }

        [SetUp]
        public async Task Arrange()
        {
            var autoFixture = new Fixture();
            _source = autoFixture.Build<GetEmployerCohortsReadyForApprovalQueryResults>().Create();
            _result = await _mapper.Map(TestHelper.Clone(_source));
        }

        [Test]
        public void ResponseIsMappedCorrectly()
        {
            var compare = new CompareLogic(new ComparisonConfig { IgnoreObjectTypes = true });
            var compareResult = compare.Compare(_source, _result);
            Assert.IsTrue(compareResult.AreEqual);
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Api.Controllers;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types.Commitment;

namespace SFA.DAS.Commitments.Api.UnitTests.Controllers.ProviderControllerTests
{
    [TestFixture]
    public class WhenGettingCommitmentAgreements
    {
        private ProviderController _controller;
        private Mock<IProviderOrchestrator> _mockProviderOrchestrator;

        [SetUp]
        public void Setup()
        {
            _mockProviderOrchestrator = new Mock<IProviderOrchestrator>();

            _controller = new ProviderController(_mockProviderOrchestrator.Object, new Mock<IApprenticeshipsOrchestrator>().Object);
        }

        [Test]
        public async Task ThenCommitmentAgreementsReturnedFromOrchestratorAreReturned()
        {
            // arrange
            const long providerId = 123L;

            var commitmentAgreements = new[] {new CommitmentAgreement {Reference = "ref", LegalEntityName = "len", AccountLegalEntityPublicHashedId = "aleHash"}};

            _mockProviderOrchestrator.Setup(o => o.GetCommitmentAgreements(providerId)).ReturnsAsync(TestHelper.Clone(commitmentAgreements));

            // act
            var result = await _controller.GetCommitmentAgreements(providerId);

            // assert
            Assert.IsNotNull(result);

            var contentResult = result as OkNegotiatedContentResult<IEnumerable<CommitmentAgreement>>;
            Assert.IsNotNull(contentResult);

            //todo: story gives sort order, need to implement/test
            Assert.IsTrue(TestHelper.EnumerablesAreEqual(commitmentAgreements, contentResult.Content));
        }
    }
}

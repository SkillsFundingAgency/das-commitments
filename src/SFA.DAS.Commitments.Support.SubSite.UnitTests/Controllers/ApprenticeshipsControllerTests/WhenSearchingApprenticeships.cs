using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Controllers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using System.Threading.Tasks;
using FluentAssertions;
using System.Web.Mvc;
using SFA.DAS.Commitments.Support.SubSite.Enums;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Controllers.ApprenticeshipsControllerTests
{
    [TestFixture]
    public class WhenSearchingApprenticeships
    {
        private Mock<IApprenticeshipsOrchestrator> _orchestrator;

        [SetUp]
        public void SetUpTest()
        {
            _orchestrator = new Mock<IApprenticeshipsOrchestrator>();
        }

        [Test]
        public async Task GivenValidUlnSearchShouldReturnUlnView()
        {
            ///Arrange
            var query = new ApprenticeshipSearchQuery
            {
                SearchTerm = "25632323233",
                SearchType = ApprenticeshipSearchType.SearchByUln,
                HashedAccountId = "ASDNA"
            };

            _orchestrator
                .Setup(x => x.GetApprenticeshipsByUln(It.IsAny<ApprenticeshipSearchQuery>()))
                .ReturnsAsync(new UlnSummaryViewModel())
                .Verifiable();

            var sut = new ApprenticeshipsController(_orchestrator.Object);

            // Act
            var result = await sut.Search(query.HashedAccountId);

            // Assert
            var view = result as ViewResult;

            view.Should().NotBeNull();

            
        }

        [Test]
        public async Task GivenValidCohortIdSearchShouldReturnCohortSummaryView()
        {
            ///Arrange
            var query = new ApprenticeshipSearchQuery
            {
                SearchTerm = "JRML7V",
                SearchType = ApprenticeshipSearchType.SearchByCohort,
                HashedAccountId = "ASDNA"
            };

            _orchestrator
                .Setup(x => x.GetCommitmentSummary(It.IsAny<ApprenticeshipSearchQuery>()))
                .ReturnsAsync(new CommitmentSummaryViewModel())
                .Verifiable();

            var sut = new ApprenticeshipsController(_orchestrator.Object);

            // Act
            var result = await sut.SearchRequest(query.HashedAccountId, query.SearchType, query.SearchTerm);

            // Assert
            var view = result as ViewResult;

            view.Should().NotBeNull();
            Assert.AreEqual(view.ViewName, "CohortSearchSummary");

            var model = view.Model as CommitmentSummaryViewModel;
            model.Should().NotBeNull();
        }

    }
}

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
        public async Task GivenValidAccountIdShouldReturnSearchView()
        {
            // Arrange
            var query = new ApprenticeshipSearchQuery
            {
                SearchTerm = "25632323233",
                SearchType = ApprenticeshipSearchType.SearchByUln,
                HashedAccountId = "ASDNA"
            };

            var sut = new ApprenticeshipsController(_orchestrator.Object);

            // Act
            var result = await sut.Search(query.HashedAccountId);

            // Assert
            var view = result as ViewResult;
            view.Should().NotBeNull();

            var model = view.Model as ApprenticeshipSearchQuery;
            model.ResponseUrl.Should().Contain(query.HashedAccountId);
        }

        [Test]
        public async Task GivenUlnSearchResultHasErrorShouldReturnSearchViewWithErrorResponse()
        {
            // Arrange
            var errorResponse = "InvalidUrn";
            var query = new ApprenticeshipSearchQuery
            {
                SearchTerm = "",
                SearchType = ApprenticeshipSearchType.SearchByUln,
                HashedAccountId = "ASDNA"
            };

            _orchestrator
                .Setup(x => x.GetApprenticeshipsByUln(It.IsAny<ApprenticeshipSearchQuery>()))
                .ReturnsAsync(new UlnSummaryViewModel { ReponseMessages = { errorResponse } })
                .Verifiable();

            var sut = new ApprenticeshipsController(_orchestrator.Object);

            // Act
            var result = await sut.SearchRequest(query.HashedAccountId, query.SearchType, query.SearchTerm);

            // Assert
            var view = result as ViewResult;

            view.Should().NotBeNull();
            Assert.AreEqual(view.ViewName, "Search");

            var model = view.Model as ApprenticeshipSearchQuery;
            model.ReponseMessages.Should().Contain(errorResponse);
        }

        [Test]
        public async Task GivenValidUlnSearchShouldReturnUlnView()
        {
            // Arrange
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
            var result = await sut.SearchRequest(query.HashedAccountId, query.SearchType, query.SearchTerm);

            // Assert
            var view = result as ViewResult;

            view.Should().NotBeNull();
            Assert.AreEqual(view.ViewName, "UlnSearchSummary");
        }

        [Test]
        public async Task GivenValidUlnSearchShouldSetAccountIdOnViewModel()
        {
            // Arrange
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
            var result = await sut.SearchRequest(query.HashedAccountId, query.SearchType, query.SearchTerm);

            // Assert
            var view = result as ViewResult;

            var model = view.Model as UlnSummaryViewModel;
            model.CurrentHashedAccountId.Should().BeEquivalentTo(query.HashedAccountId);
        }

        [Test]
        public async Task GivenCohortSearchResultHasErrorShouldReturnSearchViewWithErrorResponse()
        {
            // Arrange
            var errorResponse = "InvalidUrn";
            var query = new ApprenticeshipSearchQuery
            {
                SearchTerm = "",
                SearchType = ApprenticeshipSearchType.SearchByCohort,
                HashedAccountId = "ASDNA"
            };

            _orchestrator
                .Setup(x => x.GetCommitmentSummary(It.IsAny<ApprenticeshipSearchQuery>()))
                .ReturnsAsync(new CommitmentSummaryViewModel { ReponseMessages = { errorResponse } })
                .Verifiable();

            var sut = new ApprenticeshipsController(_orchestrator.Object);

            // Act
            var result = await sut.SearchRequest(query.HashedAccountId, query.SearchType, query.SearchTerm);

            // Assert
            var view = result as ViewResult;

            view.Should().NotBeNull();
            Assert.AreEqual(view.ViewName, "Search");

            var model = view.Model as ApprenticeshipSearchQuery;
            model.ReponseMessages.Should().Contain(errorResponse);
        }

        [Test]
        public async Task GivenValidCohortIdSearchShouldReturnCohortSummaryView()
        {
            // Arrange
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

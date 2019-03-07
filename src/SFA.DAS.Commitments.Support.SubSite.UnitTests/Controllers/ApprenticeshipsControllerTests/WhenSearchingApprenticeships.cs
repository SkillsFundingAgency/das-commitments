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
        private ApprenticeshipsController _sut;

        [SetUp]
        public void SetUpTest()
        {
            _orchestrator = new Mock<IApprenticeshipsOrchestrator>();
            _sut = new ApprenticeshipsController(_orchestrator.Object); ;
        }

        [Test]
        public async Task GivenValidAccountIdShouldReturnSearchView()
        {
            // Arrange
            var hashedAccountId = "ASDNA";

            // Act
            var result = await _sut.Search(hashedAccountId);

            // Assert
            var view = result as ViewResult;
            view.Should().NotBeNull();

            var model = view.Model as ApprenticeshipSearchQuery;
            model.ResponseUrl.Should().Contain(hashedAccountId);
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

            // Act
            var result = await _sut.SearchRequest(query.HashedAccountId, query.SearchType, query.SearchTerm);

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

            // Act
            var result = await _sut.SearchRequest(query.HashedAccountId, query.SearchType, query.SearchTerm);

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

            // Act
            var result = await _sut.SearchRequest(query.HashedAccountId, query.SearchType, query.SearchTerm);

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

            // Act
            var result = await _sut.SearchRequest(query.HashedAccountId, query.SearchType, query.SearchTerm);

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

            // Act
            var result = await _sut.SearchRequest(query.HashedAccountId, query.SearchType, query.SearchTerm);

            // Assert
            var view = result as ViewResult;

            view.Should().NotBeNull();
            Assert.AreEqual(view.ViewName, "CohortSearchSummary");

            var model = view.Model as CommitmentSummaryViewModel;
            model.Should().NotBeNull();
        }
    }
}

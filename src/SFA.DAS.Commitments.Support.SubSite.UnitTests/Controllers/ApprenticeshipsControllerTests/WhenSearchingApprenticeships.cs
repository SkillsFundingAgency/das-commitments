using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Controllers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using System.Threading.Tasks;
using FluentAssertions;
using System.Web.Mvc;

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
                SearchType = ApprenticeshipSearchType.SearchByUln
            };

            _orchestrator
                .Setup(x => x.GetApprenticeshipsByUln(query))
                .ReturnsAsync(new UlnSearchResultSummaryViewModel())
                .Verifiable();

            var sut = new ApprenticeshipsController(_orchestrator.Object);

            // Act
            var result = await sut.Search(query);

            // Assert
            var view = result as ViewResult;

            view.Should().NotBeNull();

            Assert.AreEqual(view.ViewName, "ApprenticeshipsUlnSearchSummary");
        }



    }
}

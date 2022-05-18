using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Controllers;
using SFA.DAS.Commitments.Support.SubSite.Models;
using SFA.DAS.Commitments.Support.SubSite.Orchestrators;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Controllers.ApprenticeshipsControllerTests
{
    //[TestFixture]
    //public class WhenGettingApprenticeshipDetails
    //{
    //    private Mock<IApprenticeshipsOrchestrator> _orchestrator;

    //    [SetUp]
    //    public void SetUpTest()
    //    {
    //        _orchestrator = new Mock<IApprenticeshipsOrchestrator>();
    //    }

    //    [Test]
    //    public async Task GivenValidUlnAndAccountIdShouldCallServiceAndReturnApprenticeshipView()
    //    {
    //        ///Arrange
    //        var apprenticeshipHashId = "V673UHWE";
    //        var accountHashId = "HTYDUD120";
    //        var apprenticeshipVm = new ApprenticeshipViewModel
    //        {
    //            Uln = "123456782"
    //        };

    //        _orchestrator
    //            .Setup(x => x.GetApprenticeship(apprenticeshipHashId, accountHashId))
    //            .ReturnsAsync(apprenticeshipVm)
    //            .Verifiable();

    //        var controller = new ApprenticeshipsController(_orchestrator.Object);

    //        // Act
    //        var result = await controller.Index(apprenticeshipHashId, accountHashId);

    //        // Assert
    //        _orchestrator.VerifyAll();

    //        var view = result as ViewResult;
    //        view.Should().NotBeNull();
    //        view.Model.Should().BeOfType<ApprenticeshipViewModel>();
    //    }
    //}
}
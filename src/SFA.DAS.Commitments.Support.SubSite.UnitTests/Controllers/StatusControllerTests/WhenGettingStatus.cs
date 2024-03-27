using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using SFA.DAS.Commitments.Support.SubSite.Controllers;
using SFA.DAS.Commitments.Support.SubSite.GlobalConstants;
using SFA.DAS.Commitments.Support.SubSite.Models;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Support.SubSite.UnitTests.Controllers.StatusControllerTests;

[TestFixture]
public class WhenGettingStatus
{
    [Test]
    public void ShouldReturnStatusModel()
    {
        var sut = new StatusController();
        var viewResult = sut.Get();

        Assert.That(viewResult.GetType(), Is.EqualTo(typeof(OkObjectResult)));
        var objectResult = (OkObjectResult)viewResult;
        Assert.That(objectResult.StatusCode, Is.EqualTo(200));

        var result = objectResult.Value as ServiceStatusViewModel;
        Assert.That(result, Is.Not.Null);
        Assert.That(result.ServiceName, Is.EqualTo(ApplicationConstants.ServiceName));
    }
}
﻿using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.DownloadRestrictionsExtensions;

[TestFixture]
public class WhenFilteringApprenticeships
{
    [Test]
    public void ThenShouldFilterByEndDate()
    {
        //Arrange
        var apprenticeships = new List<Apprenticeship>
        {
            new() {EndDate = DateTime.UtcNow, FirstName = "Included"},
            new() {EndDate = DateTime.UtcNow.AddMonths(-11), FirstName = "Included"},
            new() {EndDate = DateTime.UtcNow.AddMonths(-13), FirstName = "NotIncluded"},
            new() {EndDate = DateTime.UtcNow.AddMonths(-12), FirstName = "NotIncluded"}
        }.AsQueryable();

        //Act
        var result = apprenticeships.DownloadsFilter(true);

        Assert.Multiple(() =>
        {
            //Assert
            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.All(x => x.FirstName.Equals("Included")), Is.True);
        });
    }
}
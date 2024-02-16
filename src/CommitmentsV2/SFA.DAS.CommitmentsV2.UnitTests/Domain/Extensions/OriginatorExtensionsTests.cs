using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.Extensions;

public class OriginatorExtensionsTests
{
    [TestCase(Originator.Provider, Party.Provider)]
    [TestCase(Originator.Employer, Party.Employer)]
    public void TestValidOriginatorMapping(Originator originator, Party expectedResult)
    {
        var result = originator.ToParty();
        Assert.That(result, Is.EqualTo(expectedResult));
    }

    [TestCase(Originator.Unknown)]
    public void TestInvalidOriginatorMapping(Originator originator)
    {
        Assert.Throws<ArgumentException>(() => originator.ToParty());
    }
}
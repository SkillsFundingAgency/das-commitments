using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Extensions
{
    [TestFixture]
    public class EnumExtensionTests
    {
        [TestCase(ApprenticeshipStatus.WaitingToStart, "Waiting to start")]
        [TestCase(AgreementStatus.BothAgreed, "Both agreed")]
        public void EnumsWithDescription_CorrectEnumDescriptionIsReturned(Enum value, string expectedResult)
        {
            Assert.AreEqual(expectedResult, value.GetDescription());
        }

        [TestCase(PaymentStatus.Active, "Active")]
        [TestCase(Party.Employer, "Employer")]
        public void EnumsWithOutDescription_EnumAsTextIsReturned(Enum value, string expectedResult)
        {
            Assert.AreEqual(expectedResult, value.GetDescription());
        }
    }
}

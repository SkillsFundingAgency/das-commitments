using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Shared.Extensions;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Shared.UnitTests.Extensions
{
    [TestFixture]
    public class EnumExtensionTests
    {
        [TestCase(AgreementStatus.BothAgreed, "Both agreed")]
        public void EnumsWithDescription_CorrectEnumDescriptionIsReturned(AgreementStatus value, string expectedResult)
        {
            Assert.That(value.GetDescription(), Is.EqualTo(expectedResult));
        }

        [TestCase(ApprenticeshipStatus.WaitingToStart, "Waiting to start")]
        public void EnumsWithDescription_CorrectEnumDescriptionIsReturned(ApprenticeshipStatus value, string expectedResult)
        {
            Assert.That(value.GetDescription(), Is.EqualTo(expectedResult));
        }

        [TestCase(PaymentStatus.Active, "Active")]
        public void EnumsWithOutDescription_EnumAsTextIsReturned(PaymentStatus value, string expectedResult)
        {
            Assert.That(value.GetDescription(), Is.EqualTo(expectedResult));
        }

        [TestCase(Party.Employer, "Employer")]
        public void EnumsWithOutDescription_EnumAsTextIsReturned(Party value, string expectedResult)
        {
            Assert.That(value.GetDescription(), Is.EqualTo(expectedResult));
        }
    }
}

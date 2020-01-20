using System;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Mappers;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.AutoFixture;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.Apprenticeships
{
    public class ApprenticeshipStatusMapperTests
    {
        [TestCase(PaymentStatus.Active, ApprenticeshipStatus.Live)]
        [TestCase(PaymentStatus.Paused, ApprenticeshipStatus.Paused)]
        [TestCase(PaymentStatus.Withdrawn, ApprenticeshipStatus.Stopped)]
        [TestCase(PaymentStatus.Completed, ApprenticeshipStatus.Completed)]
        [TestCase(PaymentStatus.Deleted, ApprenticeshipStatus.Live)]
        [TestCase(null, ApprenticeshipStatus.WaitingToStart)]
        public void And_Has_Started_The_Payment_Status_Is_Mapped_To_Apprenticeship_Status(PaymentStatus paymentStatus, ApprenticeshipStatus expected)
        {
            //Arrange
            var mapper = new ApprenticeshipStatusMapper(new CurrentDateTime());

            //Act
            var actual = mapper.MapPaymentStatus(paymentStatus, DateTime.UtcNow.AddMonths(-2));

            //Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void And_Is_Waiting_To_Start_Then_Payment_Status_Is_Mapped_To_Apprenticeship_Status()
        {
            //Arrange
            var mapper = new ApprenticeshipStatusMapper(new CurrentDateTime());
            var expected = ApprenticeshipStatus.WaitingToStart;

            //Act
            var actual = mapper.MapPaymentStatus(PaymentStatus.Active, DateTime.UtcNow.AddMonths(2));

            //Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
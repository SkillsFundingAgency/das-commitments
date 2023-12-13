using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Mapping.Apprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Services;
using SFA.DAS.CommitmentsV2.Shared.Services;
using SFA.DAS.CommitmentsV2.Types;
using PaymentStatus = SFA.DAS.CommitmentsV2.Types.PaymentStatus;

namespace SFA.DAS.CommitmentsV2.UnitTests.Domain.Extensions
{
    public class ApprenticeshipStatusExtensionTests
    {
        [TestCase(PaymentStatus.Active, ApprenticeshipStatus.Live)]
        [TestCase(PaymentStatus.Paused, ApprenticeshipStatus.Paused)]
        [TestCase(PaymentStatus.Withdrawn, ApprenticeshipStatus.Stopped)]
        [TestCase(PaymentStatus.Completed, ApprenticeshipStatus.Completed)]
        public async Task And_Has_Started_The_Payment_Status_Is_Mapped_To_Apprenticeship_Status(PaymentStatus paymentStatus, ApprenticeshipStatus expected)
        {
            //Arrange
            var apprenticeship = CreateApprenticeship();
            apprenticeship.PaymentStatus = paymentStatus;
            apprenticeship.StartDate = DateTime.UtcNow.AddMonths(-2);
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            //Act
            var actual = await mapper.Map(apprenticeship);

            //Assert
            Assert.That(actual.ApprenticeshipStatus, Is.EqualTo(expected));
        }

        [Test]
        public async Task And_Is_Waiting_To_Start_Then_Payment_Status_Is_Mapped_To_Apprenticeship_Status()
        {
            //Arrange
            var apprenticeship = CreateApprenticeship();
            apprenticeship.PaymentStatus = PaymentStatus.Active;
            apprenticeship.StartDate = DateTime.UtcNow.AddMonths(2);
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            //Act
            var actual = await mapper.Map(apprenticeship);

            //Assert
            Assert.That(actual.ApprenticeshipStatus, Is.EqualTo(ApprenticeshipStatus.WaitingToStart));
        }

        [Test]
        public async Task And_ApprenticeshipConfirmationStatus_Is_Mapped_To_ConfirmationStatus()
        {
            //Arrange
            var apprenticeship = CreateApprenticeship();
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            //Act
            var actual = await mapper.Map(apprenticeship);

            //Assert
            Assert.That(actual.ConfirmationStatus, Is.EqualTo(apprenticeship.ApprenticeshipConfirmationStatus.ConfirmationStatus));
        }

        [Test]
        public async Task And_ApprenticeshipConfirmationStatus_Is_Mapped_To_Unconfirmed_When_Not_Present()
        {
            //Arrange
            var apprenticeship = CreateApprenticeship();
            apprenticeship.ApprenticeshipConfirmationStatus = null;
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            //Act
            var actual = await mapper.Map(apprenticeship);

            //Assert
            Assert.That(actual.ConfirmationStatus, Is.EqualTo(ConfirmationStatus.Unconfirmed));
        }

        [Test]
        public async Task And_ApprenticeshipConfirmationStatus_Is_Mapped_To_Null_When_Email_Not_Present()
        {
            //Arrange
            var apprenticeship = CreateApprenticeship();
            apprenticeship.Email = null;
            var mapper = new ApprenticeshipToApprenticeshipDetailsMapper(new CurrentDateTime());

            //Act
            var actual = await mapper.Map(apprenticeship);

            //Assert
            Assert.IsNull(actual.ConfirmationStatus);
        }

        private static Apprenticeship CreateApprenticeship()
        {
            var autoFixture = new Fixture();
            autoFixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList().ForEach(b => autoFixture.Behaviors.Remove(b));
            autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            autoFixture.Customizations.Add(
                new TypeRelay(
                    typeof(ApprenticeshipBase),
                    typeof(Apprenticeship)));
            var apprenticeship = autoFixture.Create<Apprenticeship>();
            apprenticeship.PriceHistory = new List<PriceHistory>{new PriceHistory
            {
                Cost = 3,
                ApprenticeshipId = apprenticeship.Id
            }};
            return apprenticeship;
        }
    }
}
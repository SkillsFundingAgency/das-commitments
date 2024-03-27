using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Types;


namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.ApprenticeshipStatusExtensions
{
    [Parallelizable]
    public class WhenMappingToPaymentStatus
    {

        [TestCase(ApprenticeshipStatus.WaitingToStart, PaymentStatus.Active)]
        [TestCase(ApprenticeshipStatus.Paused, PaymentStatus.Paused)]
        [TestCase(ApprenticeshipStatus.Stopped, PaymentStatus.Withdrawn)]
        [TestCase(ApprenticeshipStatus.Completed, PaymentStatus.Completed)]
        [TestCase(ApprenticeshipStatus.Live, PaymentStatus.Active)]
        public void ShouldMapToCorrectValue(ApprenticeshipStatus source, PaymentStatus target)
        {
            //Act
            var result = source.MapToPaymentStatus();

            //Assert
            Assert.That(result, Is.EqualTo(target));
        }

        [Test]
        public void Then_Throws_Exception_When_Cant_Map()
        {
            //Act Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => ((ApprenticeshipStatus)9).MapToPaymentStatus());
        }
    }
}

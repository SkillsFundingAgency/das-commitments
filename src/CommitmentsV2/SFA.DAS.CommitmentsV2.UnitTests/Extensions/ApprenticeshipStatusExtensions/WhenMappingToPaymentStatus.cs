using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Extensions;
using SFA.DAS.CommitmentsV2.Types;


namespace SFA.DAS.CommitmentsV2.UnitTests.Extensions.ApprenticeshipStatusExtensions
{
    public class WhenMappingToPaymentStatus
    {

        [TestCase(ApprenticeshipStatus.WaitingToStart, new[]{PaymentStatus.PendingApproval, PaymentStatus.Active})]
        [TestCase(ApprenticeshipStatus.Paused, new[]{PaymentStatus.Paused})]
        [TestCase(ApprenticeshipStatus.Stopped, new[]{PaymentStatus.Withdrawn})]
        [TestCase(ApprenticeshipStatus.Completed, new[]{PaymentStatus.Completed})]
        [TestCase(ApprenticeshipStatus.Live, new[]{PaymentStatus.Active, PaymentStatus.Deleted})]
        public void ShouldMapToCorrectValue(ApprenticeshipStatus source, PaymentStatus[] target)
        {
            //Act
            var result = source.MapToPaymentStatuses();

            //Assert
            Assert.AreEqual(target, result);
        }
    }
}

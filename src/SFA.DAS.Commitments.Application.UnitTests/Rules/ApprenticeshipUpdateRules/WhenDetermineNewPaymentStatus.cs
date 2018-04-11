using System;
using NUnit.Framework;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Application.UnitTests.Rules.ApprenticeshipUpdateRules
{
    [TestFixture]
    public class WhenDetermineNewPaymentStatus
    {
        private Application.Rules.ApprenticeshipUpdateRules _rules;

        [SetUp]
        public void Setup()
        {
            _rules = new Application.Rules.ApprenticeshipUpdateRules();
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.PendingApproval)]
        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        [TestCase(PaymentStatus.Deleted)]
        [TestCase(PaymentStatus.Paused)]
        public void ThenLeaveAsIsIfNoChangesThatRequirementAgreementWereMade(PaymentStatus paymentStatus)
        {
            Assert.AreEqual(paymentStatus, _rules.DetermineNewPaymentStatus(paymentStatus, false));
        }

        [TestCase(PaymentStatus.Active)]
        [TestCase(PaymentStatus.PendingApproval)]
        [TestCase(PaymentStatus.Withdrawn)]
        [TestCase(PaymentStatus.Completed)]
        [TestCase(PaymentStatus.Deleted)]
        [TestCase(PaymentStatus.Paused)]
        public void ThenSetToPendingApprovalIfChangesThatRequirementAgreementWereMade(PaymentStatus paymentStatus)
        {
            Assert.AreEqual(PaymentStatus.PendingApproval, _rules.DetermineNewPaymentStatus(paymentStatus, true));
        }
    }
}

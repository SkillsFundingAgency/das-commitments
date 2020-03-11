using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;


namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort.StateTracking
{
    [TestFixture]
    public class WhenTrackingState
    {
        [Test]
        public void LastMessageReflectsTheLastAddedMessage()
        {
            const string testMessage = "TestMessage";
            var cohort = new CommitmentsV2.Models.Cohort();
            cohort.Messages.Add(new CommitmentsV2.Models.Message { Text = testMessage });
            Assert.AreEqual(testMessage, cohort.LastMessage);
        }

        [Test]
        public void LastMessageIsNullWhenNoMessagesExist()
        {
            var cohort = new CommitmentsV2.Models.Cohort();
            Assert.AreEqual(null, cohort.LastMessage);
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(10)]
        public void DraftApprenticeshipCountReflectsTheNumberOfDraftApprenticeships(int count)
        {
            var cohort = new CommitmentsV2.Models.Cohort();
            for (var i = 0; i < count; i++)
            {
                cohort.Apprenticeships.Add(new DraftApprenticeship());
            }

            Assert.AreEqual(count, cohort.DraftApprenticeshipCount);
        }

        [TestCase(false, false, false, Party.None)]
        [TestCase(true, false, false, Party.Employer)]
        [TestCase(false, true, false, Party.Provider)]
        [TestCase(true, true, false, Party.Employer | Party.Provider)]
        [TestCase(true, true, true, Party.Employer | Party.Provider | Party.TransferSender)]
        public void PartyApprovalsReflectsTheApprovalsGiven(bool hasEmployerApproved, bool hasProviderApproved, bool hasTransferSenderApproved, Party expectedApprovals)
        {
            var cohort = new CommitmentsV2.Models.Cohort {WithParty = Party.Employer};

            if (hasEmployerApproved & hasProviderApproved)
            {
                cohort.Apprenticeships.Add(new DraftApprenticeship { AgreementStatus = AgreementStatus.BothAgreed });
            }
            else if (hasProviderApproved)
            {
                cohort.Apprenticeships.Add(new DraftApprenticeship { AgreementStatus = AgreementStatus.ProviderAgreed });
            }
            else if (hasEmployerApproved)
            {
                cohort.Apprenticeships.Add(new DraftApprenticeship { AgreementStatus = AgreementStatus.EmployerAgreed });
            }

            if (hasTransferSenderApproved)
            {
                cohort.TransferSenderId = 1;
                cohort.TransferApprovalStatus = TransferApprovalStatus.Approved;
            }
            
            Assert.AreEqual(expectedApprovals, cohort.Approvals);
        }
    }
}

using System;
using System.Threading.Tasks;
using AutoFixture;
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Services;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.Encoding;

namespace SFA.DAS.Commitments.Application.UnitTests.Services
{
    [TestFixture]
    [Parallelizable]
    public class NotificationsPublisherTests
    {
        [Test]
        public async Task ProviderAmendedCohort_PublishedSendEmailToEmployerCommand_WithCorrectTemplate()
        {
            var f = new NotificationsPublisherTestsFixtures();
            await f.Sut.ProviderAmendedCohort(f.Commitment);
            f.VerifyTemplateIsUsed(NotificationsPublisher.AmendedTemplate);
        }

        [Test]
        public async Task ProviderAmendedCohort_PublishedSendEmailToEmployerCommand_WithCorrectTokens()
        {
            var f = new NotificationsPublisherTestsFixtures().SetupEncodingService();
            await f.Sut.ProviderAmendedCohort(f.Commitment);
            f.VerifyTokensForAmendedTemplate();
        }

        [Test]
        public void ProviderAmendedCohort_ThrowsExceptionAndLogsIt_WhenPublishingFails()
        {
            var f = new NotificationsPublisherTestsFixtures().SetupMessageSessionToThrowInvalidOperationException();
            Assert.ThrowsAsync<InvalidOperationException>(() => f.Sut.ProviderAmendedCohort(f.Commitment));
            f.VerifyExceptionIsLogged<InvalidOperationException>();
        }

        [TestCase(null, NotificationsPublisher.ApprovedTemplate)]
        [TestCase(1234, NotificationsPublisher.ApprovedWithTransferTemplate)]
        public async Task ProviderApprovedCohort_PublishedSendEmailToEmployerCommand_WithCorrectTemplate(long? transferSenderId, string expectedTemplate)
        {
            var f = new NotificationsPublisherTestsFixtures().SetTransferSenderIdAndLastActionToApprove(transferSenderId);
            await f.Sut.ProviderApprovedCohort(f.Commitment);
            f.VerifyTemplateIsUsed(expectedTemplate);
        }

        [Test]
        public async Task ProviderApprovedCohort_PublishedSendEmailToEmployerCommand_WithCorrectTokens()
        {
            var f = new NotificationsPublisherTestsFixtures().SetTransferSenderIdAndLastActionToApprove(null).SetupEncodingService();
            await f.Sut.ProviderApprovedCohort(f.Commitment);
            f.VerifyCommonTokensForTemplate("approval");
        }

        [Test]
        public async Task ProviderApprovedCohort_PublishedSendEmailToEmployerCommandForTransferSender_WithCorrectTokens()
        {
            var f = new NotificationsPublisherTestsFixtures().SetTransferSenderIdAndLastActionToApprove(1234).SetupEncodingService();
            await f.Sut.ProviderApprovedCohort(f.Commitment);
            f.VerifyTokensForTransferApproval();
        }

        [Test]
        public void ProviderApprovedCohort_ThrowsExceptionAndLogsIt_WhenPublishingFails()
        {
            var f = new NotificationsPublisherTestsFixtures().SetupMessageSessionToThrowInvalidOperationException();
            Assert.ThrowsAsync<InvalidOperationException>(() => f.Sut.ProviderApprovedCohort(f.Commitment));
            f.VerifyExceptionIsLogged<InvalidOperationException>();
        }
    }

    internal class NotificationsPublisherTestsFixtures
    {
        public NotificationsPublisherTestsFixtures()
        {
            _autoFixture = new Fixture();
            EndpointInstance = new Mock<IEndpointInstance>();
            MessageSession = EndpointInstance.As<IMessageSession>();
            CommitmentsLogger = new Mock<ICommitmentsLogger>();
            EncodingService = new Mock<IEncodingService>();
            Commitment = _autoFixture.Create<Commitment>();

            Sut = new NotificationsPublisher(EndpointInstance.Object, CommitmentsLogger.Object, EncodingService.Object);
        }
        
        public Mock<IEndpointInstance> EndpointInstance { get; }
        public Mock<IMessageSession> MessageSession { get; }
        public Mock<ICommitmentsLogger> CommitmentsLogger { get; }
        public Mock<IEncodingService> EncodingService { get; }
        public Commitment Commitment { get; set; }
        public NotificationsPublisher Sut { get; }

        private Fixture _autoFixture;

        public NotificationsPublisherTestsFixtures SetupEncodingService()
        {
            EncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.AccountId)).Returns((long p, EncodingType t) => p.ToString());
            return this;
        }

        public NotificationsPublisherTestsFixtures SetupMessageSessionToThrowInvalidOperationException()
        {
            MessageSession.Setup(x => x.Send(It.IsAny<object>(), It.IsAny<SendOptions>())).Throws<InvalidOperationException>();
            return this;
        }

        public NotificationsPublisherTestsFixtures SetTransferSenderIdAndLastActionToApprove(long? transferSenderId)
        {
            Commitment.TransferSenderId = transferSenderId;
            Commitment.LastAction = LastAction.Approve;
            return this;
        }

        public void VerifyTemplateIsUsed(string template)
        {
            MessageSession.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(p => p.Template == template), It.IsAny<SendOptions>()));
        }

        public void VerifyTokensForAmendedTemplate()
        {
            VerifyCommonTokensForTemplate("review");
            MessageSession.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(p=>p.Tokens["provider_name"] == Commitment.ProviderName &&
                                                                                     p.Tokens["employer_hashed_account"] == Commitment.EmployerAccountId.ToString()),
                It.IsAny<SendOptions>()));
        }

        public void VerifyExceptionIsLogged<TException>() where TException : Exception
        {
            CommitmentsLogger.Verify(x => x.Error(It.Is<Exception>(e=> e.GetType() == typeof(TException)), It.Is<string>(s=>s.EndsWith("failed")), null, null, null, null, null));
        }

        public void VerifyTokensForTransferApproval()
        {
            VerifyCommonTokensForTemplate("approval");
            MessageSession.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(p => p.Tokens["sender_name"] == Commitment.TransferSenderName && 
                                                                                     p.Tokens["provider_name"] == Commitment.ProviderName &&
                                                                                     p.Tokens["employer_hashed_account"] == Commitment.EmployerAccountId.ToString()),
                It.IsAny<SendOptions>()));
        }

        public void VerifyCommonTokensForTemplate(string type)
        {
            MessageSession.Verify(x => x.Send(It.Is<SendEmailToEmployerCommand>(p => p.Tokens["type"] == type &&
                                                                                     p.Tokens["cohort_reference"] == Commitment.Reference),
                It.IsAny<SendOptions>()));
        }
    }
}
using Moq;
using NServiceBus;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.MessageHandlers.CommandHandlers;
using SFA.DAS.CommitmentsV2.Messages.Commands;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.MessageHandlers.UnitTests.CommandHandlers
{
    [TestFixture]
    public class ApproveTransferRequestCommandHandlerTests
    {
        private ApproveTransferRequestCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new ApproveTransferRequestCommandHandlerTestsFixture();
        }

        [Test]
        public async Task WhenCallingApproveOnTransferRequest_ThenDomainServiceIsCalled()
        {
            await _fixture.Act();

            // Assert
            _fixture.VerifyApproveTransferRequestCalled();
        }
    }

    public class ApproveTransferRequestCommandHandlerTestsFixture
    {
        private long TransferRequestId;
        private UserInfo UserInfo;
        private DateTime ApprovedOnDate;

        private Mock<IMessageHandlerContext> MockMessageHandlerContext;
        private ApproveTransferRequestCommand Command;
        private Mock<ITransferRequestDomainService> MockTransferRequestDomainService;
        private ApproveTransferRequestCommandHandler Sut;

        public ApproveTransferRequestCommandHandlerTestsFixture()
        {
            TransferRequestId = 235;

            UserInfo = new UserInfo
            {
                UserDisplayName = "TestName",
                UserEmail = "TestEmail@Test.com",
                UserId = "23432"
            };
            ApprovedOnDate = new DateTime(2020, 01, 30, 14, 22, 00);

            MockMessageHandlerContext = new Mock<IMessageHandlerContext>();
            Command = new ApproveTransferRequestCommand(TransferRequestId, ApprovedOnDate, UserInfo);
            MockTransferRequestDomainService = new Mock<ITransferRequestDomainService>();
            Sut = new ApproveTransferRequestCommandHandler(MockTransferRequestDomainService.Object);
        }

        public async Task Act() => await Sut.Handle(Command, MockMessageHandlerContext.Object);

        public void VerifyApproveTransferRequestCalled()
        {
            MockTransferRequestDomainService
                .Verify(m => m.ApproveTransferRequest(TransferRequestId, UserInfo, ApprovedOnDate, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
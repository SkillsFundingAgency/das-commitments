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
    public class RejectTransferRequestCommandHandlerTests
    {
        private RejectTransferRequestCommandHandlerTestsFixture _fixture;

        [SetUp]
        public void Setup()
        {
            _fixture = new RejectTransferRequestCommandHandlerTestsFixture();
        }

        [Test]
        public async Task WhenCallingRejectOnTransferRequest_ThenDomainServiceIsCalled()
        {
            await _fixture.Act();

            // Assert
            _fixture.VerifyRejectTransferRequestCalled();
        }
    }

    public class RejectTransferRequestCommandHandlerTestsFixture
    {
        private long TransferRequestId;
        private UserInfo UserInfo;
        private DateTime RejectedOnDate;
        
        private Mock<IMessageHandlerContext> MockMessageHandlerContext;
        private RejectTransferRequestCommand Command;
        private Mock<ITransferRequestDomainService> MockTransferRequestDomainService;
        private RejectTransferRequestCommandHandler Sut;

        public RejectTransferRequestCommandHandlerTestsFixture()
        {
            TransferRequestId = 235;
            
            UserInfo = new UserInfo
            {
                UserDisplayName = "TestName",
                UserEmail = "TestEmail@Test.com",
                UserId = "23432"
            };
            RejectedOnDate = new DateTime(2020, 01, 30, 14, 22, 00);
            
            MockMessageHandlerContext = new Mock<IMessageHandlerContext>();
            Command = new RejectTransferRequestCommand(TransferRequestId, RejectedOnDate, UserInfo);
            MockTransferRequestDomainService = new Mock<ITransferRequestDomainService>();
            Sut = new RejectTransferRequestCommandHandler(MockTransferRequestDomainService.Object);
        }

        public async Task Act() => await Sut.Handle(Command, MockMessageHandlerContext.Object);

        public void VerifyRejectTransferRequestCalled()
        {
            MockTransferRequestDomainService
                .Verify(m => m.RejectTransferRequest(TransferRequestId, UserInfo, RejectedOnDate, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
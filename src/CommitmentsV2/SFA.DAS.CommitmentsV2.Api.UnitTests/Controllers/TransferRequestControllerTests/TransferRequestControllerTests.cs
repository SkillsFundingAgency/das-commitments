using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Controllers;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateTransferApprovalForSender;
using SFA.DAS.CommitmentsV2.Application.Queries.GetTransferRequest;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;

namespace SFA.DAS.CommitmentsV2.Api.UnitTests.Controllers.TransferRequestControllerTests;

[TestFixture]
public class TransferRequestControllerTests
{
    private TransferRequestControllerTestsFixture _fixture;

    [SetUp]
    public void Arrange()
    {
        _fixture = new TransferRequestControllerTestsFixture();
    }

    [Test]
    public async Task GetTransferRequestForSender_Should_Return_Valid_Result()
    {
        await _fixture.GetTransferRequestForSender();
        _fixture.VerifyGetTransferRequestForSenderResult();
    }

    [Test]
    public async Task GetTransferRequestForSender_Should_CallMediator()
    {
        await _fixture.GetTransferRequestForSender();
        _fixture.VerifyGetTransferRequestForSenderCallsMediator();
    }

    [Test]
    public async Task GetTransferRequestForReciever_Should_Return_Valid_Result()
    {
        await _fixture.GetTransferRequestForReceiver();
        _fixture.VerifyGetTransferRequestForReceiverResult();
    }

    [Test]
    public async Task GetTransferRequestForReceiver_Should_CallMediator()
    {
        await _fixture.GetTransferRequestForReceiver();
        _fixture.VerifyGetTransferRequestForReceiverCallsMediator();
    }

    [Test]
    public async Task UpdateTransferRequestForSender_Should_CallMediator()
    {
        await _fixture.UpdateTransferApprovalForSender();
        _fixture.VerifyUpdateTransferApprovalForSenderCallsMediator();
    }

    private class TransferRequestControllerTestsFixture
    {
        private TransferRequestController Controller { get; }
        private Mock<IMediator> Mediator { get; }
        private Mock<IModelMapper> ModelMapper { get; }

        private long TransferSenderId { get; }
        private long TransferRequestId { get; }
        private long CohortId { get; }

        private GetTransferRequestQueryResult GetTransferRequestQueryResult { get; }
        private GetTransferRequestResponse GetTransferRequestResponse { get;  }

        private UpdateTransferApprovalForSenderRequest UpdateTransferApprovalForSenderRequest { get; }

        private IActionResult Result { get; set; }

        public TransferRequestControllerTestsFixture()
        {
            var autoFixture = new Fixture();

            GetTransferRequestQueryResult = autoFixture.Create<GetTransferRequestQueryResult>();
            Mediator = new Mock<IMediator>();
            Mediator.Setup(x => x.Send(It.IsAny<GetTransferRequestQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(GetTransferRequestQueryResult);

            ModelMapper = new Mock<IModelMapper>();

            GetTransferRequestResponse = autoFixture.Create<GetTransferRequestResponse>();
            ModelMapper.Setup(m => m.Map<GetTransferRequestResponse>(It.IsAny<GetTransferRequestQueryResult>()))
                .ReturnsAsync(GetTransferRequestResponse);
                
            Controller = new TransferRequestController(Mediator.Object, ModelMapper.Object);
            TransferSenderId = autoFixture.Create<long>();
            TransferRequestId = autoFixture.Create<long>();
            CohortId = autoFixture.Create<long>();

            UpdateTransferApprovalForSenderRequest = autoFixture.Create<UpdateTransferApprovalForSenderRequest>();
        }

        public async Task GetTransferRequestForSender()
        {
            Result = await Controller.GetTransferRequestForSender(TransferSenderId, TransferRequestId);
        }

        public void VerifyGetTransferRequestForSenderResult()
        {
            Assert.Multiple(() =>
            {
                Assert.That(Result.GetType(), Is.EqualTo(typeof(OkObjectResult)));
                   
                var objectResult = (OkObjectResult)Result;
                Assert.That(objectResult.StatusCode, Is.EqualTo(200));
                Assert.That(objectResult.Value, Is.InstanceOf<GetTransferRequestResponse>());
                    
                var response = (GetTransferRequestResponse) objectResult.Value;
                Assert.That(response.TransferRequestId, Is.EqualTo(GetTransferRequestResponse.TransferRequestId));
            });
        }

        public void VerifyGetTransferRequestForSenderCallsMediator()
        {
            Mediator.Verify(m => m.Send(It.Is<GetTransferRequestQuery>(p =>
                p.EmployerAccountId == TransferSenderId &&
                p.TransferRequestId == TransferRequestId &&
                p.Originator == GetTransferRequestQuery.QueryOriginator.TransferSender), It.IsAny<CancellationToken>()), Times.Once);
        }

        public async Task GetTransferRequestForReceiver()
        {
            Result = await Controller.GetTransferRequestForReceiver(TransferSenderId, TransferRequestId);
        }

        public void VerifyGetTransferRequestForReceiverResult()
        {
                
            Assert.Multiple(() =>
            {
                Assert.That(Result.GetType(), Is.EqualTo(typeof(OkObjectResult)));
                var objectResult = (OkObjectResult)Result;
                    
                Assert.That(objectResult.StatusCode, Is.EqualTo(200));
                Assert.That(objectResult.Value, Is.InstanceOf<GetTransferRequestResponse>());
                    
                var response = (GetTransferRequestResponse)objectResult.Value;
                Assert.That(response.TransferRequestId, Is.EqualTo(GetTransferRequestResponse.TransferRequestId));
            });
        }

        public void VerifyGetTransferRequestForReceiverCallsMediator()
        {
            Mediator.Verify(m => m.Send(It.Is<GetTransferRequestQuery>(p =>
                p.EmployerAccountId == TransferSenderId &&
                p.TransferRequestId == TransferRequestId &&
                p.Originator == GetTransferRequestQuery.QueryOriginator.TransferReceiver), It.IsAny<CancellationToken>()), Times.Once);
        }

        public async Task UpdateTransferApprovalForSender()
        {
            Result = await Controller.UpdateTransferApprovalForSender(TransferSenderId, TransferRequestId, CohortId, UpdateTransferApprovalForSenderRequest);
        }

        public void VerifyUpdateTransferApprovalForSenderCallsMediator()
        {
            Mediator.Verify(m => m.Send(It.Is<UpdateTransferApprovalForSenderCommand>(p =>
                p.TransferSenderId == TransferSenderId &&
                p.TransferReceiverId == UpdateTransferApprovalForSenderRequest.TransferReceiverId &&
                p.TransferRequestId == TransferRequestId &&
                p.CohortId == CohortId &&
                p.TransferApprovalStatus == UpdateTransferApprovalForSenderRequest.TransferApprovalStatus &&
                p.UserInfo == UpdateTransferApprovalForSenderRequest.UserInfo), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
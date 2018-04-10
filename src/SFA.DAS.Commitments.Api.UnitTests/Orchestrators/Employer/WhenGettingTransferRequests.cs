using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Ploeh.AutoFixture;
using SFA.DAS.Commitments.Api.Types.Commitment;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForReceiver;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForSender;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenGettingTransferRequests : EmployerOrchestratorTestBase
    {
        private List<Domain.Entities.TransferRequestSummary> _receiverMatches;
        private List<Domain.Entities.TransferRequestSummary> _senderMatches;
        private List<Types.Commitment.TransferRequestSummary> _receiverMapperMatches;
        private List<Types.Commitment.TransferRequestSummary> _senderMapperMatches;

        [SetUp]
        public new void SetUp()
        {
            var fixture = new Fixture();

            _receiverMatches = fixture.Create<List<Domain.Entities.TransferRequestSummary>>();
            _senderMatches = fixture.Create<List<Domain.Entities.TransferRequestSummary>>();

            _receiverMapperMatches = fixture.Create<List<Types.Commitment.TransferRequestSummary>>();
            _senderMapperMatches = fixture.Create<List<Types.Commitment.TransferRequestSummary>>();

            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetTransferRequestsForReceiverRequest>()))
                .ReturnsAsync(new GetTransferRequestsForReceiverResponse() { Data = _receiverMatches });

            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetTransferRequestsForSenderRequest>()))
                .ReturnsAsync(new GetTransferRequestsForSenderResponse() { Data = _senderMatches });

            MockTransferRequestMapper.Setup(x => x.MapFrom(It.IsAny<IList<Domain.Entities.TransferRequestSummary>>(), TransferType.AsReceiver))
                .Returns(_receiverMapperMatches);

            MockTransferRequestMapper.Setup(x => x.MapFrom(It.IsAny<IList<Domain.Entities.TransferRequestSummary>>(), TransferType.AsSender))
                .Returns(_senderMapperMatches);

            MockHashingService.Setup(x => x.DecodeValue(It.IsAny<string>())).Returns((string param) => Convert.ToInt64(param));
        }
    

        [Test]
        public async Task ThenRecieverCommandIsSentToMediator()
        {
            //Act
            await Orchestrator.GetTransferRequests("123");
             
            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.Is<GetTransferRequestsForReceiverRequest>(p =>
                    p.TransferReceiverAccountId == 123)), Times.Once);
        }

        [Test]
        public async Task ThenSenderCommandIsSentToMediator()
        {
            //Act
            await Orchestrator.GetTransferRequests("123");

            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.Is<GetTransferRequestsForSenderRequest>(p =>
                    p.TransferSenderAccountId == 123)), Times.Once);
        }

        [Test]
        public async Task ThenReceiverMatchesAreSentToMapper()
        {
            //Act
            await Orchestrator.GetTransferRequests("123");

            //Assert
            MockTransferRequestMapper.Verify(x=>x.MapFrom(_receiverMatches, TransferType.AsReceiver));
        }

        [Test]
        public async Task ThenSenderMatchesAreSentToMapper()
        {
            //Act
            await Orchestrator.GetTransferRequests("123");

            //Assert
            MockTransferRequestMapper.Verify(x => x.MapFrom(_senderMatches, TransferType.AsSender));
        }

        [Test]
        public async Task ThenShouldReturnAllRecievedAndSentMatches()
        {
            //Act
            var result = await Orchestrator.GetTransferRequests("123");

            result.Count.Should().Be(_receiverMapperMatches.Count + _senderMapperMatches.Count);
        }
    }
}

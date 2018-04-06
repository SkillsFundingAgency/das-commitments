using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForReceiver;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenGettingTransferRequestsForAReciever : EmployerOrchestratorTestBase
    {
        private List<Domain.Entities.TransferRequestSummary> _domainMatches;
        private List<Types.Commitment.TransferRequestSummary> _apiMatches;

        [SetUp]
        public new void SetUp()
        {
            _domainMatches = new List<Domain.Entities.TransferRequestSummary>();
            _apiMatches = new List<Types.Commitment.TransferRequestSummary>();
            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetTransferRequestsForReceiverRequest>()))
                .ReturnsAsync(new GetTransferRequestsForReceiverResponse() { Data = _domainMatches });
            MockTransferRequestMapper.Setup(x => x.MapFrom(It.IsAny<IList<Domain.Entities.TransferRequestSummary>>()))
                .Returns(_apiMatches);
            MockHashingService.Setup(x => x.DecodeValue(It.IsAny<string>())).Returns((string param) => Convert.ToInt64(param));
        }
    

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            //Act
            await Orchestrator.GetTransferRequestsForReceiver("123");
             
            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.Is<GetTransferRequestsForReceiverRequest>(p =>
                    p.TransferReceiverAccountId == 123)), Times.Once);
        }

        [Test]
        public async Task ThenDomainMatchesAreSentToMapper()
        {
            //Act
            await Orchestrator.GetTransferRequestsForReceiver("123");

            //Assert
            MockTransferRequestMapper.Verify(x=>x.MapFrom(_domainMatches));
        }

        [Test]
        public async Task ThenShouldReturnFoundMatches()
        {
            //Act
            var result = await Orchestrator.GetTransferRequestsForReceiver("123");
            
            // Assert
            Assert.AreEqual(_apiMatches, result);
        }
    }
}

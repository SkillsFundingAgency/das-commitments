using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequest;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenGettingATransferRequest : EmployerOrchestratorTestBase
    {
        private Domain.Entities.TransferRequest _domainTransferRequest;
        private Types.Commitment.TransferRequest _apiTransferRequest;

        [SetUp]
        public new void SetUp()
        {
            _domainTransferRequest = new Domain.Entities.TransferRequest();
            _apiTransferRequest = new Types.Commitment.TransferRequest();
            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetTransferRequestRequest>()))
                .ReturnsAsync(new GetTransferRequestResponse { Data =  _domainTransferRequest });
            MockTransferRequestMapper.Setup(x => x.MapFrom(It.IsAny<Domain.Entities.TransferRequest>()))
                .Returns(_apiTransferRequest);
        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            //Act
            await Orchestrator.GetTransferRequest(123, 1000, CallerType.TransferReceiver);
             
            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.Is<GetTransferRequestRequest>(p =>
                    p.TransferRequestId == 123 && p.Caller.Id == 1000 && p.Caller.CallerType == CallerType.TransferReceiver)), Times.Once);
        }

        [Test]
        public async Task ThenDomainObjectIsSentToMapper()
        {
            //Act
            await Orchestrator.GetTransferRequest(123, 1000, CallerType.TransferReceiver);

            //Assert
            MockTransferRequestMapper.Verify(x=>x.MapFrom(_domainTransferRequest));
        }

        [Test]
        public async Task ThenShouldReturnApiObject()
        {
            //Act
            var result = await Orchestrator.GetTransferRequest(123, 1000, CallerType.TransferReceiver);

            // Assert
            Assert.AreEqual(_apiTransferRequest, result);
        }
    }
}

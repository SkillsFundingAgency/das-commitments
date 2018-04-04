using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequestsForSender;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenGettingTransferRequestsForASender : EmployerOrchestratorTestBase
    {
        private List<Domain.Entities.TransferRequestSummary> _domainMatches;
        private List<Types.Commitment.TransferRequestSummary> _apiMatches;

        [SetUp]
        public new void SetUp()
        {
            _domainMatches = new List<Domain.Entities.TransferRequestSummary>();
            _apiMatches = new List<Types.Commitment.TransferRequestSummary>();
            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetTransferRequestsForSenderRequest>()))
                .ReturnsAsync(new GetTransferRequestsForSenderResponse { Data = _domainMatches });
            MockTransferRequestMapper.Setup(x => x.MapFrom(It.IsAny<IList<Domain.Entities.TransferRequestSummary>>()))
                .Returns(_apiMatches);
        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            //Act
            await Orchestrator.GetTransferRequestsForSender(123);
             
            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.Is<GetTransferRequestsForSenderRequest>(p =>
                    p.TransferSenderAccountId == 123)), Times.Once);
        }

        [Test]
        public async Task ThenDomainMatchesAreSentToMapper()
        {
            //Act
            await Orchestrator.GetTransferRequestsForSender(123);

            //Assert
            MockTransferRequestMapper.Verify(x => x.MapFrom(_domainMatches));
        }
        [Test]
        public async Task ThenShouldReturnFoundMatches()
        {
            //Act
            var result = await Orchestrator.GetTransferRequestsForSender(123);

            Assert.AreEqual(_domainMatches, result);
        }
    }
}

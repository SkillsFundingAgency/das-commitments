using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetEmployerAccountIds;
using SFA.DAS.Commitments.Application.Queries.GetTransferRequest;
using SFA.DAS.Commitments.Domain;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Employer
{
    [TestFixture]
    public class WhenGettingEmployerAccountIds : EmployerOrchestratorTestBase
    {
        private List<long> _employerAccountIds;

        [SetUp]
        public new void SetUp()
        {
            _employerAccountIds = new List<long>();

            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetEmployerAccountIdsRequest>()))
                .ReturnsAsync(new GetEmployerAccountIdsResponse() { Data = _employerAccountIds });
        }

        [Test]
        public async Task ThenAppropriateCommandIsSentToMediator()
        {
            //Act
            await Orchestrator.GetEmployerAccountIds();
             
            //Assert
            MockMediator.Verify(
                x => x.SendAsync(It.IsAny<GetEmployerAccountIdsRequest>()), Times.Once);
        }

        [Test]
        public async Task ThenShouldReturnApiObject()
        {
            //Act
            var result = await Orchestrator.GetEmployerAccountIds();

            // Assert
            Assert.AreEqual(_employerAccountIds, result);
        }
    }
}

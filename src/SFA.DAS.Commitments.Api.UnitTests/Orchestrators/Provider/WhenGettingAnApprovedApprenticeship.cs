using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using SFA.DAS.Commitments.Application.Queries.GetApprovedApprenticeship;
using SFA.DAS.Commitments.Domain;
using SFA.DAS.Commitments.Domain.Entities.ApprovedApprenticeship;

namespace SFA.DAS.Commitments.Api.UnitTests.Orchestrators.Provider
{
    [TestFixture]
    public class WhenGettingAnApprovedApprenticeship : ProviderOrchestratorTestBase
    {
        private GetApprovedApprenticeshipResponse _mediatorResponse;
        private Api.Types.ApprovedApprenticeship.ApprovedApprenticeship _mapperResult;

        public override void SetUp()
        {
            base.SetUp();

            //Arrange
            _mediatorResponse = new GetApprovedApprenticeshipResponse
            {
                Data = new ApprovedApprenticeship()
            };

            _mapperResult = new Api.Types.ApprovedApprenticeship.ApprovedApprenticeship();

            MockMediator.Setup(x => x.SendAsync(It.IsAny<GetApprovedApprenticeshipRequest>())).ReturnsAsync(_mediatorResponse);

            MockApprovedApprenticeshipMapper.Setup(x => x.Map(It.IsAny<ApprovedApprenticeship>())).Returns(_mapperResult);
        }

        [Test]
        public async Task ThenTheDomainObjectIsMapped()
        {
            //Act
            await Orchestrator.GetApprovedApprenticeship(1, 2);

            //Assert
            MockApprovedApprenticeshipMapper.Verify(x => x.Map(It.Is<ApprovedApprenticeship>(a => a == _mediatorResponse.Data)));
        }

        [Test]
        public async Task ThenTheMapperResultIsReturned()
        {
            //Act
            var result = await Orchestrator.GetApprovedApprenticeship(1, 2);

            //Assert
            Assert.AreEqual(_mapperResult, result);
        }

        [Test]
        public async Task ThenTheCallerIsTheProvider()
        {
            //Act
            await Orchestrator.GetApprovedApprenticeship(1, 2);

            //Assert
            MockMediator.Verify(x =>
                x.SendAsync(It.Is<GetApprovedApprenticeshipRequest>(r =>
                    r.Caller.CallerType == CallerType.Provider && r.Caller.Id == 1)));
        }
    }
}

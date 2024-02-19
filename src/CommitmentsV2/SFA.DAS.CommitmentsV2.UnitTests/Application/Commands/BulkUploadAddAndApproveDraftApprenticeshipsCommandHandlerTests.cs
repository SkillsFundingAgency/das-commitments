using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddAndApproveDraftApprenticeships;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    public class BulkUploadAddAndApproveDraftApprenticeshipsCommandHandlerTests
    {
        [Test]
        public async Task BulkUploadAddDraftApprenticeshipsCommandIsCalled()
        {
            //Arrange
            var fixture = new BulkUploadAddAndApproveDraftApprenticeshipsCommandHandlerTestsFixture();
            
            //Act
            await fixture.Handle();

            //Assert
            fixture.VerifyBulkUploadAddDraftApprenticeshipsCommandIsSentCorrectly();
        }
    }

    public class BulkUploadAddAndApproveDraftApprenticeshipsCommandHandlerTestsFixture 
    {
        public Fixture AutoFixture { get; set; }
        public Mock<ICohortDomainService> CohortDomainService { get; set; }
        public IRequestHandler<BulkUploadAddAndApproveDraftApprenticeshipsCommand, BulkUploadAddAndApproveDraftApprenticeshipsResponse> Handler { get; set; }
        public BulkUploadAddAndApproveDraftApprenticeshipsCommand Command { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public Mock<IModelMapper> ModelMapper { get; }
        public Mock<IMediator> Mediator { get; }
        public Mock<IProviderCommitmentsDbContext> DbContext { get; set; }
       
        public BulkUploadAddAndApproveDraftApprenticeshipsCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            CohortDomainService = new Mock<ICohortDomainService>();
            ModelMapper = new Mock<IModelMapper>();
            Mediator = new Mock<IMediator>();
            Command = AutoFixture.Create<BulkUploadAddAndApproveDraftApprenticeshipsCommand>();
            DbContext = new Mock<IProviderCommitmentsDbContext>();

            GetBulkUploadAddDraftApprenticeshipsResponse response = DraftApprenticeshipsResponse();
           
            Mediator.Setup(x => x.Send(It.IsAny<BulkUploadAddDraftApprenticeshipsCommand>(), It.IsAny<CancellationToken>()))
                 .ReturnsAsync(response);

            DbContext.Setup(x => x.Cohorts).ReturnsDbSet(new List<Cohort>()
            {
                new Cohort() { Reference = "MKRK7V" },
                new Cohort() { Reference = "MKRK7N" }
            });

            CohortDomainService.Setup(x => x.ApproveCohort(It.IsAny<long>(), It.IsAny<string>(), It.IsAny<UserInfo>(), It.IsAny<Party>(), It.IsAny<CancellationToken>()));                

            Handler = new BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler(Mock.Of<ILogger<BulkUploadAddAndApproveDraftApprenticeshipsCommandHandler>>(),
                ModelMapper.Object, CohortDomainService.Object, Mediator.Object, DbContext.Object);
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken);
        }

        internal void VerifyBulkUploadAddDraftApprenticeshipsCommandIsSentCorrectly()
        {
            Mediator.Verify(m => m.Send(
              It.Is<BulkUploadAddDraftApprenticeshipsCommand>(r =>
                  r.ProviderId.Equals(Command.ProviderId) && r.LogId.Equals(Command.LogId) && r.ProviderAction == "SaveAndApprove"),
              It.IsAny<CancellationToken>()), Times.Once);
        }

        private static GetBulkUploadAddDraftApprenticeshipsResponse DraftApprenticeshipsResponse()
        {
            return new GetBulkUploadAddDraftApprenticeshipsResponse()
            {
                BulkUploadAddDraftApprenticeshipsResponse = new List<BulkUploadAddDraftApprenticeshipsResponse>()
                   {
                       new BulkUploadAddDraftApprenticeshipsResponse()
                       {
                           CohortReference = "MKRK7V",
                           EmployerName = "Tesco",
                           NumberOfApprenticeships = 1
                       },
                      new BulkUploadAddDraftApprenticeshipsResponse()
                      {
                           CohortReference = "MKRK7V",
                           EmployerName = "Tesco",
                           NumberOfApprenticeships = 1
                      },
                      new BulkUploadAddDraftApprenticeshipsResponse()
                      {
                           CohortReference = "MKRK7N",
                           EmployerName = "Nasdaq",
                           NumberOfApprenticeships = 1
                      },
                   }
            };
        }

    }
}

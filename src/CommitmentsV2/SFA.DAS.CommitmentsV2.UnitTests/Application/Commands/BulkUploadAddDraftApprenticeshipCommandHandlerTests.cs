using AutoFixture;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadAddDraftApprenticeships;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Shared.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Reservations.Api.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.TestHelpers.DatabaseMock;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.Builders;
using SFA.DAS.CommitmentsV2.UnitTests.Mapping.BulkUpload;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    public class BulkUploadAddDraftApprenticeshipCommandHandlerTests
    {
        [Test]
        public async Task DraftApprenticeshipDetailMapperIsCalled()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifyMapperIsCalled();
        }

        [Test]
        public async Task DraftApprenticeshipDetailAreAdded()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifyDraftApprenticeshipsAreAdded();
        }

        [Test]
        public async Task GetCohortDetailsForAddedDraftApprenticeshipDetail()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifyGetCohortDetails();
        }
    }

    public class BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture
    {
        public Fixture AutoFixture { get; set; }
        public Mock<ICohortDomainService> CohortDomainService { get; set; }
        public IRequestHandler<BulkUploadAddDraftApprenticeshipsCommand, GetBulkUploadAddDraftApprenticeshipsResponse> Handler { get; set; }     
        public BulkUploadAddDraftApprenticeshipsCommand Command { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public Mock<IReservationsApiClient> ReservationApiClient { get; set; }
        public Mock<IModelMapper> ModelMapper { get; }
        public Mock<IProviderCommitmentsDbContext> DbContext { get; set; }
        public List<DraftApprenticeshipDetails> DraftApprenticeshipDetails { get; set; }
        public AccountLegalEntity AccountLegalEntity { get; set; }

        public BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            AutoFixture.Customizations.Add(new BulkUploadAddDraftApprenticeshipRequestSpecimenBuilder("12", 1));
            CohortDomainService = new Mock<ICohortDomainService>();
            ReservationApiClient = new Mock<IReservationsApiClient>();
            ModelMapper = new Mock<IModelMapper>();
            Command = AutoFixture.Create<BulkUploadAddDraftApprenticeshipsCommand>();
            DbContext = new Mock<IProviderCommitmentsDbContext>();

            var Account = new Account()
                 .Set(x => x.Id, 1)
                 .Set(x => x.Name, "Test Employer");

            AccountLegalEntity = new AccountLegalEntity()
            .Set(x => x.Id, 1)
            .Set(x => x.LegalEntityId, "12")
            .Set(x => x.Account, Account)
            .Set(x => x.AccountId, 1);

            List<AccountLegalEntity> apprenticeships = new List<AccountLegalEntity>()
            {
              AccountLegalEntity
            };

            DbContext.Setup(x => x.AccountLegalEntities).ReturnsDbSet(apprenticeships);

            DraftApprenticeshipDetails = AutoFixture.Create<List<DraftApprenticeshipDetails>>();
            DraftApprenticeshipDetails = DraftApprenticeshipDetails.Zip(Command.BulkUploadDraftApprenticeships, (x, y) => { x.Uln = y.Uln; return x; }).ToList();

            ModelMapper.Setup(x => x.Map<List<DraftApprenticeshipDetails>>(It.IsAny<BulkUploadAddDraftApprenticeshipsCommand>())).ReturnsAsync(() => DraftApprenticeshipDetails);
            CohortDomainService.Setup(x => x.AddDraftApprenticeship(It.IsAny<long>(), It.IsAny<long>(), It.IsAny<DraftApprenticeshipDetails>(), It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()));
            CohortDomainService.Setup(x => x.GetCohortDetails(It.IsAny<long>(), It.IsAny<CancellationToken>()));

            Handler = new BulkUploadAddDraftApprenticeshipCommandHandler(Mock.Of<ILogger<BulkUploadAddDraftApprenticeshipCommandHandler>>(), ModelMapper.Object, CohortDomainService.Object, Mock.Of<IMediator>(), DbContext.Object);
            CancellationToken = new CancellationToken();
        }

        public Task Handle()
        {
            return Handler.Handle(Command, CancellationToken);
        }

        internal void VerifyMapperIsCalled()
        {
            ModelMapper.Verify(x => x.Map<List<DraftApprenticeshipDetails>>(Command), Times.Once);
        }

        internal void VerifyDraftApprenticeshipsAreAdded()
        {
            foreach (var draftApp in DraftApprenticeshipDetails)
            {
                CohortDomainService.Verify(x => x.AddDraftApprenticeship(Command.ProviderId, It.IsAny<long>(), draftApp, Command.UserInfo, It.IsAny<CancellationToken>()), Times.Once);
            }
        }

        internal void VerifyGetCohortDetails()
        {
            foreach (var draftApp in DraftApprenticeshipDetails)
            {
                CohortDomainService.Verify(x => x.GetCohortDetails(It.IsAny<long>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
            }
        }
    }
}

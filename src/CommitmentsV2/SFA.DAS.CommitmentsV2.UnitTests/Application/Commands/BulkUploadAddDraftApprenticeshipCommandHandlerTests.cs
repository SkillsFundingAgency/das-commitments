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
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Testing.Builders;
using SFA.DAS.CommitmentsV2.UnitTests.Mapping.BulkUpload;
using SFA.DAS.Encoding;
using Microsoft.EntityFrameworkCore;
using System;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    public class BulkUploadAddDraftApprenticeshipCommandHandlerTests
    {
        [Test]
        public async Task DraftApprenticeshipDetailMapperIsCalled()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture();
            await fixture.WithData(1, "COHROT").Handle();

            fixture.VerifyMapperIsCalled();
        }

        [Test]
        public async Task DraftApprenticeshipDetailAreAdded()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture();
            await fixture.WithData(1, "COHROT").Handle();

            fixture.VerifyDraftApprenticeshipsAreAdded();
        }

        [Test]
        public async Task VerifyCohortRefUpdatedForNewlyCreatedCohort()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture();
            await fixture.WithData(0, string.Empty).Handle();

            fixture.VerifyCohortRefUpdated();
        }

        [Test]
        public async Task VerifyCohortRefNotUpdateForExistingCohort()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture();
            await fixture.WithData(1, "PPPP").Handle();

            fixture.VerifyCohortRefNotUpdated();
        }

        [Test]
        public async Task VeriyfResponse()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture();
            var bulkUploadResponse = await fixture
                .WithData(1, "PPPP", "EmployerExistingCohort", "EXISTING","Existing legal entity") // Existing cohort
                .WithData(0,string.Empty, "EmployerNewCohort", "NEW", "New Cohort legal entity") // new cohort
                .Handle();

            fixture.VerifyResponse(bulkUploadResponse);
        }
    }

    public class BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture
    {
        public int IdGenerator { get; set; }
        public Fixture AutoFixture { get; set; }
        public Mock<ICohortDomainService> CohortDomainService { get; set; }
        public IRequestHandler<BulkUploadAddDraftApprenticeshipsCommand, GetBulkUploadAddDraftApprenticeshipsResponse> Handler { get; set; }     
        public BulkUploadAddDraftApprenticeshipsCommand Command { get; set; }
        public CancellationToken CancellationToken { get; set; }
        public Mock<IReservationsApiClient> ReservationApiClient { get; set; }
        public Mock<IModelMapper> ModelMapper { get; }
        public ProviderCommitmentsDbContext DbContext { get; set; }
        public List<DraftApprenticeshipDetails> DraftApprenticeshipDetails { get; set; }
        public Mock<IEncodingService> EncodingService { get; set; }

        public BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            AutoFixture.Customizations.Add(new BulkUploadAddDraftApprenticeshipRequestSpecimenBuilder("XEGFX", 1));
            CohortDomainService = new Mock<ICohortDomainService>();
            ReservationApiClient = new Mock<IReservationsApiClient>();
            ModelMapper = new Mock<IModelMapper>();
            Command = AutoFixture.Create<BulkUploadAddDraftApprenticeshipsCommand>();
            DbContext = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                                 .Options);

            EncodingService = new Mock<IEncodingService>();
            EncodingService.Setup(x => x.Encode(It.IsAny<long>(), EncodingType.CohortReference)).Returns(() => "COHORTREF");

            DraftApprenticeshipDetails = AutoFixture.Create<List<DraftApprenticeshipDetails>>();
            DraftApprenticeshipDetails = DraftApprenticeshipDetails.Zip(Command.BulkUploadDraftApprenticeships, (x, y) => { x.Uln = y.Uln; return x; }).ToList();

            ModelMapper.Setup(x => x.Map<List<DraftApprenticeshipDetails>>(It.IsAny<BulkUploadAddDraftApprenticeshipsCommand>())).ReturnsAsync(() => DraftApprenticeshipDetails);
            CohortDomainService.Setup(x => x.AddDraftApprenticeships(It.IsAny<List<DraftApprenticeshipDetails>>(), It.IsAny<List<BulkUploadAddDraftApprenticeshipRequest>>(), It.IsAny<long>(), It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => DbContext.Cohorts.Include(x => x.Apprenticeships).Include(x => x.AccountLegalEntity).Select(x => x));

            Handler = new BulkUploadAddDraftApprenticeshipCommandHandler(Mock.Of<ILogger<BulkUploadAddDraftApprenticeshipCommandHandler>>(), ModelMapper.Object, CohortDomainService.Object, DbContext, EncodingService.Object, Mock.Of<IMediator>());
            CancellationToken = new CancellationToken();
        }

        public BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture WithData(long cohortId, string cohortRef,string accountName = "Test Employer", string legalEntityId = "XEGFX", string legalEntityName = "Legal Entity Name")
        {
            var Account = new Account()
             .Set(x => x.Id, ++IdGenerator)
             .Set(x => x.Name, accountName );

            var AccountLegalEntity = new AccountLegalEntity()
            .Set(x => x.LegalEntityId, legalEntityId)
            .Set(x => x.Name, legalEntityName)
            .Set(x => x.Id, ++IdGenerator)
            .Set(x => x.Account, Account);

            var cohort = new Cohort()
              .Set(c => c.Id, cohortId)
              .Set(c => c.Reference, cohortRef)
              .Set(c => c.ProviderId, 333)
              .Set(c => c.AccountLegalEntity, AccountLegalEntity);

            DbContext.Apprenticeships.Add(GenerateApprenticeshipDetails(cohort));
            DbContext.Apprenticeships.Add(GenerateApprenticeshipDetails(cohort));

            DbContext.SaveChanges();

            return this;
        }

        private Apprenticeship GenerateApprenticeshipDetails(Cohort cohort)
        {
            var ApprenticeshipDetails1 = AutoFixture.Build<Apprenticeship>()
           .With(s => s.Cohort, cohort)
           .With(s => s.EndDate, DateTime.UtcNow)
           .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
           .Without(s => s.PriceHistory)
           .Without(s => s.ApprenticeshipUpdate)
           .Without(s => s.DataLockStatus)
           .Without(s => s.EpaOrg)
           .Without(s => s.Continuation)
           .Without(s => s.PreviousApprenticeship)
           .Without(s => s.EmailAddressConfirmed)
           .Without(s => s.ApprenticeshipConfirmationStatus)
           .Create();
            return ApprenticeshipDetails1;
        }

        public Task<GetBulkUploadAddDraftApprenticeshipsResponse> Handle()
        {
            return Handler.Handle(Command, CancellationToken);
        }

        internal void VerifyMapperIsCalled()
        {
            ModelMapper.Verify(x => x.Map<List<DraftApprenticeshipDetails>>(Command), Times.Once);
        }

        internal void VerifyDraftApprenticeshipsAreAdded()
        {
            CohortDomainService.Verify(x => x.AddDraftApprenticeships(It.IsAny<List<DraftApprenticeshipDetails>>(), It.IsAny<List<BulkUploadAddDraftApprenticeshipRequest>>(), It.IsAny<long>(), It.IsAny<UserInfo>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        internal void VerifyCohortRefUpdated()
        {
            EncodingService.Verify(x => x.Encode(It.IsAny<long>(), EncodingType.CohortReference), Times.Once);
        }

        internal void VerifyCohortRefNotUpdated()
        {
            EncodingService.Verify(x => x.Encode(It.IsAny<long>(), EncodingType.CohortReference), Times.Never);
        }

        internal void VerifyResponse(GetBulkUploadAddDraftApprenticeshipsResponse bulkUploadResponse)
        {
            var firstCohort = bulkUploadResponse.BulkUploadAddDraftApprenticeshipsResponse.First();
            Assert.AreEqual("PPPP", firstCohort.CohortReference);
            Assert.AreEqual("Existing legal entity", firstCohort.EmployerName);
            Assert.AreEqual(2, firstCohort.NumberOfApprenticeships);

            var secondCohort = bulkUploadResponse.BulkUploadAddDraftApprenticeshipsResponse.Last();
            Assert.AreEqual("COHORTREF", secondCohort.CohortReference);
            Assert.AreEqual("New Cohort legal entity", secondCohort.EmployerName);
            Assert.AreEqual(2, secondCohort.NumberOfApprenticeships);
        }
    }
}

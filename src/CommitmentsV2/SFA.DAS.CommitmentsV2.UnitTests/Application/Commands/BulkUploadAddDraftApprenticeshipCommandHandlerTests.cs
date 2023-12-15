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
using SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest;

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

        [Test]
        public async Task VerifyFileUploadLog_IsCompletedWithActionAndDate()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture();
            await fixture.WithLogId(8787).AddFileUploadLogToDb().Handle();

            fixture.VerifyFileUploadLogWasSavedCorrectly();
        }

        [Test]
        public async Task VerifyRecordSaveActionForFileUploadIsNotCalledWhenNoLogId()
        {
            var fixture = new BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture();
            await fixture.Handle();

            Assert.IsFalse(fixture.DbContext.FileUploadLogs.Any()); ;
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
        public Mock<IMediator> MediatorService { get; set; }

        public BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture()
        {
            AutoFixture = new Fixture();
            AutoFixture.Customizations.Add(new BulkUploadAddDraftApprenticeshipRequestSpecimenBuilder("XEGFX", 1));
            CohortDomainService = new Mock<ICohortDomainService>();
            ReservationApiClient = new Mock<IReservationsApiClient>();
            ModelMapper = new Mock<IModelMapper>();
            Command = AutoFixture.Build<BulkUploadAddDraftApprenticeshipsCommand>().Without(x=>x.LogId).Create();
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

            MediatorService = new Mock<IMediator>();
            MediatorService.Setup(x => x.Send(It.IsAny<BulkUploadValidateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new BulkUploadValidateApiResponse { BulkUploadValidationErrors = new List<BulkUploadValidationError>() });
            Handler = new BulkUploadAddDraftApprenticeshipCommandHandler(Mock.Of<ILogger<BulkUploadAddDraftApprenticeshipCommandHandler>>(), ModelMapper.Object, CohortDomainService.Object, DbContext, EncodingService.Object, MediatorService.Object);
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

        internal BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture WithLogId(long n)
        {
            Command.LogId = n;
            return this;
        }

        internal BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture AddFileUploadLogToDb()
        {
            DbContext.FileUploadLogs.Add(new FileUploadLog {Id = Command.LogId.Value});
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

        internal void VerifyFileUploadLogWasSavedCorrectly()
        {
            var log = DbContext.FileUploadLogs.FirstOrDefault(x => x.Id.Equals(Command.LogId.Value));
            Assert.That(log, Is.Not.Null);
            Assert.That(log.ProviderAction, Is.EqualTo(Command.ProviderAction));
            Assert.That(log.CreatedOn, Is.Not.Null);
            Assert.That(log.CohortLogs.Count, Is.EqualTo(DbContext.Cohorts.Count()));
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
            Assert.That(firstCohort.CohortReference, Is.EqualTo("PPPP"));
            Assert.That(firstCohort.EmployerName, Is.EqualTo("Existing legal entity"));
            Assert.That(firstCohort.NumberOfApprenticeships, Is.EqualTo(2));

            var secondCohort = bulkUploadResponse.BulkUploadAddDraftApprenticeshipsResponse.Last();
            Assert.That(secondCohort.CohortReference, Is.EqualTo("COHORTREF"));
            Assert.That(secondCohort.EmployerName, Is.EqualTo("New Cohort legal entity"));
            Assert.That(secondCohort.NumberOfApprenticeships, Is.EqualTo(2));
        }

        internal BulkUploadAddDraftApprenticeshipCommandHandlerTestsFixture WithValidationErrors()
        {
            MediatorService.Setup(x => x.Send(It.IsAny<BulkUploadValidateCommand>(), It.IsAny<CancellationToken>())).ReturnsAsync(() => new BulkUploadValidateApiResponse
            {
                BulkUploadValidationErrors = new List<BulkUploadValidationError>()
                {
                    new BulkUploadValidationError(1, "PPPP", "12343343", "aappp aabbb", new List<Error>{ new Error("uln", "this is error")})
                }
            });
            return this;
        }
    }
}

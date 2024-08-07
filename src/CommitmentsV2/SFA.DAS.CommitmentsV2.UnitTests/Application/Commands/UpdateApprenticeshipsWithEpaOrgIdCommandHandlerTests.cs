﻿using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateApprenticeshipsWithEpaOrgId;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi.Types;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    public class UpdateApprenticeshipsWithEpaOrgIdCommandHandlerTests
    {
        [Test]
        public async Task EpaoOrgId_Are_Updated()
        {
            using var fixture = new UpdateCacheOfAssessmentOrganisationsCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifyEpaoOrgIdUpdated();
        }

        public class UpdateCacheOfAssessmentOrganisationsCommandHandlerTestsFixture : IDisposable
        {
            private readonly long ApprenticeshipId;
            private ProviderCommitmentsDbContext _db { get; set; }
            private UpdateApprenticeshipsWithEpaOrgIdCommandHandler _sut { get; set; }
            private UpdateApprenticeshipsWithEpaOrgIdCommand _command;

            public UpdateCacheOfAssessmentOrganisationsCommandHandlerTestsFixture()
            {
                ApprenticeshipId = 10;

                var submissionEvents = new List<SubmissionEvent>
                {
                    new SubmissionEvent{ ApprenticeshipId = ApprenticeshipId, EPAOrgId = "22" }
                };
                _command = new UpdateApprenticeshipsWithEpaOrgIdCommand(submissionEvents);

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

                _sut = new UpdateApprenticeshipsWithEpaOrgIdCommandHandler(new Lazy<ProviderCommitmentsDbContext>(() => _db), Mock.Of<ILogger<UpdateApprenticeshipsWithEpaOrgIdCommandHandler>>());
                SeedData();
            }

            private void SeedData()
            {
                var fixture = new Fixture();
                fixture.Behaviors.Add(new OmitOnRecursionBehavior());

                var cohort = new CommitmentsV2.Models.Cohort()
                    .Set(c => c.Id, 111)
                    .Set(c => c.EmployerAccountId, 222)
                    .Set(c => c.ProviderId, 333)
                    .Set(c => c.AccountLegalEntity, new AccountLegalEntity());

                var priceHistory = new List<PriceHistory>()
                {
                    new PriceHistory
                    {
                        FromDate = DateTime.Now,
                        ToDate = null,
                        Cost = 10000,
                    }
                };

                var apprenticeshipDetails = fixture.Build<CommitmentsV2.Models.Apprenticeship>()
                 .With(s => s.Id, ApprenticeshipId)
                 .With(s => s.Cohort, cohort)
                 .With(s => s.PaymentStatus, PaymentStatus.Active)
                 .With(s => s.EndDate, DateTime.UtcNow.AddDays(10))
                 .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
                 .With(s => s.PriceHistory, priceHistory)
                 .Without(s => s.ApprenticeshipUpdate)
                 .Without(s => s.DataLockStatus)
                 .Without(s => s.EpaOrg)
                 .Without(s => s.Continuation)
                 .Without(s => s.PreviousApprenticeship)
                 .Without(s => s.EmailAddressConfirmed)
                 .Without(s => s.ApprenticeshipConfirmationStatus)
                 .Create();

                _db.Apprenticeships.Add(apprenticeshipDetails);
                _db.SaveChanges();
            }

            public async Task Handle()
            {
                await _sut.Handle(_command, CancellationToken.None);
            }

            internal void VerifyEpaoOrgIdUpdated()
            {
                var apprenticeship = _db.Apprenticeships.FirstOrDefault(x => x.Id == ApprenticeshipId);
                Assert.That(apprenticeship.EpaOrgId, Is.EqualTo(_command.SubmissionEvents.First().EPAOrgId));
            }

            public void Dispose()
            {
                _db?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}

﻿using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateCacheOfAssessmentOrganisations;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    public class UpdateCacheOfAssessmentOrganisationsCommandHandlerTests
    {
        [Test]
        public async Task Verify_All_EaposOrganisations_Added_To_Db()
        {
            using var fixture = new UpdateCacheOfAssessmentOrganisationsCommandHandlerTestsFixture();
            await fixture.Handle();

            fixture.VerifyOrganisationsAdded();
        }

        private class UpdateCacheOfAssessmentOrganisationsCommandHandlerTestsFixture : IDisposable
        {
            private ProviderCommitmentsDbContext _db { get; set; }
            private UpdateCacheOfAssessmentOrganisationsCommandHandler _sut { get; set; }
            private UpdateCacheOfAssessmentOrganisationsCommand _command;
            private Mock<IApprovalsOuterApiClient> _approvalOuterApi;
            private EpaoResponse _epaoResponse;

            public UpdateCacheOfAssessmentOrganisationsCommandHandlerTestsFixture()
            {
                _epaoResponse = new EpaoResponse();
                _epaoResponse.Epaos = new List<OrganisationSummary>()
                {
                    new OrganisationSummary() { Id = "1", Name = "11" },
                    new OrganisationSummary() { Id = "2", Name = "22" },
                    new OrganisationSummary() { Id = "3", Name = "33" },
                };

                _approvalOuterApi = new Mock<IApprovalsOuterApiClient>();
                _approvalOuterApi.Setup(x => x.Get<EpaoResponse>(It.IsAny<GetEpaoOrganisationsRequest>())).ReturnsAsync(_epaoResponse);

                _command = new UpdateCacheOfAssessmentOrganisationsCommand();

                _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

                _sut = new UpdateCacheOfAssessmentOrganisationsCommandHandler(_approvalOuterApi.Object, new Lazy<ProviderCommitmentsDbContext>(() => _db), Mock.Of<ILogger<UpdateCacheOfAssessmentOrganisationsCommandHandler>>());
            }

            public async Task Handle()
            {
                await _sut.Handle(_command, CancellationToken.None);
            }

            internal void VerifyOrganisationsAdded()
            {
                foreach (var org in _epaoResponse.Epaos)
                {
                    var organisatinInDb = _db.AssessmentOrganisations.FirstOrDefault(x => x.EpaOrgId == org.Id && x.Name == org.Name);
                    Assert.That(organisatinInDb, Is.Not.Null);
                }
            }

            public void Dispose()
            {
                _db?.Dispose();
                GC.SuppressFinalize(this);
            }
        }
    }
}

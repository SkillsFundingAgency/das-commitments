using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Application.Commands.UpdateCacheOfAssessmentOrganisations;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities.AddEpaToApprenticeship;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Models.ApprovalsOuterApi;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
public class UpdateCacheOfAssessmentOrganisationsCommandHandlerTests
{
    [Test]
    public async Task Verify_All_EaposOrganisations_Are_Added_When_There_Are_No_Existing_Records()
    {
        using var fixture = new UpdateCacheOfAssessmentOrganisationsCommandHandlerTestsFixture();
        await fixture.Handle();

        fixture.VerifyOrganisationsAdded();
    }

    [Test]
    public async Task Verify_New_EaposOrganisations_Are_Added_When_There_Are_Existing_Record()
    {
        using var fixture = new UpdateCacheOfAssessmentOrganisationsCommandHandlerTestsFixture();
        await fixture.AddExistingEpaosToDatabase();
        fixture.AddNewOrgToResponse();
        
        await fixture.Handle();

        fixture.VerifyOrganisationsUpdated();
    }

    private class UpdateCacheOfAssessmentOrganisationsCommandHandlerTestsFixture : IDisposable
    {
        private readonly ProviderCommitmentsDbContext _db;
        private readonly UpdateCacheOfAssessmentOrganisationsCommandHandler _sut;
        private readonly UpdateCacheOfAssessmentOrganisationsCommand _command;
        private readonly EpaoResponse _epaoResponse;

        public UpdateCacheOfAssessmentOrganisationsCommandHandlerTestsFixture()
        {
            _epaoResponse = new EpaoResponse
            {
                Epaos = new List<OrganisationSummary>
                {
                    // These should be out of order
                    new() { Id = "EPA0001", Name = "Test Name 1" },
                    new() { Id = "EPA0003", Name = "Test Name 3" },
                    new() { Id = "EPA0002", Name = "Test Name 2" },
                }
            };

            var approvalOuterApi = new Mock<IApprovalsOuterApiClient>();
            
            approvalOuterApi.Setup(x => x.Get<EpaoResponse>(It.IsAny<GetEpaoOrganisationsRequest>())).ReturnsAsync(_epaoResponse);

            _command = new UpdateCacheOfAssessmentOrganisationsCommand();

            _db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            _sut = new UpdateCacheOfAssessmentOrganisationsCommandHandler(approvalOuterApi.Object, new Lazy<ProviderCommitmentsDbContext>(() => _db), Mock.Of<ILogger<UpdateCacheOfAssessmentOrganisationsCommandHandler>>());
        }

        public async Task AddExistingEpaosToDatabase()
        {
            var assessmentOrganisationsToAdd = _epaoResponse.Epaos
                .Select(os => new AssessmentOrganisation { EpaOrgId = os.Id, Name = os.Name })
                .ToList();
            
            await _db.AssessmentOrganisations.AddRangeAsync(assessmentOrganisationsToAdd);

            await _db.SaveChangesAsync();
        }

        public void AddNewOrgToResponse()
        {
            var existingEpaos = _epaoResponse.Epaos.ToList();
            existingEpaos.Add(new OrganisationSummary { Id = "EPA0004", Name = "Test Name 4" });
            
            _epaoResponse.Epaos = existingEpaos;
        }

        public async Task Handle()
        {
            await _sut.Handle(_command, CancellationToken.None);
        }

        internal void VerifyOrganisationsAdded()
        {
            var organisationsInDb = _db.AssessmentOrganisations.ToList();
            organisationsInDb.Count.Should().Be(3);
            
            foreach (var org in _epaoResponse.Epaos)
            {
                var organisation = organisationsInDb.FirstOrDefault(x => x.EpaOrgId == org.Id && x.Name == org.Name);
                organisation.Should().NotBeNull();
            }
        }

        internal void VerifyOrganisationsUpdated()
        {
            var organisationsInDb = _db.AssessmentOrganisations.ToList();
            organisationsInDb.Count.Should().Be(4);
            
            foreach (var org in _epaoResponse.Epaos)
            {
                var organisation = organisationsInDb.FirstOrDefault(x => x.EpaOrgId == org.Id && x.Name == org.Name);
                organisation.Should().NotBeNull();
            }
        }

        public void Dispose()
        {
            _db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
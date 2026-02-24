using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Exceptions;
using SFA.DAS.CommitmentsV2.Mapping.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Testing.Builders;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Mapping.CocApprovals
{

    [TestFixture]
    [Parallelizable]
    public class CocApprovalRequestToCocApprovalCommandMapperTests
    {
        private CocApprovalRequestToCocApprovalCommandMapperTestsFixture _fixture;

        [SetUp]
        public void Arrange()
        {
            _fixture = new CocApprovalRequestToCocApprovalCommandMapperTestsFixture();
        }

        [TearDown]
        public void TearDown()
        {
            _fixture?.Dispose();
        }

        [Test]
        public async Task ShouldMapCoreData_WhenValuesAreValid()
        {
            // Arrange
            _fixture.SeedData();

            var command = await _fixture.Mapper.Map(_fixture.Request);

            // Assert
            command.Should().NotBeNull();
            command.LearningKey.Should().Be(_fixture.Request.LearningKey);
            command.ApprenticeshipId.Should().Be(_fixture.Request.ApprenticeshipId);
            command.LearningType.ToString().Should().Be(_fixture.Request.LearningType);
            command.ProviderId.ToString().Should().Be(_fixture.Request.UKPRN);
            command.ULN.Should().Be(_fixture.Request.ULN);
        }

        [Test]
        public async Task ShouldMapChangesData_WhenValuesAreValid()
        {
            // Arrange
            _fixture.SeedData();

            _fixture.AddFieldChange("TNP1", "100", "15")
                .AddFieldChange("TNP2", "20", "25");

            var command = await _fixture.Mapper.Map(_fixture.Request);

            // Assert
            command.Should().NotBeNull();
            command.Updates.TNP1.Old.ToString().Should().Be("100");
            command.Updates.TNP1.New.ToString().Should().Be("15");
            command.Updates.TNP2.Old.ToString().Should().Be("20");
            command.Updates.TNP2.New.ToString().Should().Be("25");
            command.ApprovalFieldChanges.Should().BeEquivalentTo(_fixture.Request.Changes);
        }

        [Test]
        public async Task ShouldThrowException_WhenOldValuesAreNotValid()
        {
            // Arrange
            _fixture.SeedData();

            _fixture.AddFieldChange("TNP1", "X10X0", "15");

            var act = async () => await _fixture.Mapper.Map(_fixture.Request);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Test]
        public async Task ShouldThrowException_WhenNewValuesAreNotValid()
        {
            // Arrange
            _fixture.SeedData();

            _fixture.AddFieldChange("TNP1", "100", "1-t5");

            var act = async () => await _fixture.Mapper.Map(_fixture.Request);

            await act.Should().ThrowAsync<DomainException>();
        }

        [Test]
        public async Task ShouldReturnNullApprenticeship_WhenApprenticeshipNotFound()
        {
            var command = await _fixture.Mapper.Map(_fixture.Request);
            command.Apprenticeship.Should().BeNull();
        }

        [Test]
        public async Task ShouldReturnApprenticeship_WhenApprenticeshipFound()
        {
            // Arrange
            _fixture.SeedData();

            var command = await _fixture.Mapper.Map(_fixture.Request);
            command.Apprenticeship.Should().NotBeNull();
            command.Apprenticeship.Should().Be(_fixture.ApprenticeshipFromDb);
        }
    }

    public class CocApprovalRequestToCocApprovalCommandMapperTestsFixture : IDisposable
    {
        public const long ApprenticeshipId = 12;
        public const long ProviderId = 123456;
        public const long ULN = 1234567890;

        public Fixture AutoFixture { get; set; }
        public CocApprovalRequest Request { get; set; }
        public List<CocApprovalFieldChange> FieldChanges { get; set; } = new();
        public ProviderCommitmentsDbContext Db { get; set; }
        public UnitOfWorkContext UnitOfWorkContext { get; set; }
        public CocApprovalRequestToCocApprovalCommandMapper Mapper { get; set; }
        public Apprenticeship ApprenticeshipFromDb => Db.Apprenticeships.First(x => x.Id == ApprenticeshipId);

        public CocApprovalRequestToCocApprovalCommandMapperTestsFixture()
        {
            AutoFixture = new Fixture();
            AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
            AutoFixture.Customizations.Add(new ModelSpecimenBuilder());

            UnitOfWorkContext = new UnitOfWorkContext();

            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false))
                .Options);

            Request = AutoFixture.Build<CocApprovalRequest>()
                .With(x => x.ApprenticeshipId, ApprenticeshipId)
                .With(x => x.LearningType, "Apprenticeship")
                .With(x => x.UKPRN, ProviderId.ToString())
                .With(x => x.ULN, ULN.ToString())
                .With(x => x.Changes, FieldChanges)
                .Create();

            Mapper = new CocApprovalRequestToCocApprovalCommandMapper(new Lazy<ProviderCommitmentsDbContext>(Db), Mock.Of<ILogger<CocApprovalRequestToCocApprovalCommandMapper>>());
        }

        public CocApprovalRequestToCocApprovalCommandMapperTestsFixture SeedData()
        {
            var accountLegalEntityDetails = new AccountLegalEntity()
                .Set(c => c.Id, 444);

            Db.AccountLegalEntities.Add(accountLegalEntityDetails);

            var cohortDetails = new Cohort()
                .Set(c => c.Id, 111)
                .Set(c => c.EmployerAccountId, 222)
                .Set(c => c.ProviderId, 333)
                .Set(c => c.AccountLegalEntityId, accountLegalEntityDetails.Id);

            Db.Cohorts.Add(cohortDetails);

            var apprenticeshipDetails = AutoFixture.Build<Apprenticeship>()
             .With(s => s.Id, ApprenticeshipId)
             .With(s => s.CommitmentId, cohortDetails.Id)
             .With(s => s.ProgrammeType, ProgrammeType.Standard)
             .With(s => s.PaymentStatus, PaymentStatus.Completed)
             .With(s => s.EndDate, DateTime.UtcNow)
             .With(s => s.CompletionDate, DateTime.UtcNow.AddDays(10))
             .With(s => s.StartDate, DateTime.UtcNow.AddDays(-10))
             .Without(s => s.Cohort)
             .Without(s => s.PriceHistory)
             .Without(s => s.ApprenticeshipUpdate)
             .Without(s => s.DataLockStatus)
             .Without(s => s.EpaOrg)
             .Without(s => s.Continuation)
             .Without(s => s.PreviousApprenticeship)
             .Create();

            Db.Apprenticeships.Add(apprenticeshipDetails);
            Db.SaveChanges();

            return this;
        }

        public CocApprovalRequestToCocApprovalCommandMapperTestsFixture AddFieldChange(string fieldName, string oldValue, string newValue)
        {
            FieldChanges.Add(new CocApprovalFieldChange
            {
                ChangeType = fieldName,
                Data = new CocData
                {
                    Old = oldValue,
                    New = newValue
                }
            });
            return this;
        }

        public void Dispose()
        {
            Db?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
using SFA.DAS.CommitmentsV2.Application.Queries.GetDraftApprenticeship;
using SFA.DAS.CommitmentsV2.Authentication;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.Authorization.Features.Models;
using SFA.DAS.Authorization.Features.Services;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetDraftApprentice
{
    [TestFixture]
    [Parallelizable]
    public class GetDraftApprenticeHandlerTests
    {
        [TestCase(Party.Employer, "EMPREF123", Party.Employer, "EMPREF123")]
        [TestCase(Party.Employer, "EMPREF123", Party.Provider, null)]
        [TestCase(Party.Provider, "PROVREF123", Party.Provider, "PROVREF123")]
        [TestCase(Party.Provider, "PROVREF123", Party.Employer, null)]
        public async Task Handle_WhenRequested_ThenShouldReturnCorrectReferenceForRequester(Party creatingParty, string creatingReference, Party requestingParty, string expectedReference)
        {
            var fixture = new GetDraftApprenticeHandlerTestFixtures()
                .SetApprentice(creatingParty, creatingReference)
                .SetRequestingParty(requestingParty);

            var result = await fixture.Handle(); 
            
            Assert.That(result.Reference, Is.EqualTo(expectedReference));
            result.HasStandardOptions.Should().BeFalse();
        }

        [Test]
        public async Task Then_If_There_Options_With_The_Standard_Property_Is_True()
        {
            var fixture = new GetDraftApprenticeHandlerTestFixtures()
                .SetApprentice(Party.Employer, "EMPREF123", true)
                .SetRequestingParty(Party.Employer);

            var result = await fixture.Handle();

            result.HasStandardOptions.Should().BeTrue();
        }

        [Test]
        public async Task Then_If_There_is_prior_learning_return_values_and_status()
        {
            var fixture = new GetDraftApprenticeHandlerTestFixtures()
                .SetApprentice(Party.Employer, "EMPREF123")
                .SetApprenticeshipPriorLearning();

            var result = await fixture.Handle();

            result.DurationReducedBy.Should().Be(fixture.PriorLearning.DurationReducedBy);
            result.PriceReducedBy.Should().Be(fixture.PriorLearning.PriceReducedBy);

            result.DurationReducedByHours.Should().Be(fixture.PriorLearning.DurationReducedByHours);

            result.RecognisePriorLearning.Should().BeTrue();

            var draftApprenticeship = fixture.GetDraftApprenticeship();
            result.RecognisingPriorLearningExtendedStillNeedsToBeConsidered.Should().Be(draftApprenticeship.RecognisingPriorLearningExtendedStillNeedsToBeConsidered);
        }

        [Test]
        public async Task Then_If_There_is_flexible_employment_return_values()
        {
            var fixture = new GetDraftApprenticeHandlerTestFixtures()
                .SetApprentice(Party.Employer, "EMPREF123")
                .SetApprenticeshipFlexiJob();

            var result = await fixture.Handle();

            result.DeliveryModel.Should().Be(DeliveryModel.PortableFlexiJob);
            result.EmploymentEndDate.Should().Be(fixture.FlexibleEmployment.EmploymentEndDate);
            result.EmploymentPrice.Should().Be(fixture.FlexibleEmployment.EmploymentPrice);
        }
    }

    public class GetDraftApprenticeHandlerTestFixtures
    {
        public ProviderCommitmentsDbContext Db { get; set; }
        public Mock<IAuthenticationService> AuthenticationServiceMock { get; set; }
        public Mock<IFeatureTogglesService<FeatureToggle>> FeatureToggleServiceMock { get; set; }
        public GetDraftApprenticeshipQueryHandler Handler { get; set; }
        public ApprenticeshipPriorLearning PriorLearning { get; set; }
        public FlexibleEmployment FlexibleEmployment { get; set; }

        private long CohortId = 1;
        private long ApprenticeshipId = 1;

        public GetDraftApprenticeHandlerTestFixtures()
        {
            AuthenticationServiceMock = new Mock<IAuthenticationService>();
            FeatureToggleServiceMock = new Mock<IFeatureTogglesService<FeatureToggle>>();
            Db = new ProviderCommitmentsDbContext(new DbContextOptionsBuilder<ProviderCommitmentsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString(), b => b.EnableNullChecks(false)).Options);
            Handler = new GetDraftApprenticeshipQueryHandler(
                new Lazy<ProviderCommitmentsDbContext>(() => Db), 
                AuthenticationServiceMock.Object);

            PriorLearning = new ApprenticeshipPriorLearning {DurationReducedBy = 10, PriceReducedBy = 999, DurationReducedByHours = 9 };
            FlexibleEmployment = new FlexibleEmployment {EmploymentEndDate = DateTime.Today, EmploymentPrice = 987};
        }

        public Task<GetDraftApprenticeshipQueryResult> Handle()
        {
            var query = new GetDraftApprenticeshipQuery(CohortId, ApprenticeshipId);
            return Handler.Handle(query, CancellationToken.None);
        }

        public GetDraftApprenticeHandlerTestFixtures SetRequestingParty(Party requestingParty)
        {
            AuthenticationServiceMock
                .Setup(a => a.GetUserParty())
                .Returns(requestingParty);

            return this;
        }

        public GetDraftApprenticeHandlerTestFixtures SetApprentice(Party creatingParty, string reference, bool hasOptions = false, DateTime? startDate = null)
        {
            // This line is required.
            // ReSharper disable once ObjectCreationAsStatement
            new UnitOfWorkContext();

            var autoFixture = new Fixture();

            var draftApprenticeshipDetails = new DraftApprenticeshipDetails
            {
                Reference = reference,
                Id = ApprenticeshipId,
                FirstName = "AFirstName",
                LastName = "ALastName",
                DeliveryModel = DeliveryModel.Regular,
                StartDate = startDate,
                IsOnFlexiPaymentPilot = false
            };

            if (hasOptions)
            {
                draftApprenticeshipDetails.StandardUId = "ST1.01";

                Db.StandardOptions.Add(new StandardOption
                {
                    StandardUId = "ST1.01",
                    Option = "An option"
                });
            }

            var commitment = new Cohort(
                autoFixture.Create<long>(),
                autoFixture.Create<long>(),
                autoFixture.Create<long>(),
                null,
                null,
                draftApprenticeshipDetails,
                creatingParty,
                new UserInfo());

            Db.Cohorts.Add(commitment);

            Db.SaveChanges();

            ApprenticeshipId = commitment.Apprenticeships.First().Id;
            
            CohortId = commitment.Id;

            return this;
        }

        public GetDraftApprenticeHandlerTestFixtures SetApprenticeshipPriorLearning()
        {
            var apprenticeship = Db.DraftApprenticeships.First();
            apprenticeship.RecognisePriorLearning = true;
            apprenticeship.PriorLearning = PriorLearning;

            Db.SaveChanges();

            return this;
        }

        public GetDraftApprenticeHandlerTestFixtures SetApprenticeshipFlexiJob()
        {
            var apprenticeship = Db.DraftApprenticeships.First();
            apprenticeship.FlexibleEmployment = FlexibleEmployment;
            apprenticeship.DeliveryModel = DeliveryModel.PortableFlexiJob;

            Db.SaveChanges();

            return this;
        }

        public DraftApprenticeship GetDraftApprenticeship()
        {
            return Db.DraftApprenticeships.First();
        }
    }
}